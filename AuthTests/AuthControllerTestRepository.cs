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
        private Lte _lte;

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
                Os = "Window10",
                Organization = organization
            };
            _simGroup = new SimGroup
            {
                SimGroupName = "SimGroup1",
                Organization = organization,
                PrimaryDns = "255.0.0.0",
                SecondaryDns = "255.0.0.1",
                Apn = "SimGroupApn1",
                NasIpAddress = "NasAddress",
                Nw1IpAddressPool = "Nw1AddressPool",
                Nw1IpAddressRange = "Nw1AddressRange",
                AuthServerIpAddress = "127.0.0.1",
                Nw1PrimaryDns = "255.0.0.0",
                Nw1SecondaryDns = "255.0.0.0"
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
            _lte = new Lte
            {
                LteName = "Lte1",
                NwAdapterName = "LteAdapter1",
                SoftwareRadioState = true
            };
            _mainDbContext.AddRange(organization,_deviceGroup,_domain,_userGroup,_lte);
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
                .AsNoTracking()
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
            var device = new AdDevice
            {
                DeviceName = "Device1",
                Imei = "352555093320000",
                ManageNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            _mainDbContext.Device.Add(device);

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
                Nw2IpAddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
            var simDeviceAuthenticationStateDone = new SimDeviceAuthenticationStateDone
            {
                SimDevice = simDevice,
                TimeLimit = DateTime.Now.AddHours(-1.00)
            };
            _mainDbContext.SimDevice.Add(simDevice);
            _mainDbContext.SimDeviceAuthenticationStateDone.Add(simDeviceAuthenticationStateDone);

            _mainDbContext.SaveChanges();
            return simDevice;
        }

        public void SetUpInsertDataForCase131()
        {
            SetUpInsertDataForCase13();
        }
        public void CreateUser2(SimDevice simDevice)
        {
            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                Name = "管理人一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                Name = "一般次郎",
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
                NwIpAddress = "NwAddress",
            };
            var factorCombination2 = new FactorCombination
            {

                SimDevice = simDevice,
                EndUser = general,
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(-6.00),
                NwIpAddress = "NwAddress"
            };
            _mainDbContext.FactorCombination.Add(factorCombination);
            _mainDbContext.FactorCombination.Add(factorCombination2);
            _mainDbContext.SaveChanges();
        }
        public void SetUpInsertDataForCase132()
        {
            var simDevice = SetUpInsertDataForCase13();
            CreateUser2(simDevice);
        }

        public void CreateUser3(SimDevice simDevice)
        {
            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                Name = "管理人一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                Name = "一般次郎",
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
                NwIpAddress = "NwAddress",
            };
            var factorCombination2 = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = general,
                StartDay = DateTime.Now.AddHours(6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                NwIpAddress = "NwAddress"
            };
            _mainDbContext.FactorCombination.Add(factorCombination);
            _mainDbContext.FactorCombination.Add(factorCombination2);
            _mainDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase133()
        {
            var simDevice = SetUpInsertDataForCase13();
            CreateUser3(simDevice);
        }

   

        public void SetUpInsertDataForCase15()
        {
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
            _mainDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase16()
        {
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
            var device = new AdDevice
            {
                DeviceName = "Device1",
                Imei = "352555093320000",
                ManageNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2IpAddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
            _mainDbContext.AddRange(sim,device,simDevice);
            _mainDbContext.SaveChanges();
        }
        public void SetUpInsertDataForCase17()
        {
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
            var device = new AdDevice
            {
                DeviceName = "Device1",
                Imei = "352555093320000",
                ManageNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2IpAddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(-6.00),
                AuthPeriod = 1
            };
            _mainDbContext.AddRange(sim, device, simDevice);
            _mainDbContext.SaveChanges();
        }
        public SimDevice SetUpInsertDataForCase18()
        {
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
            var device = new AdDevice
            {
                DeviceName = "Device1",
                Imei = "352555093320000",
                ManageNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2IpAddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                AuthPeriod = 1
            };
            _mainDbContext.AddRange(sim, device, simDevice);
            _mainDbContext.SaveChanges();
            return simDevice;
        }

        public void SetUpInsertDataForCase181()
        {
            SetUpInsertDataForCase18();
        }
        public void SetUpInsertDataForCase182()
        {
            var simDevice = SetUpInsertDataForCase18();
            CreateUser2(simDevice);
        }
        public void SetUpInsertDataForCase183()
        {
            var simDevice = SetUpInsertDataForCase18();
            CreateUser3(simDevice);
        }

        public SimDevice SetUpInsertDataForCase19()
        {
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
            var device = new AdDevice
            {
                DeviceName = "Device1",
                Imei = "352555093320000",
                ManageNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2IpAddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
            _mainDbContext.AddRange(sim, device, simDevice);
            _mainDbContext.SaveChanges();
            return simDevice;
        }

        public void SetUpInsertDataForCase191()
        {
            SetUpInsertDataForCase19();
        }
        public void SetUpInsertDataForCase192()
        {
            var simDevice = SetUpInsertDataForCase18();
            CreateUser2(simDevice);
        }
        public void SetUpInsertDataForCase193()
        {
            var simDevice = SetUpInsertDataForCase18();
            CreateUser3(simDevice);
        }

        public SimDevice SetUpInsertDataForCase20()
        {
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
            var device = new AdDevice
            {
                DeviceName = "Device1",
                Imei = "352555093320000",
                ManageNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var adDeviceSettingOfflineWindowsSignIn = new AdDeviceSettingOfflineWindowsSignIn
            {
                AdDevice = device,
                WindowsSignInListCacheDays = 1
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2IpAddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1
            };
            _mainDbContext.AddRange(sim, device, simDevice,adDeviceSettingOfflineWindowsSignIn);
            _mainDbContext.SaveChanges();
            return simDevice;
        }
        public void SetUpInsertDataForCase201()
        {
            SetUpInsertDataForCase19();
        }
        public void SetUpInsertDataForCase202()
        {
            var simDevice = SetUpInsertDataForCase18();
            CreateUser2(simDevice);
        }
        public void SetUpInsertDataForCase203()
        {
            var simDevice = SetUpInsertDataForCase18();
            CreateUser3(simDevice);
        }

        public void CreateUser5(SimDevice simDevice)
        {
            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                Name = "管理人一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                Name = "一般一郎",
                AccountName = "AccountUser2"
            };
            var factorCombination = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = admin,
                StartDay = DateTime.Now.AddHours(-6.00),
                NwIpAddress = "NwAddress"
            };
            _mainDbContext.AddRange(general,admin,factorCombination);
            _mainDbContext.SaveChanges();
        }
        public void CreateUser6(SimDevice simDevice)
        {
            var admin = new AdminUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                Name = "管理人一郎",
                Password = "password",
                AccountName = "AccountUser1"
            };
            var general = new GeneralUser
            {
                Domain = _userGroup.Domain,
                UserGroup = _userGroup,
                Name = "一般一郎",
                AccountName = "AccountUser2"
            };
            var factorCombination = new FactorCombination
            {
                SimDevice = simDevice,
                EndUser = admin,
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                NwIpAddress = "NwAddress"
            };
            _mainDbContext.AddRange(general, admin, factorCombination);
            _mainDbContext.SaveChanges();
        }
        public SimDevice SetUpInsertDataForCase21()
        {
            var device = new AdDevice
            {
                DeviceName = "Device1",
                Imei = "352555093320000",
                ManageNumber = "DeviceManager1",
                Type = "DeviceType1",
                Lte = _lte,
                DeviceGroup = _deviceGroup,
                Domain = _domain
            };
            var sim = new Sim
            {
                Msisdn = "02017911000",
                Imsi = "440103213100000",
                IccId = "8981100005819480000",
                SimGroup = _simGroup,
                Password = "123456",
                UserName = "user1"
            };
            var simDevice = new SimDevice
            {
                Sim = sim,
                Device = device,
                Nw2IpAddressPool = "Nw2Address",
                StartDay = DateTime.Now.AddHours(-6.00),
                EndDay = DateTime.Now.AddHours(6.00),
                AuthPeriod = 1,
            };
            var adDeviceSettingOfflineWindowsSignIn = new AdDeviceSettingOfflineWindowsSignIn
            {
                AdDevice = device,
                WindowsSignInListCacheDays = 1
            };
            _mainDbContext.AddRange(sim,device, simDevice, adDeviceSettingOfflineWindowsSignIn);
            _mainDbContext.SaveChanges();
            return simDevice;
        }
        public void SetUpInsertDataForCase211()
        {
            var simDevice = SetUpInsertDataForCase21();
            CreateUser5(simDevice);
        }
        public void SetUpInsertDataForCase212()
        {
            var simDevice = SetUpInsertDataForCase21();
            CreateUser6(simDevice);
        }
    }
}
