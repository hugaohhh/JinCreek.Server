using System;
using ConsoleAppFramework;
using JinCreek.Server.Batch.Repositories;
using JinCreek.Server.Common.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.Batch
{
    [Collection("Sequential")]
    public class LdapSyncTest : BatchTestBase
    {
        private readonly BatchTestRepository _batchTestRepository;
        private readonly LdapSyncTestSetupRepository _ldapSyncTestSetupRepository;

        public LdapSyncTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _batchTestRepository = new BatchTestRepository(MainDbContext, RadiusDbContext);
            _ldapSyncTestSetupRepository = new LdapSyncTestSetupRepository(MainDbContext, RadiusDbContext);
        }

        private (List<EndUser> users,List<Device> devices,List<DeviceGroup> deviceGroups,List<UserGroup> userGroups) GetRecordsForAssert()
        {
            int targetOrganizationCode = _ldapSyncTestSetupRepository.OrganizationCode;
            var users = _batchTestRepository.GetUsers(targetOrganizationCode);
            var devices = _batchTestRepository.GetDevices(targetOrganizationCode);
            var deviceGroups = _batchTestRepository.GetDeviceGroup(targetOrganizationCode);
            var userGroups = _batchTestRepository.GetUserGroup(targetOrganizationCode);
            return (users, devices, deviceGroups,userGroups);
        }

        private void Assert09_10_11()
        {
            var (users, devices,deviceGroups,userGroups) = GetRecordsForAssert();
            var domains = _batchTestRepository.GetDomain(_ldapSyncTestSetupRepository.OrganizationCode); //ad-Domain-4
            Assert.NotEmpty(domains);
            Assert.Equal("jincreek.jp", domains.Where(r => r.Name == "jincreek.jp").FirstOrDefault()?.Name);
            Assert.Equal(_ldapSyncTestSetupRepository.OrganizationCode, domains[0].OrganizationCode);
            Assert.NotNull(domains[0].AdObjectId);
            //DeviceGroup3~5
            foreach (var deviceGroup in deviceGroups.Where(r => r.Domain.Name == "jincreek.jp").ToList())
            {
                Assert.NotNull(deviceGroup.AdObjectId);
                Assert.Equal("jincreek.jp", deviceGroup.Domain.Name);
            }
            //device1~6
            foreach (var device in devices.Where(r => r.Domain.Name == "jincreek.jp").ToList())
            {
                Assert.NotNull(device.AdObjectId);
                if (device.Name.Equals("TESTCOMP1")) //device1 は新規
                {
                    Assert.Null(device.EndDate);
                    Assert.Equal(DateTime.Now.Date, device.StartDate);
                    Assert.Null(device.OrganizationClientApp);
                }
                if (device.Name.Equals("TESTCOMP3") || device.Name.Equals("TESTCOMP2") ||
                    device.Name.Equals("TESTCOMP4") || device.Name.Equals("TESTCOMP5")) //更新device
                {
                    Assert.True(DateTime.Now.Date >= device.StartDate);
                    Assert.True(device.EndDate == null || DateTime.Now.Date <= device.EndDate);
                    // 更新時は過去データを引き継ぐ
                    Assert.NotNull(device.OrganizationClientApp);
                }

                if (device.Name.Equals("device01")) // 更新しないdevice
                {
                    Assert.Equal(DateTime.Now.Date, device.EndDate);
                    Assert.NotNull(device.OrganizationClientApp);
                    Assert.Equal("2020-03-01T00:00:00", device.StartDate.ToString("s")); // 更新しない確認
                }
            }
            //userGroup3~5
            foreach (var userGroup in userGroups.Where(r => r.Domain.Name == "jincreek.jp").ToList())
            {
                Assert.NotNull(userGroup.AdObjectId);
                Assert.Equal("jincreek.jp", userGroup.Domain.Name);
            }
            //user1~6
            foreach (var endUser in users.Where(r => r.Domain.Name == "jincreek.jp").ToList())
            {
                Assert.NotNull(endUser.AdObjectId);
                if (endUser.Name.Equals("TESTUSER1")) // 新規 user-1
                {
                    var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                    Assert.NotNull(availablePeriod);
                    Assert.Null(availablePeriod.EndDate);
                    Assert.Equal(DateTime.Now.Date, availablePeriod.StartDate);
                }
                if (endUser.Name.Equals("TESTUSER3") || endUser.Name.Equals("TESTUSER2") ||
                    endUser.Name.Equals("TESTUSER4") || endUser.Name.Equals("TESTUSER5")) //更新
                {
                    Assert.Equal(endUser.Name + "@jincreek.jp", endUser.AccountName);
                    var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                    Assert.NotNull(availablePeriod);
                    Assert.True(DateTime.Now.Date <= availablePeriod.StartDate);
                    Assert.True(availablePeriod.EndDate == null || DateTime.Now.Date <= availablePeriod.EndDate);
                }
                if (endUser.AccountName.Equals("accountName01")) //更新しないuser
                {
                    var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                    Assert.NotNull(availablePeriod);
                    Assert.Equal("2020-03-01T00:00:00", availablePeriod.StartDate.ToString("s")); // 更新しない確認
                    Assert.NotNull(availablePeriod.EndDate);
                }
            }
        }

        [Fact]
        public void TestCase01()
        {
            _ldapSyncTestSetupRepository.CreateDataCase01();
            var args = new[] { "ldap_sync" };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args); // COde:'不在
            Assert.True(rtn.IsCompletedSuccessfully);
            var (users, devices, deviceGroups, userGroups) = GetRecordsForAssert();
            Assert.Empty(users);
            Assert.Empty(devices);
            Assert.Empty(deviceGroups);
            Assert.Empty(userGroups);
        }

        [Fact]
        public void TestCase02()
        {
            _ldapSyncTestSetupRepository.CreateDataCase02();
            var args = new[] { "ldap_sync", "-organization_code", "aaaaaaaaa" };// COde:''数値以外
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (users, devices, deviceGroups, userGroups) = GetRecordsForAssert();
            Assert.Empty(users);
            Assert.Empty(devices);
            Assert.Empty(deviceGroups);
            Assert.Empty(userGroups);
        }

        [Fact]
        public void TestCase03()
        { 
            _ldapSyncTestSetupRepository.CreateDataCase03();
            var args = new[] { "ldap_sync", "-organization_code", "1" };// COde:'数値(該当Organization不在)
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (users, devices, deviceGroups, userGroups) = GetRecordsForAssert();
            Assert.Empty(users);
            Assert.Empty(devices);
            Assert.Empty(deviceGroups);
            Assert.Empty(userGroups);
        }

        [Fact]
        public void TestCase04()
        {
            _ldapSyncTestSetupRepository.CreateDataCase04(); // StartDate:'実行日より未来
            var args = new[] { "ldap_sync", "-organization_code", _ldapSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (users, devices, deviceGroups, userGroups) = GetRecordsForAssert();
            Assert.Empty(users);
            Assert.Empty(devices);
            Assert.Empty(deviceGroups);
            Assert.Empty(userGroups);
        }

        [Fact]
        public void TestCase05()
        {
            _ldapSyncTestSetupRepository.CreateDataCase05(); // Ad-Domain-1
            var args = new[] { "ldap_sync", "-organization_code", _ldapSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);
            var domains = _batchTestRepository.GetDomain(_ldapSyncTestSetupRepository.OrganizationCode);
            Assert.NotEmpty(domains);
            Assert.Equal("jincreek.jp",domains.Where(r => r.Name == "jincreek.jp").FirstOrDefault()?.Name);
            Assert.Equal(_ldapSyncTestSetupRepository.OrganizationCode, domains[0].OrganizationCode);
            Assert.NotNull( domains[0].AdObjectId);
        }

        [Fact]
        public void TestCase06()
        {
            _ldapSyncTestSetupRepository.CreateDataCase06(); // Ad側データなし
            var args = new[]
                {"ldap_sync", "-organization_code", _ldapSyncTestSetupRepository.OrganizationCode.ToString()};
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);

            Assert.True(rtn.IsCompletedSuccessfully);
            var (users, devices, _, _) = GetRecordsForAssert();
            Assert.NotEmpty(users);
            Assert.NotEmpty(devices);
            foreach (var device in devices.Where(d => d.Domain.Name == "test").ToList())
            {
                if (device.Name.Equals("device01"))//device-7 
                {
                    Assert.Null(device.EndDate);
                    Assert.NotNull(device.AdObjectId);
                    Assert.Equal("2020-03-01T00:00:00", device.StartDate.ToString("s")); // 更新しない確認
                }
            }
            //user7~9
            foreach (var endUser in users.Where(d => d.Domain.Name == "test").ToList())
            {
                if (endUser.Name.Equals("user01") || endUser.Name.Equals("user02") ||
                    endUser.Name.Equals("user03")) //
                {
                    Assert.NotNull(endUser.AdObjectId);
                    var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                    Assert.NotNull(availablePeriod);
                    Assert.Equal("2020-03-01T00:00:00", availablePeriod.StartDate.ToString("s")); // 更新しない確認
                }
            }
        }

        [Fact]
        public void TestCase07()
        {
            var ogranizationCode = 5;
            _ldapSyncTestSetupRepository.CreateDataCase07();
            var args = new[] { "ldap_sync", "-organization_code", ogranizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var domains = _batchTestRepository.GetDomain(ogranizationCode);
            Assert.NotEmpty(domains);
            Assert.Equal("jincreek.jp", domains.Where(r => r.Name == "jincreek.jp").FirstOrDefault()?.Name);
            Assert.Equal(ogranizationCode, domains[0].OrganizationCode);
            Assert.NotNull(domains[0].AdObjectId);
            var (users, devices, deviceGroups, userGroups) = GetRecordsForAssert();
            //DeviceGroup1-2-6
            foreach (var deviceGroup in deviceGroups.Where(r => r.Domain.Name == "jincreek.jp").ToList())
            {
                Assert.NotNull(deviceGroup.AdObjectId);
                Assert.Equal("jincreek.jp", deviceGroup.Domain.Name);
            }
            //Device1-7
            foreach (var device in devices.Where(r => r.Domain.Name == "jincreek.jp").ToList())
            {
                if (device.Name.Equals("TESTCOMP1")) //device1 は新規
                {
                    Assert.Null(device.EndDate);
                    Assert.NotNull(device.AdObjectId);
                    Assert.Equal(DateTime.Now.Date, device.StartDate);
                    Assert.Null(device.OrganizationClientApp);
                }
                if (device.Name.Equals("device01"))//device-7 論理削除device
                {
                    Assert.NotNull(device.EndDate);
                    Assert.NotNull(device.AdObjectId);
                    Assert.NotNull(device.OrganizationClientApp);
                    Assert.Equal("2020-03-01T00:00:00", device.StartDate.ToString("s")); // 更新しない確認
                    var simAndDevices = _batchTestRepository.GetSimAndDeviceByDeviceId(device.Id);
                    foreach (var simAndDevice in simAndDevices)
                    {
                        Assert.True(DateTime.Now.Date >= simAndDevice.EndDate);
                    }
                }
            }
            //userGroup1-2-6
            foreach (var userGroup in userGroups.Where(r => r.Domain.Name == "jincreek.jp").ToList())
            {
                Assert.NotNull(userGroup.AdObjectId);
                Assert.Equal("jincreek.jp", userGroup.Domain.Name);
            }
            //user1 7~9
            foreach (var endUser in users.Where(r => r.Domain.Name == "jincreek.jp").ToList())
            {
                Assert.NotNull(endUser.AdObjectId);
                if (endUser.Name.Equals("TESTUSER1")) // 新規 user-1
                {
                    var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                    Assert.NotNull(availablePeriod);
                    Assert.Null(availablePeriod.EndDate);
                    Assert.Equal(DateTime.Now.Date, availablePeriod.StartDate);
                }
                if (endUser.Name.Equals("user01") || endUser.Name.Equals("user02") || endUser.Name.Equals("user03")) //論理削除のuser
                {
                    var availablePeriod = endUser.AvailablePeriods.OrderByDescending(a => a.StartDate).FirstOrDefault();
                    Assert.NotNull(availablePeriod);
                    Assert.Equal("2020-03-01T00:00:00", availablePeriod.StartDate.ToString("s")); // 更新しない確認
                    Assert.NotNull(availablePeriod.EndDate);
                    var multiFactors = _batchTestRepository.GetMultiFactorByEndUser(endUser.Id);
                    foreach (var multiFactor in multiFactors)
                    {
                        Assert.Equal(DateTime.Now.Date,multiFactor.EndDate);//論理削除
                    }
                }
            }
        }

        [Fact]
        public void TestCase08()
        {
            _ldapSyncTestSetupRepository.CreateDataCase08();
            var args = new[] { "ldap_sync", "-organization_code", _ldapSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            var (users, devices, deviceGroups, userGroups) = GetRecordsForAssert();
            Assert.Empty(users);
            Assert.Empty(devices);
            Assert.Empty(deviceGroups);
            Assert.Empty(userGroups);
        }

        [Fact]
        public void TestCase09()
        {
            _ldapSyncTestSetupRepository.CreateDataCase09();// EndDate'実行日と同日
            var args = new[] { "ldap_sync", "-organization_code", _ldapSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            Assert09_10_11();
        }

        [Fact]
        public void TestCase10()
        {
            _ldapSyncTestSetupRepository.CreateDataCase10();// EndDate''実行日より未来
            var args = new[] { "ldap_sync", "-organization_code", _ldapSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            Assert09_10_11();
        }

        [Fact]
        public void TestCase11()
        {
            _ldapSyncTestSetupRepository.CreateDataCase11();// EndDate'不在
            var args = new[] { "ldap_sync", "-organization_code", _ldapSyncTestSetupRepository.OrganizationCode.ToString() };
            var rtn = HostBuilder.RunConsoleAppFrameworkAsync(args);
            Assert.True(rtn.IsCompletedSuccessfully);
            Assert09_10_11();
        }

    }
}
