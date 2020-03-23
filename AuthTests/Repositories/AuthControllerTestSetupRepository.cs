using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using System;

namespace JinCreek.Server.AuthTests.Repositories
{
    public abstract class AuthControllerTestSetupRepository
    {
        private static readonly DateTime AncientDateTime = DateTime.Now.AddDays(-3.00).Date;
        private static readonly DateTime PastDateTime = DateTime.Now.AddDays(-2.00).Date;
        private static readonly DateTime TodayDateTime = DateTime.Now.Date;
        private static readonly DateTime FutureDateTime = DateTime.Now.AddDays(2.00).Date;

        protected static readonly (DateTime, DateTime) CurrentDateTimeForLessThanStart = (FutureDateTime, FutureDateTime);
        protected static readonly (DateTime, DateTime) CurrentDateTimeForLaterEnd = (PastDateTime, PastDateTime);
        protected static readonly (DateTime, DateTime) CurrentDateTimeForStart = (TodayDateTime, FutureDateTime);
        protected static readonly (DateTime, DateTime) CurrentDateTimeForEnd = (PastDateTime, TodayDateTime);

        protected static readonly (DateTime, DateTime) AncientDateTimeForLaterEnd = (AncientDateTime, AncientDateTime);

        protected MainDbContext MainDbContext;
        protected RadiusDbContext RadiusDbContext;

        protected Organization Organization;
        protected SimGroup SimGroup;
        protected DeviceGroup DeviceGroup;
        protected Domain Domain;
        protected UserGroup UserGroup;
        protected LteModule LteModule;
        protected Sim Sim;
        protected Sim SimOther;
        protected Device Device;
        protected Device DeviceOther;

        public AuthControllerTestSetupRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext)
        {
            MainDbContext = mainDbContext;
            RadiusDbContext = radiusDbContext;
        }

