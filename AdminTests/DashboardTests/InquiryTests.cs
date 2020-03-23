using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.DashboardTests
{
    /// <summary>
    /// トップ画面照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public class InquiryTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/dashboard";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly SimGroup _simGroup1;
        private readonly DeviceGroup _deviceGroup1;
        private readonly Device _device1;
        private readonly Device _device2;
        private readonly Device _device3;
        private readonly SimAndDevice _simDevice1;
        private readonly SimAndDevice _simDevice2;
        private readonly SimAndDevice _simDevice3;
        private readonly SimAndDevice _simDevice4;
        private readonly User _user0;
        private readonly UserAdmin _user1;
        private readonly GeneralUser _user2;
        private readonly GeneralUser _user3;
        private readonly GeneralUser _user4;
        private readonly Sim _sim1;
        private readonly Sim _sim2;
        private readonly LteModule _lte1 = null;
        private readonly UserGroupEndUser _userGroupEndUser;

        private readonly MultiFactor _multiFactor1;
        private readonly MultiFactor _multiFactor2;
        private readonly MultiFactor _multiFactor3;
        private readonly MultiFactor _multiFactor4;
        private readonly MultiFactor _multiFactor5;
        private readonly MultiFactor _multiFactor6;
        private readonly MultiFactor _multiFactor7;
        private readonly MultiFactor _multiFactor8;
        private readonly MultiFactor _multiFactor9;
        private readonly MultiFactor _multiFactor10;
        private readonly MultiFactor _multiFactor11;
        private readonly MultiFactor _multiFactor12;

        public InquiryTests(CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", OrganizationCode = _org1.Code });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", OrganizationCode = _org2.Code });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup1", DomainId = _domain1.Id });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup2", DomainId = _domain2.Id });
            _context.Add(_user0 = new SuperAdmin { Name = "", AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user1 = new UserAdmin() { Name = "", AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_user2 = new GeneralUser() { Name = "", AccountName = "user2", DomainId = _domain1.Id });
            _context.Add(_user3 = new GeneralUser() { Name = "", AccountName = "user3", DomainId = _domain1.Id });
            _context.Add(_user4 = new GeneralUser() { Name = "", AccountName = "user4", DomainId = _domain2.Id });
            _context.Add(new AvailablePeriod() { EndUser = _user1, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(new AvailablePeriod() { EndUser = _user2, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(new AvailablePeriod() { EndUser = _user3, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(new AvailablePeriod() { EndUser = _user4, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(_userGroupEndUser = new UserGroupEndUser() { EndUser = _user2, UserGroup = _userGroup1 });
            _context.Add(_simGroup1 = new SimGroup
            {
                Name = "simGroup1",
                Organization = _org1,
                Apn = "apn",
                AuthenticationServerIp = "AuthServerIpAddress",
                IsolatedNw1IpPool = "Nw1IpAddressPool",
                IsolatedNw1SecondaryDns = "Nw1SecondaryDns",
                IsolatedNw1IpRange = "Nw1IpAddressRange",
                IsolatedNw1PrimaryDns = "Nw1PrimaryDns",
                NasIp = "NasIpAddress",
                UserNameSuffix = "UserNameSuffix",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns"
            });
            _context.Add(_sim1 = new Sim { SimGroup = _simGroup1, Msisdn = "msisdn01", Imsi = "imsi01", IccId = "iccid01", UserName = "sim01", Password = "password" });
            _context.Add(_sim2 = new Sim { SimGroup = _simGroup1, Msisdn = "msisdn02", Imsi = "imsi02", IccId = "iccid02", UserName = "sim02", Password = "password" });
            _context.Add(_deviceGroup1 = new DeviceGroup { DomainId = _domain1.Id, Name = "deviceGroup1" });
            _context.Add(_device1 = new Device
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                ProductName = "",
                SerialNumber = "",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.Add(_device2 = new Device
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device02",
                UseTpm = true,
                ManagedNumber = "002",
                ProductName = "",
                SerialNumber = "",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.Add(_device3 = new Device
            {
                LteModule = _lte1,
                Domain = _domain2,
                Name = "device02",
                UseTpm = true,
                ManagedNumber = "002",
                ProductName = "",
                SerialNumber = "",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.Add(_simDevice1 = new SimAndDevice
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2020-03-07"),
            });
            _context.Add(_simDevice2 = new SimAndDevice
            {
                SimId = _sim2.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2020-03-07"),
            });
            _context.Add(_simDevice3 = new SimAndDevice
            {
                SimId = _sim1.Id,
                DeviceId = _device2.Id,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2020-03-07"),
            });
            _context.Add(_simDevice4 = new SimAndDevice
            {
                SimId = _sim2.Id,
                DeviceId = _device2.Id,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2020-03-07"),
            });
            _context.Add(_multiFactor1 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice1.Id,
                EndUserId = _user2.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor2 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice2.Id,
                EndUserId = _user2.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor3 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice3.Id,
                EndUserId = _user2.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor4 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice4.Id,
                EndUserId = _user2.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor5 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice1.Id,
                EndUserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor6 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice2.Id,
                EndUserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor7 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice3.Id,
                EndUserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor8 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice4.Id,
                EndUserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor9 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice1.Id,
                EndUserId = _user4.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor10 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice2.Id,
                EndUserId = _user4.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor11 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice3.Id,
                EndUserId = _user4.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(_multiFactor12 = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice4.Id,
                EndUserId = _user4.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor1 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor2 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor3 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor4 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor5 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor6 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor7 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor8 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor9 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor10 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor11 });
            _context.Add(new MultiFactorAuthenticated() { Expiration = DateTime.Now.AddHours(1.00), MultiFactor = _multiFactor12 });
            _context.SaveChanges();
        }

        /// <summary>
        /// ロール :  ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, body, _) = Utils.Get(_client, Url, "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, _, json) = Utils.Get(_client, Url, "user0", "user0");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("12", json["onlineUsers"]); // すべてのonlineUsers
            Assert.Equal("4", json["totalUsers"]);　//すべてUser
            Assert.Equal("3", json["totalDevices"]);　//すべての端末
            Assert.Equal("3", json["windowsDevices"]);
            Assert.Equal("0", json["androidDevices"]);
            Assert.Equal("0", json["linuxDevices"]);
            Assert.Equal("0", json["iosDevices"]);
        }


    }
}
