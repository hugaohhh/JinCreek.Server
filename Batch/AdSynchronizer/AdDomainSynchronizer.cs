using System.Collections.Generic;
using System.Linq;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Logging;

namespace JinCreek.Server.Batch.AdSynchronizer
{
    class AdDomainSynchronizer : AdSynchronizerBase
    {
        public AdDomainSynchronizer(ILogger<Deauthentication> logger, UserRepository userRepository, AuthenticationRepository authenticationRepository, int organizaionCode, IEnumerable<IActiveDirectorySynchronizable> dbs, IEnumerable<ILdap> ldaps) : base(logger, userRepository, authenticationRepository, organizaionCode, dbs, ldaps)
        {
        }

        protected override void ProcessLdapOnly(List<ILdap> ldapList)
        {
            Logger.LogDebug($"DOMAIN LDAPONLY {string.Join(",", ldapList.Select(r => r.ObjectGuid).ToList())}");
            foreach (ILdap ldap in ldapList)
            {
                var domain = new Domain()
                {
                    OrganizationCode = OrganizaionCode,
                    Name = ldap.Name,
                    AdObjectId = ldap.ObjectGuid
                };
                UserRepository.Create(domain);
            }
        }

        protected override void ProcessSame(List<(ILdap, IActiveDirectorySynchronizable)> ldapDbTupleList)
        {
            Logger.LogDebug($"DOMAIN SAME {string.Join(",", ldapDbTupleList.Select(r => r.Item1.ObjectGuid).ToList())}");
            foreach (var (ldap, db) in ldapDbTupleList)
            {
                var domain = (Domain)db;

                //var domain = domains.Where(r => r.AdObjectId == ldapDomain.ObjectGuid).First();
                domain.OrganizationCode = OrganizaionCode;
                domain.Name = ldap.Name;
                domain.AdObjectId = ldap.ObjectGuid;
                UserRepository.Update(domain);
            }
        }

        protected override void ProcessDbOnly(List<IActiveDirectorySynchronizable> dbList)
        {
            Logger.LogDebug($"DOMAIN " +
                            $"DBONLY {string.Join(",", dbList.Select(r => r.AdObjectId).ToList())}");
            // 論理削除対象外
        }
    }
}
