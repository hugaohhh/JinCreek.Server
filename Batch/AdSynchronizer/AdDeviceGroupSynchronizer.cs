using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Batch.AdSynchronizer
{
    class AdDeviceGroupSynchronizer : AdSynchronizerBase
    {
        private Domain _domain;

        public AdDeviceGroupSynchronizer(ILogger<Deauthentication> logger, UserRepository userRepository, AuthenticationRepository authenticationRepository, int organizaionCode, Domain domain, IEnumerable<IActiveDirectorySynchronizable> dbs, IEnumerable<ILdap> ldaps) : base(logger, userRepository, authenticationRepository, organizaionCode, dbs, ldaps)
        {
            this._domain = domain;
        }

        protected override void ProcessLdapOnly(List<ILdap> ldapList)
        {
            Logger.LogDebug($"DEVICEGROUP LDAPONLY {string.Join(",", ldapList.Select(r => r.ObjectGuid).ToList())}");
            var deviceGroups = ldapList.Select(ldap =>
            {
                var deviceGroup = new DeviceGroup()
                {
                    Domain = _domain,
                    Name = ldap.Name,
                    AdObjectId = ldap.ObjectGuid
                };
                return deviceGroup;
            }).ToList();

            UserRepository.Create(deviceGroups.ToArray());
        }

        protected override void ProcessSame(List<(ILdap, IActiveDirectorySynchronizable)> ldapDbTupleList)
        {
            Logger.LogDebug($"DEVICEGROUP SAME {string.Join(",", ldapDbTupleList.Select(r => r.Item1.ObjectGuid).ToList())}");
            var deviceGroups = ldapDbTupleList.Select(ldapDbTuple =>
            {
                var (ldap, db) = ldapDbTuple;
                var deviceGroup = (DeviceGroup)db;
                deviceGroup.Domain = _domain;
                deviceGroup.Name = ldap.Name;
                deviceGroup.AdObjectId = ldap.ObjectGuid;
                return deviceGroup;
            });

            UserRepository.Update(deviceGroups.ToArray());
        }

        protected override void ProcessDbOnly(List<IActiveDirectorySynchronizable> dbList)
        {
            Logger.LogDebug($"DEVICEGROUP DBONLY {string.Join(",", dbList.Select(r => r.AdObjectId).ToList())}");
            // 論理削除対象外
        }
    }
}
