using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using System;
using System.Linq;

namespace JinCreek.Server.AuthTests.Repositories
{
    public class DeauthenticationControllerTestSetupRepository : AuthControllerTestSetupRepository
    {
        public DeauthenticationControllerTestSetupRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext) : base(mainDbContext, radiusDbContext)
        {
        }

        public void SetUpInsertBaseDataForDeauthenticationRadiusDb()
        {
            //base.SetUpInsertBaseDataForRadiusDb();

            RadiusDbContext.SaveChanges();
            var radreply = RadiusDbContext.Radreply.Where(r => r.Username == "user1@jincreek2" && r.Attribute == "Framed-IP-Address").Single();
            RadiusDbContext.SaveChanges();
            radreply.Value = "NwAddress";
        }


        public void SetUpInsertDataForCase01()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase02()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        //public void SetUpInsertDataForCase03()
        //{
        //    SetUpInsertBaseDataForDeauthenticationRadiusDb();

        //    MainDbContext.SaveChanges();
        //    RadiusDbContext.SaveChanges();
        //}

        //public void SetUpInsertDataForCase04()
        //{
        //    SetUpInsertBaseDataForDeauthenticationRadiusDb();

        //    MainDbContext.SaveChanges();
        //    RadiusDbContext.SaveChanges();
        //}

        //public void SetUpInsertDataForCase05()
        //{
        //    var (simAndDevice, _, _) = InsertSimDevice(CurrentDateTimeForEnd, true);
        //    simAndDevice.SimAndDeviceAuthenticated.Id = Guid.NewGuid();

        //    SetUpInsertBaseDataForDeauthenticationRadiusDb();

        //    MainDbContext.SaveChanges();
        //    RadiusDbContext.SaveChanges();
        //}

        public void SetUpInsertDataForCase06()
        {
            var (simDevice, _, _) = InsertSimDevice(CurrentDateTimeForEnd, true);
            simDevice.SimAndDeviceAuthenticated.Id = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776a");

            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase07()
        {
            var (simDevice, _, _) = InsertSimDevice(CurrentDateTimeForEnd, true);
            simDevice.SimAndDeviceAuthenticated.Id = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776a");

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);

            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase08()
        {
            var (simDevice, _, _) = InsertSimDevice(CurrentDateTimeForEnd, true);
            simDevice.SimAndDeviceAuthenticated.Id = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776a");

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "AccountUser1",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);

            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase09()
        {
            var (simDevice, _, _) = InsertSimDevice(CurrentDateTimeForEnd, true);
            simDevice.SimAndDeviceAuthenticated.Id = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776a");

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "AccountUser1",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);
            InsertMultiFactorAuthenticationStateDone(fca);
            InsertMultiFactorAuthenticationStateDone(fcg);

            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase10()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase11()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase12()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase13()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase14()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase15()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase16()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase17()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase18()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase19()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase20()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase21()
        {
            SetUpInsertBaseDataForDeauthenticationRadiusDb();

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }


        public SimAndDevice SetUpInsertDataForDeauthenticationCase13()
        {
            var simDevice = new SimAndDevice
            {
                Sim = Sim,
                Device = Device,
                IsolatedNw2Ip = "Nw2Address",
                StartDate = DateTime.Now.AddHours(-6.00),
                EndDate = DateTime.Now.AddHours(6.00),
                AuthenticationDuration = 1
            };
            MainDbContext.AddRange(simDevice);
            MainDbContext.SaveChanges();
            return simDevice;
        }
        public void SetUpInsertDataForDeauthenticationCase14()
        {
            SetUpInsertDataForDeauthenticationCase13();
        }
        public void SetUpInsertDataForDeauthenticationCase15()
        {
            SetUpInsertDataForDeauthenticationCase13();
        }
        public void SetUpInsertDataForDeauthenticationCase16()
        {
            SetUpInsertDataForDeauthenticationCase13();
        }
        public void SetUpInsertDataForDeauthenticationCase17()
        {
            SetUpInsertDataForDeauthenticationCase13();
        }
        public void SetUpInsertDataForDeauthenticationCase18()
        {
            var simDevice = SetUpInsertDataForDeauthenticationCase13();
            CreateUser6(simDevice);
        }
        public void SetUpInsertDataForDeauthenticationCase19()
        {
            var simDevice = SetUpInsertDataForDeauthenticationCase13();
            CreateUser6(simDevice);
        }
        public void SetUpInsertDataForDeauthenticationCase20()
        {
            var simDevice = SetUpInsertDataForDeauthenticationCase13();
            var simDeviceAuthenticationStateDone = new SimAndDeviceAuthenticated
            {
                Expiration = DateTime.Now.AddHours(1.00)
            };
            simDevice.SimAndDeviceAuthenticated = simDeviceAuthenticationStateDone;
            MainDbContext.SimAndDeviceAuthenticated.Add(simDeviceAuthenticationStateDone);
            MainDbContext.SaveChanges();
            CreateUser6(simDevice);
        }
        public void SetUpInsertDataForDeauthenticationCase21()
        {
            var simDevice = SetUpInsertDataForDeauthenticationCase13();
            var simDeviceAuthenticationStateDone = new SimAndDeviceAuthenticated
            {
                Expiration = DateTime.Now.AddHours(1.00)
            };
            simDevice.SimAndDeviceAuthenticated = simDeviceAuthenticationStateDone;
            MainDbContext.SimAndDeviceAuthenticated.Add(simDeviceAuthenticationStateDone);
            var factorCombination = CreateUser6(simDevice);
            var multiFactorAuthenticationStateDone = new MultiFactorAuthenticated
            {
                Expiration = DateTime.Now.AddHours(1.00)
            };
            factorCombination.MultiFactorAuthenticated = multiFactorAuthenticationStateDone;
            MainDbContext.MultiFactorAuthenticated.Add(multiFactorAuthenticationStateDone);
            MainDbContext.SaveChanges();
        }
    }
}
