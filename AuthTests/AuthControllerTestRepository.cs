using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace JinCreek.Server.AuthTests
{
    public class AuthControllerTestRepository
    {
        private MainDbContext _mainDbContext;
        private RadiusDbContext _radiusDbContext;

        private SimGroup _simGroup;
        private DeviceGroup _deviceGroup;
        private Domain _domain;
        private UserGroup _userGroup;

        public AuthControllerTestRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext)
        {
            _mainDbContext = mainDbContext;
            _radiusDbContext = radiusDbContext;
        }

        public void SetUpInsertBaseData()
        {
            SetUpInsertBaseDataForMainDb();
            SetUpInsertBaseDataForRadiusDb();
        }

        private void SetUpInsertBaseDataForMainDb()
        {
            var organization = new Organization
            {
                Name = "OrganizationName1",
                Address = "OrganizationAddress1",
                DelegatePhone = "123465789",
                AdminPhone = "987654321",
                AdminMail = "Organization1@xx.com",
                StartDay = DateTime.Now,
                EndDay = DateTime.Now,
                Url = "Organization1.co.jp",
                IsValid = true,
            };
            _deviceGroup = new DeviceGroup
            {
                Version = "1.1",
                OsType = "Window10",
                Organization = organization
            };
            _simGroup = new SimGroup
            {
                SimGroupName = "SimGroup1",
                Organization = organization,
                PrimaryDns = "255.0.0.0",
                SecondDns = "255.0.0.1",
                Apn = "SimGroupApn1",
                NasAddress = "NasAddress",
                Nw1AddressPool = "Nw1AddressPool",
                Nw1AddressRange = "Nw1AddressRange",
                ServerAddress = "127.0.0.1"
            };
            _domain = new Domain
            {
                DomainName = "Domain1",
                Organization = organization,
            };
            _userGroup = new UserGroup
            {
                Domain = _domain,
                UserGroupName = "UserGroup1"
            };


            _mainDbContext.Organization.Add(organization);
            _mainDbContext.DeviceGroup.Add(_deviceGroup);
            _mainDbContext.SimGroup.Add(_simGroup);
            _mainDbContext.Domain.Add(_domain);
            _mainDbContext.UserGroup.Add(_userGroup);
            _mainDbContext.SaveChanges();
        }

        private void SetUpInsertBaseDataForRadiusDb()
        {
            var radreply = new Radreply
            {
                Username = "user1",
                Op = "=",
                Attribute = "RadreplyAttribute1",
                Value = "Nw1Address"
            };
            _radiusDbContext.Radreply.Add(radreply);
            _radiusDbContext.SaveChanges();
        }

        //////////////////////////////
        //// MainDbContext
        public SimDeviceAuthenticationStateDone GetSimDeviceAuthenticationStateDone(Guid id)
        {
            return _mainDbContext.SimDeviceAuthenticationStateDone
                .Include(sd => sd.SimDevice)
                .Include(sd => sd.SimDevice.Sim)
                .FirstOrDefault(sd => sd.Id == id);
        }

        //////////////////////////////
        //// RadiusDbContext
        public Radreply GetRadreply(string username)
        {
            return _radiusDbContext.Radreply
                .AsNoTracking()
                .Where(r => r.Username == username).FirstOrDefault();
        }

        //////////////////////////////
        //// Setup methods
        private SimDevice SetUpInsertDataForCase13()
        {
            // Device
            var lte = new Lte
            {
                LteName = "Lte1",
                LteAdapter = "LteAdapter1",
                SoftwareRadioState = true
            };

            var device = new AdDevice
            {
                DeviceName = "Device1",
                DeviceImei = "352555093320000",
                ManagerNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            _mainDbContext.Lte.Add(lte);
            _mainDbContext.AdDevice.Add(device);


            // Sim
            var sim = new Sim
            {
                Msisdn = "02017911000",
                Imsi = "440103213100000",
                IccId = "8981100005819480000",
                SimGroup = _simGroup,
                Password = "123456",
                UserName = "user1"
            };
            _mainDbContext.Sim.Add(sim);

            // authentication
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2AddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
            var simDeviceAuthenticationStateDone = new SimDeviceAuthenticationStateDone
            {
                SimDevice = simDevice,
                TimeLimit = DateTime.Now.AddHours(1.00)
            };
            _mainDbContext.SimDevice.Add(simDevice);
            _mainDbContext.SimDeviceAuthenticationStateDone.Add(simDeviceAuthenticationStateDone);

            _mainDbContext.SaveChanges();
            return simDevice;
        }

        public void SetUpInsertDataForCase131()
        {
            var simDevice = SetUpInsertDataForCase13();
        }

        public void SetUpInsertDataForCase132()
        {
            var simDevice = SetUpInsertDataForCase13();

            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "管理人",
                FirstName = "一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "一般",
                FirstName = "次郎",
                AccountName = "AccountUser2"
            };
            _mainDbContext.AdminUser.Add(admin);
            _mainDbContext.GeneralUser.Add(general);


            var factorCombination = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = admin,
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(-6.00),
                NwAddress = "NwAddress",
            };
            var factorCombination2 = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = general,
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(-6.00),
                NwAddress = "NwAddress"
            };
            _mainDbContext.FactorCombination.Add(factorCombination);
            _mainDbContext.FactorCombination.Add(factorCombination2);
            _mainDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase133()
        {
            var simDevice = SetUpInsertDataForCase13();

            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "管理人",
                FirstName = "一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                LastName = "一般",
                FirstName = "次郎",
                AccountName = "AccountUser2"
            };
            _mainDbContext.AdminUser.Add(admin);
            _mainDbContext.GeneralUser.Add(general);

            var factorCombination = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = admin,
                StartDay = DateTime.Now.AddHours(6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                NwAddress = "NwAddress",
            };
            var factorCombination2 = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = general,
                StartDay = DateTime.Now.AddHours(6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                NwAddress = "NwAddress"
            };
            _mainDbContext.FactorCombination.Add(factorCombination);
            _mainDbContext.FactorCombination.Add(factorCombination2);
            _mainDbContext.SaveChanges();
        }
    }
}
