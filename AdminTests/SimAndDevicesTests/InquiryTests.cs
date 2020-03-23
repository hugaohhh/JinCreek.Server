using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimAndDevicesTests
{
    /// <summary>
    /// SIM端末組合せ照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public class InquiryTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/sim-and-devices";
        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly DeviceGroup _deviceGroup1;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly LteModule _lte1;
        private readonly Device _device1;
        private readonly Device _device2;
        private readonly Sim _sim1;
        private readonly Sim _sim1a;
        private readonly Sim _sim2;

        public InquiryTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin() { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_deviceGroup1 = new DeviceGroup() { Id = Guid.NewGuid(), Name = "_deviceGroup1", Domain = _domain1 });
            _context.Add(_lte1 = new LteModule() { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.Add(_simGroup1 = new SimGroup()
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
            _context.Add(_simGroup2 = new SimGroup
                { Id = Guid.NewGuid(), Name = "simGroup2", Organization = _org2, IsolatedNw1IpPool = "" });
            _context.Add(_sim1 = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_sim1a = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01a",
                Imsi = "imsi01a",
                IccId = "iccid01a",
                UserName = "sim01a",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_sim2 = new Sim() // 組織 : '他組織
            {
                Msisdn = "msisdn02",
                Imsi = "imsi02",
                IccId = "iccid02",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup2
            });
            _context.Add(_device1 = new Device()
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
            _context.Add(_device2 = new Device()
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device02",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// ロール :  ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, body, _) = Utils.Get(_client, Url, "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// ID:'UUID以外
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/abcde1234", "user0", "user0");//    スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }
        /// <summary>
        /// DBにSIM&端末組合せ：不在
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/{Guid.NewGuid()}", "user0", "user0");//    スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimAndDevice)]);
        }

        /// <summary>
        /// DBにSIM&端末組合せ：存在　利用終了日：存在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var simDeviceAuthenticationStateDone = new SimAndDeviceAuthenticated()
            {
                Expiration = DateTime.Now.AddHours(1.00)
            };
            var simDevice1 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2020-03-07"),
                SimAndDeviceAuthenticated = simDeviceAuthenticationStateDone
            };
            _context.Add(simDeviceAuthenticationStateDone);
            _context.Add(simDevice1);
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/{simDevice1.Id}", "user0", "user0");//    スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(simDevice1.Id.ToString(), json["id"]);
            Assert.Equal(simDevice1.Sim.Msisdn, json["sim"]["msisdn"]);
            Assert.Equal(simDevice1.Device.Name, json["device"]["name"]);
            Assert.Equal(simDevice1.IsolatedNw2Ip, json["isolatedNw2Ip"]);
            Assert.Equal(simDevice1.AuthenticationDuration, json["authenticationDuration"]);
            Assert.Equal(simDevice1.StartDate.ToString("yyyy-MM-dd"), json["startDate"]);
            Assert.Equal(simDevice1.EndDate?.ToString("yyyy-MM-dd"), json["endDate"]);
            Assert.Equal(simDeviceAuthenticationStateDone.Id, json["simAndDeviceAuthenticated"]["id"]);

        }

        /// <summary>
        /// DBにSIM&端末組合せ：存在　利用終了日：不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var simDeviceAuthenticationStateDone = new SimAndDeviceAuthenticated()
            {
                Expiration = DateTime.Now.AddHours(1.00)
            };
            var simDevice1 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                SimAndDeviceAuthenticated = simDeviceAuthenticationStateDone
            };
            _context.Add(simDeviceAuthenticationStateDone);
            _context.Add(simDevice1);
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/{simDevice1.Id}", "user0", "user0");//    スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(simDevice1.Id.ToString(), json["id"]);
            Assert.Equal(simDevice1.Sim.Msisdn, json["sim"]["msisdn"]);
            Assert.Equal(simDevice1.Device.Name, json["device"]["name"]);
            Assert.Equal(simDevice1.IsolatedNw2Ip, json["isolatedNw2Ip"]);
            Assert.Equal(simDevice1.AuthenticationDuration, json["authenticationDuration"]);
            Assert.Equal(simDevice1.StartDate.ToString("yyyy-MM-dd"), json["startDate"]);
            Assert.Null(json["endDate"].FirstOrDefault());
            Assert.Equal(simDeviceAuthenticationStateDone.Id, json["simAndDeviceAuthenticated"]["id"]);

        }


    }
}
