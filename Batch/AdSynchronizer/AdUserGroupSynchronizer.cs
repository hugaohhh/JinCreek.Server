using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Batch.AdSynchronizer
{
    class AdUserGroupSynchronizer : AdSynchronizerBase
    {
        private Domain _domain;

        public AdUserGroupSynchronizer(ILogger<Deauthentication> logger, UserRepository userRepository, AuthenticationRepository authenticationRepository, int organizaionCode, Domain domain, IEnumerable<IActiveDirectorySynchronizable> dbs, IEnumerable<ILdap> ldaps) : base(logger, userRepository, authenticationRepository, organizaionCode, dbs, ldaps)
        {
            this._domain = domain;
        }

        protected override void ProcessLdapOnly(List<ILdap> ldapList)
        {
            Logger.LogDebug($"USERGROUP LDAPONLY {string.Join(",", ldapList.Select(r => r.ObjectGuid).ToList())}");
            var userGroups = ldapList.Select(ldap =>
            {
                var userGroup = new UserGroup()
                {
                    Domain = _domain,
                    Name = ldap.Name,
                    AdObjectId = ldap.ObjectGuid
                };
                return userGroup;
            }).ToList();

            UserRepository.Create(userGroups.ToArray());
        }

        protected override void ProcessSame(List<(ILdap, IActiveDirectorySynchronizable)> ldapDbTupleList)
        {
            Logger.LogDebug($"USERGROUP SAME {string.Join(",", ldapDbTupleList.Select(r => r.Item1.ObjectGuid).ToList())}");
            var userGroups = ldapDbTupleList.Select(ldapDbTuple =>
            {
                var (ldap, db) = ldapDbTuple;
                var userGroup = (UserGroup)db;
                userGroup.Domain = _domain;
                userGroup.Name = ldap.Name;
                userGroup.AdObjectId = ldap.ObjectGuid;
                return userGroup;
            });

            UserRepository.Update(userGroups.ToArray());
        }

        protected override void ProcessDbOnly(List<IActiveDirectorySynchronizable> dbList)
        {
            Logger.LogDebug($"USERGROUP DBONLY {string.Join(",", dbList.Select(r => r.AdObjectId).ToList())}");
            // 論理削除対象外
        }
    }
}
