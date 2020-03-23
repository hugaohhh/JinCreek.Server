using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JinCreek.Server.AdminTests.SimTests
{
    /// <summary>
    /// 自分SIM削除
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class DeleteMineTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;
        private const string Url = "/api/sims/mine";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Sim _sim;
        private readonly Sim _sim2;
        private readonly Device _device1;

        public DeleteMineTests(CustomWebApplicationFactoryWithMariaDb<Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Name = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Name = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_simGroup1 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup1",
                Organization = _org1,
                Apn = "apn",
                AuthenticationServerIp = "AuthenticationServerIp",
                IsolatedNw1IpPool = "IsolatedNw1IpPool",
                IsolatedNw1SecondaryDns = "IsolatedNw1SecondaryDns",
                IsolatedNw1IpRange = "IsolatedNw1IpRange",
                IsolatedNw1PrimaryDns = "IsolatedNw1PrimaryDns",
                NasIp = "NasIp",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns",
                UserNameSuffix = "UserNameSuffix",
            });
            _context.Add(_simGroup2 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup2",
                Organization = _org2,
                Apn = "apn",
                AuthenticationServerIp = "AuthenticationServerIp",
                IsolatedNw1IpPool = "IsolatedNw1IpPool",
                IsolatedNw1SecondaryDns = "IsolatedNw1SecondaryDns",
                IsolatedNw1IpRange = "IsolatedNw1IpRange",
                IsolatedNw1PrimaryDns = "IsolatedNw1PrimaryDns",
                NasIp = "NasIp",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns",
                UserNameSuffix = "UserNameSuffix",
            });
            _context.Add(_sim = new Sim()
            {
                SimGroup = _simGroup1,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password"
            });
            _context.Add(_sim2 = new Sim()
            {
                SimGroup = _simGroup2,
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password"
            });
            _context.Add(_device1 = new Device()
            {
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                ProductName = "",
                SerialNumber = ""
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// IDがない
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, body, _) = Utils.Delete(_client, $"{Url}/", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// IDがUUIDではない
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/aaaaaaaaaaa", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// DBにSimが不在
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{Guid.NewGuid()}", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Sim)]);
        }

        /// <summary>
        /// DBにSimが存在(他組織)
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{_sim2.Id}", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{_sim.Id}", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_sim.Id.ToString(), json["id"].ToString());
        }

        /// <summary>
        /// DBに　SIM＆端末組み合わせ　が存在
        /// </summary>
        [Fact]
        public void Case06()
        {
            _context.Add(new SimAndDevice()
            {
                Sim = _sim,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse("2020-02-09")
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Delete(_client, $"{Url}/{_sim.Id}", token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 不在
            Assert.NotNull(json["errors"]?["SimAndDevice"]);
        }

        /// <summary>
        /// DBに　SIM＆端末認証失敗　が存在
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(new SimAndDeviceAuthenticationFailureLog()
            {
                Sim = _sim,
                Time = DateTime.Now
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Delete(_client, $"{Url}/{_sim.Id}", token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 不在
            Assert.NotNull(json["errors"][nameof(SimAndDeviceAuthenticationFailureLog)]);
        }
    }
}
