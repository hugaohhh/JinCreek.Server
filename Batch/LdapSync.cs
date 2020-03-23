using ConsoleAppFramework;
using JinCreek.Server.Batch.AdSynchronizer;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JinCreek.Server.Batch
{
    class LdapSync : ConsoleAppBase
    {
        // configurationを扱う定型としたいため
        // ReSharper disable once NotAccessedField.Local
        private readonly IConfiguration _configuration;
        private readonly ILogger<Deauthentication> _logger;
        private readonly UserRepository _userRepository;
        private readonly AuthenticationRepository _authenticationRepository;

        public LdapSync(IConfiguration configuration, ILogger<Deauthentication> logger, UserRepository userRepository, AuthenticationRepository authenticationRepository)
        {
            _configuration = configuration;
            //_ldapSyncSettings = ldapSyncSettings;
            _logger = logger;
            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;
        }

        [Command("ldap_sync")]
        public void Main(
            [Option("organization_code", "organization code")]
            int organizationCode
        )
        {
            _logger.LogInformation($"{GetType().FullName} Start");
            try
            {
                var domainSets =
                    _configuration.GetSection("ActiveDirectorySync")
                        .Get<List<OrganizationDomainSet>>()
                        .Where(d => d.OrganizationCode == organizationCode);

                foreach (var organizationDomain in domainSets)
                {
                    _logger.LogDebug($"{organizationDomain.OrganizationCode} {organizationDomain.DomainName}");

                    var ldapContext = new LdapContext(organizationDomain.LdapConfig.Server, organizationDomain.LdapConfig.Port, organizationDomain.LdapConfig.DomainAndUser, organizationDomain.LdapConfig.Password);
                    var ldapRepository = new LdapRepository(ldapContext);

                    DoIt(ldapRepository, organizationDomain.OrganizationCode, organizationDomain.DomainName, organizationDomain.DeviceGroupObjectGuidArray, organizationDomain.UserGroupObjectGuidArray);
                }

                _logger.LogInformation($"{GetType().FullName} Success");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                _logger.LogInformation($"{GetType().FullName} Error");
            }
        }

        private void DoIt(LdapRepository ldapRepository, int organizationCode, string domainName, string[] targetDeviceGroupGuIdArray, string[] targetUserGroupGuIdArray)
        {
            var organization = _userRepository.GetOrganization(organizationCode);
            if (organization.StartDate > DateTime.Now.Date
                || (organization.EndDate != null && organization.EndDate < DateTime.Now.Date))
            {
                _logger.LogWarning($"[{organizationCode}-{domainName}] Organization is not target {organization.Code} {organization.StartDate} {organization.EndDate}");
                return;
            }

            if ((targetDeviceGroupGuIdArray == null || targetDeviceGroupGuIdArray.Length == 0) || (targetUserGroupGuIdArray == null || targetUserGroupGuIdArray.Length == 0))
            {
                if (targetDeviceGroupGuIdArray == null || targetDeviceGroupGuIdArray.Length == 0)
                {
                    _logger.LogWarning($"[{organizationCode}-{domainName}] DeviceGroupObjectGuidArray is empty. Skipped.");
                }
                if (targetUserGroupGuIdArray == null || targetUserGroupGuIdArray.Length == 0)
                {
                    _logger.LogWarning($"[{organizationCode}-{domainName}] UserGroupObjectGuidArray is empty. Skipped.");
                }
                return;
            }

            var ldapDomains = ldapRepository.GetDomain(domainName).ToList();
            var ldapDeviceGroups = ldapRepository.GetDeviceGroupWithDevices(domainName, targetDeviceGroupGuIdArray).ToList();
            var ldapUserGroups = ldapRepository.GetUserGroupWithUsers(domainName, targetUserGroupGuIdArray).ToList();

            var domains = _userRepository.GetDomainsByOrganizationCode(organizationCode).ToList();
            var deviceGroups = _userRepository.GetDeviceGroupByOrganizationCode(organizationCode).ToList();
            var devices = _userRepository.GetDeviceByOrganizationCode(organizationCode).ToList();
            var userGroups = _userRepository.GetUserGroupByOrganizationCode(organizationCode).ToList();
            var users = _userRepository.GetUserByOrganizationCode(organizationCode).ToList();


            new AdDomainSynchronizer(_logger, _userRepository, _authenticationRepository, organizationCode, domains,
                ldapDomains).Synchronize();
            foreach (var ldapDomain
                in ldapDomains.Where(r => (domains.Select(d => d.AdObjectId).ToList()).Contains(r.ObjectGuid)).ToList())
            {
                var targetDomain = domains
                    .Where(r => r.OrganizationCode == organizationCode && r.AdObjectId == ldapDomain.ObjectGuid)
                    .ToList().First();

                new AdUserGroupSynchronizer(_logger, _userRepository, _authenticationRepository, organizationCode,
                    targetDomain, userGroups, ldapUserGroups).Synchronize();
                // 上記で更新した結果を最新取得
                userGroups = _userRepository.GetUserGroupByOrganizationCode(organizationCode).ToList();
                // ユーザーを最新
                foreach (var ldap in ldapUserGroups)
                {
                    var ldapUserGroup = (LdapUserGroup)ldap;
                    UserGroup userGroup = userGroups.Where(r => r.AdObjectId == ldapUserGroup.ObjectGuid).FirstOrDefault();
                    new AdUserSynchronizer(_logger, _userRepository, _authenticationRepository, organizationCode,
                        targetDomain, userGroup, users, ldapUserGroup.UserList).Synchronize();
                }


                new AdDeviceGroupSynchronizer(_logger, _userRepository, _authenticationRepository, organizationCode,
                    targetDomain, deviceGroups, ldapDeviceGroups).Synchronize();
                // 上記で更新した結果を最新取得
                deviceGroups = _userRepository.GetDeviceGroupByOrganizationCode(organizationCode).ToList();
                // 端末を最新
                foreach (var ldap in ldapDeviceGroups)
                {
                    var ldapDeviceGroup = (LdapDeviceGroup)ldap;
                    var deviceGroup = deviceGroups.Where(r => r.AdObjectId == ldapDeviceGroup.ObjectGuid).FirstOrDefault();
                    new AdDeviceSynchronizer(_logger, _userRepository, _authenticationRepository, organizationCode,
                        targetDomain, deviceGroup, devices, ldapDeviceGroup.DeviceList).Synchronize();
                }
            }
        }
    }

    public class OrganizationDomainSet
    {
        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int OrganizationCode { get; set; }
        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string DomainName { get; set; }

        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string[] DeviceGroupObjectGuidArray { get; set; }

        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string[] UserGroupObjectGuidArray { get; set; }

        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public LdapConfig LdapConfig { get; set; }
    }

    public class LdapConfig
    {
        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Server { get; set; }
        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Port { get; set; }
        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string DomainAndUser { get; set; }
        // 設定ファイル読み込みのため
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Password { get; set; }
    }
}