        public void SetUpInsertBaseData()
        {
            SetUpInsertBaseDataForMainDb();
            MainDbContext.SaveChanges();

            SetUpInsertBaseDataForRadiusDb();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertBaseDataForMainDb()
        {
            Organization = new Organization
            {
                Name = "OrganizationName1",
                Address = "OrganizationAddress1",
                Phone = "123465789",
                AdminPhone = "987654321",
                AdminMail = "Organization1@xx.com",
                StartDate = CurrentDateTimeForEnd.Item1,
                EndDate = CurrentDateTimeForEnd.Item2,
                Url = "Organization1.co.jp",
                IsValid = true,
                DistributionServerIp = "127.0.0.1"
            };

            SimGroup = new SimGroup
            {
                Name = "SimGroup1",
                Organization = Organization,
                PrimaryDns = "255.0.0.0",
                SecondaryDns = "255.0.0.1",
                Apn = "SimGroupApn1",
                NasIp = "NasAddress",
                IsolatedNw1IpPool = "Nw1AddressPool",
                IsolatedNw1IpRange = "Nw1AddressRange",
                AuthenticationServerIp = "127.0.0.1",
                IsolatedNw1PrimaryDns = "255.0.0.0",
                IsolatedNw1SecondaryDns = "255.0.0.0",
                UserNameSuffix = "jincreek2"
            };

            var simGroup2 = new SimGroup()
            {
                Name = "SimGroup2",
                Organization = Organization,
                PrimaryDns = "255.2.0.0",
                SecondaryDns = "255.2.0.1",
                Apn = "SimGroupApn2",
                NasIp = "NasAddress2",
                IsolatedNw1IpPool = "Nw1AddressPool2",
                IsolatedNw1IpRange = "Nw1AddressRange",
                AuthenticationServerIp = "127.0.0.2",
                IsolatedNw1PrimaryDns = "255.2.0.0",
                IsolatedNw1SecondaryDns = "255.2.0.0",
                UserNameSuffix = "jincreek22"
            };

            Domain = new Domain
            {
                Name = "Domain1",
                Organization = Organization,
            };
            DeviceGroup = new DeviceGroup
            {
                Domain = Domain,
                Name = "DeviceGroupName1"
            };
            UserGroup = new UserGroup
            {
                Domain = Domain,
                Name = "UserGroup1"
            };
            LteModule = new LteModule
            {
                Name = "Lte1",
                NwAdapterName = "LteAdapter1",
                UseSoftwareRadioState = true
            };

            // Device
            Device = new Device
            {
                Name = "JINCREEK-PC",
                SerialNumber = "352555093320000",
                ManagedNumber = "DeviceManager1",
                ProductName = "DeviceType1",
                UseTpm = true,
                LteModule = LteModule,
                WindowsSignInListCacheDays = 1,
                Domain = Domain
            };

            var deviceGroupDevice = new DeviceGroupDevice()
            {
                DeviceGroup = DeviceGroup,
                Device = Device
            };

            DeviceOther = new Device
            {
                Name = "OTHER-PC",
                SerialNumber = "992555093320000",
                ManagedNumber = "OTHER0001",
                ProductName = "DeviceType9",
                UseTpm = true,
                LteModule = LteModule,
                WindowsSignInListCacheDays = 1,
                Domain = Domain
            };

            var deviceGroupDeviceOther = new DeviceGroupDevice()
            {
                DeviceGroup = DeviceGroup,
                Device = DeviceOther
            };


            // Sim
            Sim = new Sim
            {
                Msisdn = "02017911000",
                Imsi = "440103213100000",
                IccId = "8981100005819480000",
                SimGroup = SimGroup,
                Password = "123456",
                UserName = "user1"
            };

            SimOther = new Sim
            {
                Msisdn = "02017912000",
                Imsi = "540103213100000",
                IccId = "9981100005819480000",
                SimGroup = simGroup2,
                Password = "789012",
                UserName = "user2"
            };
            MainDbContext.AddRange(Organization, DeviceGroup, Domain, UserGroup, LteModule, Sim, Device, deviceGroupDevice);
            MainDbContext.AddRange(simGroup2, SimOther);
            MainDbContext.AddRange(DeviceOther, deviceGroupDeviceOther);
        }

        public virtual void SetUpInsertBaseDataForRadiusDb()
        {
            var radreply = new Radreply
            {
                Username = "user1@jincreek2",
                Op = "=",
                Attribute = "Framed-IP-Address",
                Value = "Nw1Address"
            };
            RadiusDbContext.Radreply.Add(radreply);
        }


        protected MultiFactor CreateUser2(SimAndDevice simAndDevice)
        {
            var admin = new UserAdmin
            {
                Domain = UserGroup.Domain,
                Name = "管理人一郎",
                Password = "password",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = "AccountUser1"
            };
            var userGroupEndUserAdmin = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = admin
            };
            var general = new GeneralUser
            {
                Domain = UserGroup.Domain,
                Name = "一般次郎",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = "AccountUser2"
            };
            var userGroupEndUserGeneral = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = general
            };
            MainDbContext.UserAdmin.Add(admin);
            MainDbContext.UserGroupEndUser.Add(userGroupEndUserAdmin);
            MainDbContext.GeneralUser.Add(general);
            MainDbContext.UserGroupEndUser.Add(userGroupEndUserGeneral);
            var factorCombination = new MultiFactor
            {
                SimAndDevice = simAndDevice,
                EndUser = admin,
                StartDate = DateTime.Now.AddHours(6.00),
                EndDate = DateTime.Now.AddHours(6.00),
                ClosedNwIp = "NwAddress"
            };
            var factorCombination2 = new MultiFactor
            {

                SimAndDevice = simAndDevice,
                EndUser = general,
                StartDate = DateTime.Now.AddHours(6.00),
                EndDate = DateTime.Now.AddHours(6.00),
                ClosedNwIp = "NwAddress"
            };
            MainDbContext.MultiFactor.Add(factorCombination);
            MainDbContext.MultiFactor.Add(factorCombination2);
            MainDbContext.SaveChanges();
            return factorCombination;
        }

        protected MultiFactor CreateUser3(SimAndDevice simAndDevice)
        {
            var admin = new UserAdmin
            {
                Domain = UserGroup.Domain,
                Name = "管理人一郎",
                Password = "password",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = "AccountUser1"
            };
            var userGroupEndUserAdmin = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = admin
            };
            var general = new GeneralUser
            {
                Domain = UserGroup.Domain,
                Name = "一般次郎",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = "AccountUser2"
            };
            var userGroupEndUserGeneral = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = general
            };
            MainDbContext.UserAdmin.Add(admin);
            MainDbContext.UserGroupEndUser.Add(userGroupEndUserAdmin);
            MainDbContext.GeneralUser.Add(general);
            MainDbContext.UserGroupEndUser.Add(userGroupEndUserGeneral);
            var factorCombination = new MultiFactor
            {
                SimAndDevice = simAndDevice,
                EndUser = admin,
                StartDate = DateTime.Now.AddHours(-6.00),
                EndDate = DateTime.Now.AddHours(-6.00),
                ClosedNwIp = "NwAddress",
            };
            var factorCombination2 = new MultiFactor
            {
                SimAndDevice = simAndDevice,
                EndUser = general,
                StartDate = DateTime.Now.AddHours(-6.00),
                EndDate = DateTime.Now.AddHours(-6.00),
                ClosedNwIp = "NwAddress"
            };
            MainDbContext.MultiFactor.Add(factorCombination);
            MainDbContext.MultiFactor.Add(factorCombination2);
            MainDbContext.SaveChanges();
            return factorCombination;
        }

        protected MultiFactor CreateUser5(SimAndDevice simAndDevice)
        {
            var admin = new UserAdmin
            {
                Domain = UserGroup.Domain,
                Name = "管理人一郎",
                Password = "password",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = "AccountUser1"
            };
            var userGroupEndUserAdmin = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = admin
            };
            var general = new GeneralUser
            {
                Domain = UserGroup.Domain,
                Name = "一般一郎",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = "AccountUser2"
            };
            var userGroupEndUserGeneral = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = general
            };
            var factorCombination = new MultiFactor
            {
                SimAndDevice = simAndDevice,
                EndUser = admin,
                StartDate = DateTime.Now.AddHours(-6.00),
                ClosedNwIp = "NwAddress"
            };
            var factorCombination2 = new MultiFactor
            {
                SimAndDevice = simAndDevice,
                EndUser = general,
                StartDate = DateTime.Now.AddHours(-6.00),
                ClosedNwIp = "NwAddress"
            };
            MainDbContext.AddRange(general, userGroupEndUserGeneral, admin, userGroupEndUserAdmin, factorCombination, factorCombination2);
            MainDbContext.SaveChanges();
            return factorCombination;
        }

        protected MultiFactor CreateUser6(SimAndDevice simAndDevice)
        {
            var admin = new UserAdmin
            {
                Domain = UserGroup.Domain,
                Name = "管理人一郎",
                Password = "password",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = "AccountUser1"
            };
            var userGroupEndUserAdmin = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = admin
            };
            var general = new GeneralUser
            {
                Domain = UserGroup.Domain,
                Name = "一般一郎",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = "AccountUser2"
            };
            var userGroupEndUserGeneral = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = general
            };
            var factorCombination = new MultiFactor
            {
                SimAndDevice = simAndDevice,
                EndUser = admin,
                StartDate = DateTime.Now.AddHours(-6.00),
                EndDate = DateTime.Now.AddHours(6.00),
                ClosedNwIp = "NwAddress"
            };
            var factorCombination2 = new MultiFactor
            {
                SimAndDevice = simAndDevice,
                EndUser = general,
                StartDate = DateTime.Now.AddHours(-6.00),
                EndDate = DateTime.Now.AddHours(6.00),
                ClosedNwIp = "NwAddress"
            };
            MainDbContext.AddRange(general, userGroupEndUserGeneral, admin, userGroupEndUserAdmin, factorCombination, factorCombination2);
            MainDbContext.SaveChanges();
            return factorCombination;
        }

        protected void UpdateOrganization((DateTime, DateTime) dayTuple, bool isValid)
        {
            Organization.StartDate = dayTuple.Item1;
            Organization.EndDate = dayTuple.Item2;
            Organization.IsValid = isValid;
        }

        protected (SimAndDevice, SimAndDevice, SimAndDevice) InsertSimDevice((DateTime, DateTime)? dayTuple = null, bool isExistsSimDeviceAuthenticationStateDone = false)
        {
            return InsertSimDevice(dayTuple?.Item1, dayTuple?.Item2, isExistsSimDeviceAuthenticationStateDone);
        }

        private (SimAndDevice, SimAndDevice, SimAndDevice) InsertSimDevice(DateTime? start, DateTime? end, bool isExistsSimAndDeviceAuthenticated)
        {
            // authentication
            var simDevice = new SimAndDevice()
            {
                Sim = Sim,
                Device = Device,
                IsolatedNw2Ip = "Nw2Address",
                StartDate = start ?? CurrentDateTimeForEnd.Item1,
                EndDate = end ?? CurrentDateTimeForEnd.Item2,
                AuthenticationDuration = 1
            };
            MainDbContext.SimAndDevice.Add(simDevice);

            if (isExistsSimAndDeviceAuthenticated)
            {
                var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated
                {
                    SimAndDevice = simDevice,
                    Expiration = DateTime.Now.AddHours(-1.00)
                };
                MainDbContext.SimAndDeviceAuthenticated.Add(simAndDeviceAuthenticated);
            }

            var simDeviceSameSimOtherDevice = new SimAndDevice()
            {
                Sim = Sim,
                Device = DeviceOther,
                IsolatedNw2Ip = "Nw2AddressOther",
                StartDate = start ?? CurrentDateTimeForEnd.Item1,
                EndDate = end ?? CurrentDateTimeForEnd.Item2,
                AuthenticationDuration = 2
            };
            var simDeviceOtherSimSameDevice = new SimAndDevice()
            {
                Sim = SimOther,
                Device = Device,
                IsolatedNw2Ip = "Nw2AddressOther",
                StartDate = start ?? CurrentDateTimeForEnd.Item1,
                EndDate = end ?? CurrentDateTimeForEnd.Item2,
                AuthenticationDuration = 3
            };

            MainDbContext.SimAndDevice.Add(simDeviceSameSimOtherDevice);
            MainDbContext.SimAndDevice.Add(simDeviceOtherSimSameDevice);

            return (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);
        }

        protected MultiFactor InsertFactorCombination(SimAndDevice simAndDevice, EndUser endUser,
            (DateTime, DateTime)? dayTuple = null)
        {
            return InsertFactorCombination(simAndDevice, endUser, dayTuple?.Item1, dayTuple?.Item2);
        }

        private MultiFactor InsertFactorCombination(SimAndDevice simAndDevice, EndUser endUser, DateTime? start, DateTime? end)
        {
            var factorCombination = new MultiFactor
            {
                SimAndDevice = simAndDevice,
                EndUser = endUser,
                StartDate = start ?? CurrentDateTimeForEnd.Item1,
                EndDate = end ?? CurrentDateTimeForEnd.Item2,
                ClosedNwIp = "NwAddress"
            };
            MainDbContext.MultiFactor.Add(factorCombination);
            return factorCombination;
        }

        protected void InsertMultiFactorAuthenticationStateDone(MultiFactor multiFactor)
        {
            var multiFactorAuthenticationStateDone = new MultiFactorAuthenticated()
            {
                Expiration = CurrentDateTimeForEnd.Item2
            };
            multiFactor.MultiFactorAuthenticated = multiFactorAuthenticationStateDone;
            MainDbContext.MultiFactorAuthenticated.Add(multiFactorAuthenticationStateDone);
        }

        protected UserAdmin InsertAdminUser(string accountName = "AccountUser1", (DateTime, DateTime)? dayTuple = null)
        {
            return InsertAdminUser(accountName, dayTuple?.Item1, dayTuple?.Item2);
        }

        private UserAdmin InsertAdminUser(string accountName, DateTime? start, DateTime? end)
        {
            var admin = new UserAdmin
            {
                Domain = UserGroup.Domain,
                Name = "管理人一郎",
                Password = "password",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = accountName,
            };
            var userGroupEndUser = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = admin
            };
            var availablePeriod = new AvailablePeriod()
            {
                EndUser = admin,
                StartDate = start ?? CurrentDateTimeForEnd.Item1,
                EndDate = end ?? CurrentDateTimeForEnd.Item2
            };
            var ancientAvailablePeriod = new AvailablePeriod()
            {
                EndUser = admin,
                StartDate = AncientDateTimeForLaterEnd.Item1,
                EndDate = AncientDateTimeForLaterEnd.Item2
            };

            MainDbContext.UserAdmin.Add(admin);
            MainDbContext.UserGroupEndUser.Add(userGroupEndUser);
            MainDbContext.AvailablePeriod.Add(availablePeriod);
            MainDbContext.AvailablePeriod.Add(ancientAvailablePeriod);
            return admin;
        }

        protected GeneralUser InsertGeneralUser(string accountName = "AccountUser2", (DateTime, DateTime)? dayTuple = null)
        {
            return InsertGeneralUser(accountName, dayTuple?.Item1, dayTuple?.Item2);
        }

        private GeneralUser InsertGeneralUser(string accountName, DateTime? start, DateTime? end)
        {
            var general = new GeneralUser
            {
                Domain = UserGroup.Domain,
                Name = "一般次郎",
                AuthenticateWhenUnlockingScreen = true,
                AccountName = accountName,
            };
            var userGroupEndUser = new UserGroupEndUser()
            {
                UserGroup = UserGroup,
                EndUser = general
            };
            var availablePeriod = new AvailablePeriod()
            {
                EndUser = general,
                StartDate = start ?? CurrentDateTimeForEnd.Item1,
                EndDate = end ?? CurrentDateTimeForEnd.Item2
            };
            var ancientAvailablePeriod = new AvailablePeriod()
            {
                EndUser = general,
                StartDate = AncientDateTimeForLaterEnd.Item1,
                EndDate = AncientDateTimeForLaterEnd.Item2
            };

            MainDbContext.GeneralUser.Add(general);
            MainDbContext.UserGroupEndUser.Add(userGroupEndUser);
            MainDbContext.AvailablePeriod.Add(availablePeriod);
            MainDbContext.AvailablePeriod.Add(ancientAvailablePeriod);
            return general;
        }


        protected void SetUpInsertOtherFactorCombinationAndEndUser(SimAndDevice simAndDeviceSameSimAndOtherDevice, SimAndDevice simAndDeviceOtherSimAndSameDevice)
        {
            // その他(同じSIM、違う端末)
            var insertAdminUserSsod = InsertAdminUser("AdminAccount_SameSimOtherDevice");
            var insertGeneralUserSsod = InsertGeneralUser("GeneralAccount_SameSimOtherDevice");
            InsertFactorCombination(simAndDeviceSameSimAndOtherDevice, insertAdminUserSsod);
            InsertFactorCombination(simAndDeviceSameSimAndOtherDevice, insertGeneralUserSsod);

            // その他(違うSIM、同じ端末)
            var insertAdminUserOssd = InsertAdminUser("AdminAccount_OtherSimSameDevice");
            var insertGeneralUserOssd = InsertGeneralUser("GeneralAccount_OtherSimSameDevice");
            InsertFactorCombination(simAndDeviceOtherSimAndSameDevice, insertAdminUserOssd);
            InsertFactorCombination(simAndDeviceOtherSimAndSameDevice, insertGeneralUserOssd);
        }

        protected (MultiFactor, MultiFactor) SetUpInsertFactorCombinationAndEndUser(SimAndDevice simAndDevice,
            string adminAccountName, string generalAccountName, (DateTime, DateTime) userDayTuple, (DateTime, DateTime) factorCombinationDayTuple)
        {
            var insertAdminUser = InsertAdminUser(adminAccountName, userDayTuple);
            var insertGeneralUser = InsertGeneralUser(generalAccountName, userDayTuple);
            var factorCombinationAdmin = InsertFactorCombination(simAndDevice, insertAdminUser, factorCombinationDayTuple);
            var factorCombinationGeneral = InsertFactorCombination(simAndDevice, insertGeneralUser, factorCombinationDayTuple);
            return (factorCombinationAdmin, factorCombinationGeneral);
        }

    }
}
