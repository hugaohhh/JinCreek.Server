using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.MultiFactorTests
{
    /// <summary>
    /// 自分認証要素組合せ照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public class InquiryMineTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/multi-factors/mine";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly DeviceGroup _deviceGroup1;
        private readonly Device _device1;
        private readonly SimAndDevice _simDevice1;
        private readonly SimAndDevice _simDevice2;
        private readonly User _user0;
        private readonly User _user1;
        private readonly User _user2;
        private readonly Sim _sim1;
        private readonly Sim _sim2;

        private readonly MultiFactor _multiFactor2;

        public InquiryMineTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
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
            _context.Add(_user0 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user1 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), DomainId = _domain1.Id }); // ユーザー管理者
            _context.Add(_user2 = new GeneralUser() { AccountName = "user2", DomainId = _domain2.Id });
            _context.Add(_simGroup1 = new SimGroup
            {
                Id = Guid.NewGuid(),
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
            _context.Add(_simGroup2 = new SimGroup { Id = Guid.NewGuid(), Name = "simGroup2", OrganizationCode = _org2.Code });
            _context.Add(_sim1 = new Sim { SimGroup = _simGroup1, Msisdn = "msisdn01", Imsi = "imsi01", IccId = "iccid01", UserName = "sim01", Password = "password" });
            _context.Add(_sim2 = new Sim { SimGroup = _simGroup1, Msisdn = "msisdn01", Imsi = "imsi01", IccId = "iccid01", UserName = "sim01", Password = "password" });//他組織
            _context.Add(_deviceGroup1 = new DeviceGroup { Id = Guid.NewGuid(), DomainId = _domain1.Id, Name = "deviceGroup1" });
            _context.Add(_device1 = new Device { Id = Guid.NewGuid(), DomainId = _domain1.Id, Name = "device1", ManagedNumber = "2", UseTpm = true, WindowsSignInListCacheDays = 1 });
            _context.Add(_device1 = new Device { Id = Guid.NewGuid(), DomainId = _domain2.Id, Name = "device2", ManagedNumber = "2", UseTpm = true, WindowsSignInListCacheDays = 1 });//他組織
            _context.Add(_simDevice1 = new SimAndDevice { SimId = _sim1.Id, DeviceId = _device1.Id });
            _context.Add(_simDevice2 = new SimAndDevice { SimId = _sim2.Id, DeviceId = _device1.Id }); // sim 他組織
            //_context.Add(_simDevice3 = new SimAndDevice { SimId = _sim1.Id, DeviceId = _device2.Id }); // 端末 他組織
            _context.Add(_multiFactor2 = new MultiFactor { Id = Guid.NewGuid(), SimAndDeviceId = _simDevice1.Id, EndUserId = _user2.Id }); //User他組織
            _context.SaveChanges();
        }

        /// <summary>
        /// ID:'UUID以外
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/abcde1234", "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }
        /// <summary>
        /// DBに認証要素組合せ　：不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/{Guid.NewGuid()}", "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(MultiFactor)]);
        }

        /// <summary>
        /// DBに認証要素組合せ　：'存在（他組織）
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/{_multiFactor2.Id}", "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// DBに認証要素組合せ：存在　利用終了日：存在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var multiFactor = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice1.Id,
                EndUserId = _user1.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse("2020-02-07")
            };
            _context.Add(multiFactor);
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/{multiFactor.Id}", "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(multiFactor.Id, json["id"]);
            Assert.Equal(multiFactor.SimAndDevice.Sim.Msisdn, json["simAndDevice"]["sim"]["msisdn"]);
            Assert.Equal(multiFactor.SimAndDevice.Device.Name, json["simAndDevice"]["device"]["name"]);
            Assert.Equal(multiFactor.EndUser.AccountName, json["endUser"]["accountName"]);
            Assert.Equal(multiFactor.StartDate.ToString("yyyy-MM-dd"), json["startDate"]);
            Assert.Equal(multiFactor.EndDate?.ToString("yyyy-MM-dd"), json["endDate"]);
            Assert.Equal(multiFactor.ClosedNwIp, json["closedNwIp"]);
        }

        /// <summary>
        /// DBに認証要素組合せ：存在　利用終了日：不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var multiFactor = new MultiFactor
            {
                Id = Guid.NewGuid(),
                SimAndDeviceId = _simDevice1.Id,
                EndUserId = _user1.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
            };
            _context.Add(multiFactor);
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/{multiFactor.Id}", "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(multiFactor.Id, json["id"]);
            Assert.Equal(multiFactor.SimAndDevice.Sim.Msisdn, json["simAndDevice"]["sim"]["msisdn"]);
            Assert.Equal(multiFactor.SimAndDevice.Device.Name, json["simAndDevice"]["device"]["name"]);
            Assert.Equal(multiFactor.EndUser.AccountName, json["endUser"]["accountName"]);
            Assert.Equal(multiFactor.StartDate.ToString("yyyy-MM-dd"), json["startDate"]);
            Assert.Empty(json["endDate"]);
            Assert.Equal(multiFactor.ClosedNwIp, json["closedNwIp"]);
        }

    }
}
