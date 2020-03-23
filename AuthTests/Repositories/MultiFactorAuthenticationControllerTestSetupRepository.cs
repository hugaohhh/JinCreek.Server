using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using System;
using System.Linq;

namespace JinCreek.Server.AuthTests.Repositories
{
    public class MultiFactorAuthenticationControllerTestSetupRepository : AuthControllerTestSetupRepository
    {
        public MultiFactorAuthenticationControllerTestSetupRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext) : base(mainDbContext, radiusDbContext)
        {
        }

        private (SimAndDevice, SimAndDevice, SimAndDevice) SetUpInsertBaseDataForMainDb((DateTime, DateTime) dayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(dayTuple, true);
            simDevice.SimAndDeviceAuthenticated.Id = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b");

            return (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);
        }


        public override void SetUpInsertBaseDataForRadiusDb()
        {
            base.SetUpInsertBaseDataForRadiusDb();

            RadiusDbContext.SaveChanges();
            var radreply = RadiusDbContext.Radreply.Where(r => r.Username == "user1@jincreek2" && r.Attribute == "Framed-IP-Address").Single();
            RadiusDbContext.SaveChanges();
            radreply.Value = "Nw2Address";
        }


        public void SetUpInsertDataForCase01()
        {
            SetUpInsertBaseDataForRadiusDb();

            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase02()
        {
            SetUpInsertBaseDataForRadiusDb();

            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase03()
        {
            SetUpInsertBaseDataForRadiusDb();

            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase04()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);

            SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase05()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);
            fca.EndUser.AuthenticateWhenUnlockingScreen = true;
            fcg.EndUser.AuthenticateWhenUnlockingScreen = true;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase06()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);
            fca.EndUser.AuthenticateWhenUnlockingScreen = true;
            fcg.EndUser.AuthenticateWhenUnlockingScreen = true;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase07()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForLessThanStart);
            fca.EndUser.AuthenticateWhenUnlockingScreen = true;
            fcg.EndUser.AuthenticateWhenUnlockingScreen = true;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase08()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForLaterEnd);
            fca.EndUser.AuthenticateWhenUnlockingScreen = true;
            fcg.EndUser.AuthenticateWhenUnlockingScreen = true;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase09()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForStart);
            fcg.EndUser.AccountName = "AccountUser1";
            fca.EndUser.AuthenticateWhenUnlockingScreen = true;
            fcg.EndUser.AuthenticateWhenUnlockingScreen = true;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase10()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);
            fcg.EndUser.AccountName = "AccountUser1";
            fca.EndUser.AuthenticateWhenUnlockingScreen = true;
            fcg.EndUser.AuthenticateWhenUnlockingScreen = true;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase11()
        {
            SetUpInsertDataForCase10();
        }

        public void SetUpInsertDataForCase12()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);
            fcg.EndUser.AccountName = "AccountUser1";
            fca.EndUser.AuthenticateWhenUnlockingScreen = false;
            fcg.EndUser.AuthenticateWhenUnlockingScreen = false;
            InsertMultiFactorAuthenticationStateDone(fca);
            InsertMultiFactorAuthenticationStateDone(fcg);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase13()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            simDevice.SimAndDeviceAuthenticated.Id = Guid.NewGuid();
            MainDbContext.SaveChanges();

            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            var (fca, fcg) = SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);
            fcg.EndUser.AccountName = "AccountUser1";
            fca.EndUser.AuthenticateWhenUnlockingScreen = false;
            fcg.EndUser.AuthenticateWhenUnlockingScreen = false;
            InsertMultiFactorAuthenticationStateDone(fca);
            InsertMultiFactorAuthenticationStateDone(fcg);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase14()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (_, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();

            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase15()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (_, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();

            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase16()
        {
            UpdateOrganization(CurrentDateTimeForLessThanStart, true);
            var (_, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();

            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase17()
        {
            UpdateOrganization(CurrentDateTimeForLaterEnd, true);
            var (_, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();

            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase18()
        {
            UpdateOrganization(CurrentDateTimeForStart, true);
            var (_, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();

            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase19()
        {
            UpdateOrganization(CurrentDateTimeForEnd, true);
            var (_, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();

            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase20()
        {
            UpdateOrganization(CurrentDateTimeForEnd, false);
            var (_, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = SetUpInsertBaseDataForMainDb(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();

            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }
    }
}
