using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Batch.AdSynchronizer
{
    class AdDeviceSynchronizer : AdSynchronizerBase
    {
        private readonly Domain _domain;

        private readonly DeviceGroup _deviceGroup;
        private bool DEFAULT_USE_TPM = false;
        private int? DEFAULT_WINDOWS_SIGNIN_LIST_CACHE_DAYS = 3;

        public AdDeviceSynchronizer(ILogger<Deauthentication> logger, UserRepository userRepository, AuthenticationRepository authenticationRepository, int organizaionCode, Domain domain, DeviceGroup deviceGroup, IEnumerable<IActiveDirectorySynchronizable> dbs, IEnumerable<ILdap> ldaps) : base(logger, userRepository, authenticationRepository, organizaionCode, dbs, ldaps)
        {
            this._domain = domain;
            this._deviceGroup = deviceGroup;
        }

        protected override void ProcessLdapOnly(List<ILdap> ldapList)
        {
            List<Device> devices = ldapList.Select(ldap =>
            {
                var device = new Device()
                {
                    Domain = _domain,
                    Name = ldap.Name,
                    UseTpm = DEFAULT_USE_TPM,
                    WindowsSignInListCacheDays = DEFAULT_WINDOWS_SIGNIN_LIST_CACHE_DAYS,
                    StartDate = DateTime.Now,
                    EndDate = null,
                    OrganizationClientApp = null,
                    AdObjectId = ldap.ObjectGuid
                };
                return device;
            }).ToList();

            UserRepository.Create(_deviceGroup, devices.ToArray());
        }

        protected override void ProcessSame(List<(ILdap, IActiveDirectorySynchronizable)> ldapDbTupleList)
        {
            Logger.LogDebug($"DEVICE SAME {string.Join(",", ldapDbTupleList.Select(r => r.Item1.ObjectGuid).ToList())}");
            IEnumerable<Device> devices = ldapDbTupleList.Select(ldapDbTuple =>
            {
                var (ldap, db) = ldapDbTuple;
                var device = (Device)db;

                device.Domain = _domain;
                device.Name = ldap.Name;
                device.AdObjectId = ldap.ObjectGuid;
                if (device.EndDate < DateTime.Now)
                {
                    device.UseTpm = DEFAULT_USE_TPM;
                    device.WindowsSignInListCacheDays = DEFAULT_WINDOWS_SIGNIN_LIST_CACHE_DAYS;
                    device.StartDate = DateTime.Now;
                    device.EndDate = null;
                }
                return device;
            });

            UserRepository.Update(devices.ToArray());
        }

        protected override void ProcessDbOnly(List<IActiveDirectorySynchronizable> dbList)
        {
            Logger.LogDebug($"DEVICE DBONLY {string.Join(",", dbList.Select(r => r.AdObjectId).ToArray())}");
            UserRepository.RemoveLogicallyDevices(dbList.Select(r => r.AdObjectId).ToArray());
        }
    }
}
