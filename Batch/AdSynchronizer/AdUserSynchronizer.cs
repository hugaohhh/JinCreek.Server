using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Batch.AdSynchronizer
{
    class AdUserSynchronizer : AdSynchronizerBase
    {
        private readonly Domain _domain;

        private readonly UserGroup _userGroup;
        private readonly string DEFAULT_ACCOUNT_NAME = new Random().Next(0, Int32.MaxValue).ToString();

        public AdUserSynchronizer(ILogger<Deauthentication> logger, UserRepository userRepository, AuthenticationRepository authenticationRepository, int organizaionCode, Domain domain, UserGroup userGroup, IEnumerable<IActiveDirectorySynchronizable> dbs, IEnumerable<ILdap> ldaps) : base(logger, userRepository, authenticationRepository, organizaionCode, dbs, ldaps)
        {
            this._domain = domain;
            this._userGroup = userGroup;
        }

        protected override void ProcessLdapOnly(List<ILdap> ldapList)
        {
            Logger.LogDebug($"USER LDAPONLY {string.Join(",", ldapList.Select(r => r.ObjectGuid).ToList())}");
            List<GeneralUser> generalUsers = ldapList.Select(ldap =>
            {
                //TODO 管理者はいない?
                var generalUser = new GeneralUser()
                {
                    Domain = _domain,
                    Name = ldap.Name,
                    AccountName = GetAccountName(((LdapUser)ldap).UserPrincipalName, ldap.Name),
                    AdObjectId = ldap.ObjectGuid
                };
                return generalUser;
            }).ToList();

            UserRepository.Create(_userGroup, generalUsers.ToArray());
        }

        protected override void ProcessSame(List<(ILdap, IActiveDirectorySynchronizable)> ldapDbTupleList)
        {
            Logger.LogDebug($"USER SAME {string.Join(",", ldapDbTupleList.Select(r => r.Item1.ObjectGuid).ToList())}");
            IEnumerable<EndUser> endUsers = ldapDbTupleList.Select(ldapDbTuple =>
            {
                var (ldap, db) = ldapDbTuple;
                var user = (EndUser)db;
                user.Domain = _domain;
                user.Name = ldap.Name;
                user.AccountName = GetAccountName(((LdapUser) ldap).UserPrincipalName, ldap.Name);
                user.AdObjectId = ldap.ObjectGuid;
                return user;
            });

            UserRepository.Update(endUsers.ToArray());
        }

        private string GetAccountName(string userPrincipalName, string name)
        {
            if (string.IsNullOrEmpty(userPrincipalName))
            {
                if (string.IsNullOrEmpty(name))
                {
                    return DEFAULT_ACCOUNT_NAME + DateTime.Now.ToLongDateString();
                }
                else
                {
                    return name;
                }
            }
            return (userPrincipalName.Contains("@"))
                ? userPrincipalName.Split('@')[0] : userPrincipalName;
        }

        protected override void ProcessDbOnly(List<IActiveDirectorySynchronizable> dbList)
        {
            Logger.LogDebug($"USER DBONLY {string.Join(",", dbList.Select(r => r.AdObjectId).ToArray())}");
            UserRepository.RemoveLogicallyEndUsers(dbList.Select(r => r.AdObjectId).ToArray());
        }
    }
}
