using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using System;

namespace JinCreek.Server.AuthTests.Repositories
{
    public class SimDeviceAuthenticationControllerTestSetupRepository : AuthControllerTestSetupRepository
    {
        public SimDeviceAuthenticationControllerTestSetupRepository(MainDbContext mainDbContext, RadiusDbContext radiusDbContext) : base(mainDbContext, radiusDbContext)
        {
        }

        private void SetUpInsertFactorCombinationAndEndUser01((DateTime, DateTime)? simDeviceDayTuple)
        {
            InsertSimDevice(simDeviceDayTuple, true);
        }

        private void SetUpInsertFactorCombinationAndEndUser02((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (_, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);
        }

        private void SetUpInsertFactorCombinationAndEndUser03((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForLessThanStart, CurrentDateTimeForLessThanStart);
        }

        private void SetUpInsertFactorCombinationAndEndUser04((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForLessThanStart, CurrentDateTimeForLaterEnd);
        }

        private void SetUpInsertFactorCombinationAndEndUser05((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForLessThanStart, CurrentDateTimeForStart);
        }

        private void SetUpInsertFactorCombinationAndEndUser06((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForLessThanStart, CurrentDateTimeForEnd);
        }

        private void SetUpInsertFactorCombinationAndEndUser07((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForLaterEnd, CurrentDateTimeForLessThanStart);
        }

        private void SetUpInsertFactorCombinationAndEndUser08((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForLaterEnd, CurrentDateTimeForLaterEnd);
        }

        private void SetUpInsertFactorCombinationAndEndUser09((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForLaterEnd, CurrentDateTimeForStart);
        }

        private void SetUpInsertFactorCombinationAndEndUser10((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForLaterEnd, CurrentDateTimeForEnd);
        }

        private void SetUpInsertFactorCombinationAndEndUser11((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForStart, CurrentDateTimeForLessThanStart);
        }

        private void SetUpInsertFactorCombinationAndEndUser12((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForStart, CurrentDateTimeForLaterEnd);
        }

        private void SetUpInsertFactorCombinationAndEndUser13((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForStart, CurrentDateTimeForStart);
        }

        private void SetUpInsertFactorCombinationAndEndUser14((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForStart, CurrentDateTimeForEnd);
        }

        private void SetUpInsertFactorCombinationAndEndUser15((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForLessThanStart);
        }

        private void SetUpInsertFactorCombinationAndEndUser16((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForLaterEnd);
        }

        private void SetUpInsertFactorCombinationAndEndUser17((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForStart);
        }

        private void SetUpInsertFactorCombinationAndEndUser18((DateTime, DateTime)? simDeviceDayTuple)
        {
            var (simDevice, simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice) = InsertSimDevice(simDeviceDayTuple, true);
            SetUpInsertOtherFactorCombinationAndEndUser(simDeviceSameSimOtherDevice, simDeviceOtherSimSameDevice);

            SetUpInsertFactorCombinationAndEndUser(simDevice, "AdminAccount", "GeneralAccount",
                CurrentDateTimeForEnd, CurrentDateTimeForEnd);
        }


        public void SetUpInsertDataForCase01_12()
        {
            SetUpInsertBaseDataForRadiusDb();
            RadiusDbContext.SaveChanges();
        }


        private void SetUpInsertDataForCase13()
        {
            UpdateOrganization(CurrentDateTimeForStart, true);
        }

        public void SetUpInsertDataForCase1301()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser01(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1302()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser02(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1303()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser03(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1304()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser04(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1305()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser05(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1306()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser06(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1307()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser07(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1308()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser08(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1309()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser09(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1310()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser10(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1311()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser11(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1312()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser12(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1313()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser13(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1314()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser14(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1315()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser15(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1316()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser16(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1317()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser17(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1318()
        {
            SetUpInsertDataForCase13();
            SetUpInsertFactorCombinationAndEndUser18(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase14()
        {
            SetUpInsertBaseDataForRadiusDb();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase15()
        {
        }

        public void SetUpInsertDataForCase16()
        {
            var simDevice = new SimAndDevice
            {
                Sim = Sim,
                Device = Device,
                IsolatedNw2Ip = "Nw2Address",
                StartDate = CurrentDateTimeForLessThanStart.Item1,
                EndDate = CurrentDateTimeForLessThanStart.Item2,
                AuthenticationDuration = 1
            };
            MainDbContext.SimAndDevice.Add(simDevice);

            MainDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase17()
        {
            var simDevice = new SimAndDevice
            {
                Sim = Sim,
                Device = Device,
                IsolatedNw2Ip = "Nw2Address",
                StartDate = CurrentDateTimeForLaterEnd.Item1,
                EndDate = CurrentDateTimeForLaterEnd.Item2,
                AuthenticationDuration = 1
            };
            MainDbContext.SimAndDevice.Add(simDevice);

            MainDbContext.SaveChanges();
        }

        private void SetUpInsertDataForCase18()
        {
            UpdateOrganization(CurrentDateTimeForStart, true);
        }

        public void SetUpInsertDataForCase1801()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser01(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1802()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser02(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1803()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser03(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1804()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser04(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1805()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser05(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1806()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser06(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1807()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser07(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1808()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser08(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1809()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser09(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1810()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser10(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1811()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser11(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1812()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser12(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1813()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser13(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1814()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser14(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1815()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser15(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1816()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser16(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1817()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser17(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1818()
        {
            SetUpInsertDataForCase18();
            SetUpInsertFactorCombinationAndEndUser18(CurrentDateTimeForStart);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        private void SetUpInsertDataForCase19()
        {
            UpdateOrganization(CurrentDateTimeForStart, true);
        }

        public void SetUpInsertDataForCase1901()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser01(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1902()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser02(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1903()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser03(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1904()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser04(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1905()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser05(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1906()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser06(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1907()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser07(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1908()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser08(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1909()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser09(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1910()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser10(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1911()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser11(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1912()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser12(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1913()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser13(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1914()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser14(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1915()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser15(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1916()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser16(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1917()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser17(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase1918()
        {
            SetUpInsertDataForCase19();
            SetUpInsertFactorCombinationAndEndUser18(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }


        public void SetUpInsertDataForCase20()
        {
            Organization.StartDate = CurrentDateTimeForLessThanStart.Item1;
            Organization.EndDate = CurrentDateTimeForLessThanStart.Item2;
            Organization.IsValid = true;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase21()
        {
            Organization.StartDate = CurrentDateTimeForLaterEnd.Item1;
            Organization.EndDate = CurrentDateTimeForLaterEnd.Item2;
            Organization.IsValid = true;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        private void SetUpInsertDataForCase22()
        {
            UpdateOrganization(CurrentDateTimeForStart, true);
        }

        public void SetUpInsertDataForCase2201()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser01(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2202()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser02(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2203()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser03(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2204()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser04(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2205()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser05(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2206()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser06(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2207()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser07(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2208()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser08(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2209()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser09(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2210()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser10(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2211()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser11(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2212()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser12(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2213()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser13(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2214()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser14(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2215()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser15(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2216()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser16(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2217()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser17(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase2218()
        {
            SetUpInsertDataForCase22();
            SetUpInsertFactorCombinationAndEndUser18(CurrentDateTimeForEnd);
            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }

        public void SetUpInsertDataForCase23()
        {
            Organization.StartDate = CurrentDateTimeForEnd.Item1;
            Organization.EndDate = CurrentDateTimeForEnd.Item2;
            Organization.IsValid = false;

            MainDbContext.SaveChanges();
            RadiusDbContext.SaveChanges();
        }
    }
}
