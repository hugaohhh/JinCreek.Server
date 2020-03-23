using System;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;

namespace JinCreek.Server.Batch.Repositories
{
    class DeauthenticationTestSetupRepository : BatchTestSetupRepository
    {
        public DeauthenticationTestSetupRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext) : base(mainDbContext, radiusDbContext)
        {
        }

        protected override void CreateBaseData()
        {
            base.CreateBaseData();

            CreateUserRecords();
            CreateDeviceRecords();

            CreateSimAndDeviceRecords();
            CreateMultiFactorRecords();
        }

        private void CreateUserRecords()
        {
            var userGroup = new UserGroup()
            {
                Domain = Domain,
                Name = "UserGroup",
                AdObjectId = Guid.NewGuid()
            };

            GeneralUser1 = new GeneralUser()
            {
                AccountName = "GeneralUser1",
                Name = "GU1",
                Domain = Domain,
                AuthenticateWhenUnlockingScreen = true,
                AdObjectId = Guid.NewGuid()
            };

            var availablePeriod = new AvailablePeriod()
            {
                EndUser = GeneralUser1,
                StartDate = CurrentDateTimeForStart.Item1,
                EndDate = CurrentDateTimeForStart.Item2
            };

            var userGroupEndUser = new UserGroupEndUser()
            {
                UserGroup = userGroup,
                EndUser = GeneralUser1
            };

            MainDbContext.AddRange(userGroup, GeneralUser1, availablePeriod, userGroupEndUser);
        }

        private void CreateDeviceRecords()
        {
            var lteModule = new LteModule()
            {
                Name = "Lte",
                NwAdapterName = "Adapter",
                UseSoftwareRadioState = true
            };

            var deviceGroup = new DeviceGroup()
            {
                Domain = Domain,
                Name = "DeviceGroup",
                AdObjectId = Guid.NewGuid()
            };

            Device1 = new Device()
            {
                Domain = Domain,
                Name = "Device1",
                LteModule = lteModule,
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
                StartDate = CurrentDateTimeForStart.Item1,
                EndDate = CurrentDateTimeForStart.Item2,
                AdObjectId = Guid.NewGuid()
            };

            var deviceGroupDevice = new DeviceGroupDevice()
            {
                DeviceGroup = deviceGroup,
                Device = Device1
            };

            MainDbContext.AddRange(lteModule, deviceGroup, Device1, deviceGroupDevice);
        }

        private void CreateSimAndDeviceRecords()
        {
            SimAndDevice1 = new SimAndDevice()
            {
                Sim = Sim1,
                Device = Device1,
                IsolatedNw2Ip = "192.168.10.1",
                AuthenticationDuration = 2,
                StartDate = CurrentDateTimeForStart.Item1,
                EndDate = CurrentDateTimeForStart.Item2,
            };

            MainDbContext.AddRange(SimAndDevice1);
        }

        private void CreateMultiFactorRecords()
        {
            MultiFactor1 = new MultiFactor()
            {
                SimAndDevice = SimAndDevice1,
                EndUser = GeneralUser1,
                ClosedNwIp = "192.168.20.1",
                StartDate = CurrentDateTimeForStart.Item1,
                EndDate = CurrentDateTimeForStart.Item2,
            };
        }


        //--------------------------------------------------------------------

        public void CreateDataCase01()
        {
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase02()
        {
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase03()
        {
            CreateBaseData();

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForLaterEnd.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase04()
        {
            CreateBaseData();

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForEnd.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase05()
        {
            CreateBaseData();

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForLessThanStart.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase06()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForLaterEnd.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase07()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForLaterEnd.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForLaterEnd.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase08()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForLaterEnd.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForEnd.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase09()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForLaterEnd.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForLessThanStart.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase10()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForEnd.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase11()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForEnd.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForLaterEnd.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase12()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForEnd.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForEnd.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase13()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForEnd.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForLessThanStart.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase14()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForLessThanStart.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase15()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForLessThanStart.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForLaterEnd.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase16()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForLessThanStart.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForEnd.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void CreateDataCase17()
        {
            CreateBaseData();

            var simAndDeviceAuthenticated = new SimAndDeviceAuthenticated()
            {
                SimAndDevice = SimAndDevice1,
                Expiration = CurrentDateTimeForLessThanStart.Item2
            };
            MainDbContext.AddRange(simAndDeviceAuthenticated);
            SimAndDevice1.SimAndDeviceAuthenticated = simAndDeviceAuthenticated;

            var multiFactorAuthenticated = new MultiFactorAuthenticated()
            {
                MultiFactor = MultiFactor1,
                Expiration = CurrentDateTimeForLessThanStart.Item2
            };
            MainDbContext.AddRange(multiFactorAuthenticated);
            MultiFactor1.MultiFactorAuthenticated = multiFactorAuthenticated;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }
    }
}
