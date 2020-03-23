using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimAndDevicesTests
{
    /// <summary>
    /// SimDevice削除
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class DeleteTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;
        private const string Url = "/api/sim-and-devices";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Device _device1;
        private readonly Sim _sim1;
        private readonly Sim _sim2;
        private readonly SimAndDevice _simDevice1;

        public DeleteTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup1", Domain = _domain1 });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup2", Domain = _domain2 });

            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
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
            _context.Add(_sim2 = new Sim() // 組織 : '他組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup2
            });
            _context.Add(_device1 = new Device()
            {
                LteModule = null,
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
            _context.Add(_simDevice1 = new SimAndDevice()
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse("2020-02-09")
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response,body,_) = Utils.Delete(_client, $"{Url}/{_simDevice1.Id}", "user1","user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// IDがない
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, body, _) = Utils.Delete(_client, $"{Url}/", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// IDがUUIDではない
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/aaa", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// DBにSimDeviceが不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{Guid.NewGuid()}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimAndDevice)]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{_simDevice1.Id}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_simDevice1.Id.ToString(), json["id"].ToString());
        }

        /// <summary>
        /// DBにSimDeviceが存在　多要素組み合わせも存在
        /// </summary>
        [Fact]
        public void Case06()
        {
            _context.Add(new MultiFactor()
            {
                SimAndDevice = _simDevice1,
                ClosedNwIp = "127.0.0.1",
                EndUser = _user2,
                StartDate = DateTime.Now,
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{_simDevice1.Id}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(MultiFactor)]);
        }

        /// <summary>
        /// DBにSimDeviceが存在　多要素認証失敗も存在
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(new MultiFactorAuthenticationFailureLog()
            {
                SimAndDevice = _simDevice1,
                Time = DateTime.Now,
                Sim = _sim1
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{_simDevice1.Id}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(MultiFactorAuthenticationFailureLog)]);
        }

        /// <summary>
        /// DBにSimDeviceが存在　SIMDevice 認証成功も存在
        /// </summary>
        [Fact]
        public void Case08()
        {
            _context.Add(new SimAndDeviceAuthenticationSuccessLog()
            {
                SimAndDevice = _simDevice1,
                Time = DateTime.Now,
                Sim = _sim1
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{_simDevice1.Id}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimAndDeviceAuthenticationSuccessLog)]);
        }

        /// <summary>
        /// DBに simDevice が存在　simDevice認証済みも存在
        /// </summary>
        [Fact]
        public void Case09()
        {
            _context.Add(new SimAndDeviceAuthenticated()
            {
                SimAndDevice = _simDevice1,
                Expiration = DateTime.Now.AddHours(1.00)
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Delete(_client, $"{Url}/{_simDevice1.Id}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimAndDeviceAuthenticated)]);
        }
    }
}
