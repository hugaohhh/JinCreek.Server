using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using JinCreek.Server.Admin;
using Xunit;

namespace JinCreek.Server.AdminTests.MultiFactorTests
{
    /// <summary>
    /// 認証要素組合せ更新
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public class UpdateTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly MainDbContext _context;

        private const string Url = "/api/multi-factors";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly EndUser _user3;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Device _device1;
        private readonly Sim _sim1;
        private readonly SimAndDevice _simDevice1;

        private readonly MultiFactor _multiFactor1;

        private readonly AvailablePeriod _availablePeriod;

        public UpdateTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
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
            _context.Add(_user2 = new UserAdmin() { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_user3 = new GeneralUser() { AccountName = "user2", DomainId = _domain1.Id });
            _context.Add(_availablePeriod = new AvailablePeriod()
                { EndUser = _user2, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(_availablePeriod = new AvailablePeriod()
                { EndUser = _user3, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
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
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.Add(_multiFactor1 = new MultiFactor
            {
                SimAndDeviceId = _simDevice1.Id,
                EndUserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var obj = new { };
            var (response, body, _) = Utils.Put(_client, $"{Url}/{_simDevice1.Id}", Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// ID不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var obj = new { };
            var (response, body, _) = Utils.Put(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// IDがUUIDでない
        /// </summary>
        [Fact]
        public void Case03()
        {
            var obj = new
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/aa", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// 入力が不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var obj = new { };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Equal("The ClosedNwIp field is required.", json["errors"]?["ClosedNwIp"].First.ToString());
            Assert.Equal("The StartDate field is required.", json["errors"]?["StartDate"].First.ToString());
        }

        /// <summary>
        /// 入力値が全部不正パターン
        /// </summary>
        [Fact]
        public void Case05A()
        {
            var obj = new
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-98", // '存在しない日付
                EndDate = "2020-02-98" // '存在しない日付
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["StartDate"]);
            Assert.NotNull(json["errors"]?["EndDate"]);
        }

        /// <summary>
        /// 入力値が全部不正パターン
        /// </summary>
        [Fact]
        public void Case05B()
        {
            var obj = new
            {
                ClosedNwIp = "1346546", // IpAddressではない
                StartDate = "2020-02-07",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Equal("The field is not a valid IP address.", json["errors"]?["ClosedNwIp"].First.ToString());
        }

        /// <summary>
        /// '利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case06()
        {
            var obj = new
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-27",
                EndDate = "2020-02-26"  // '利用開始日未満の日付
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Equal(Messages.InvalidEndDate, json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// DBに 認証要素組合せが不在
        /// </summary>
        [Fact]
        public void Case07()
        {
            var obj = new
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{Guid.NewGuid()}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"][nameof(MultiFactor)]);
        }

        /// <summary>
        /// 全部正常 利用終了日:存在
        /// </summary>
        [Fact]
        public void Case08()
        {
            var obj = new
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-01-27"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_multiFactor1.Id, json["id"]);
            Assert.Equal(obj.ClosedNwIp, json["closedNwIp"].ToString());
            Assert.Equal("2020-02-17", (string)json["startDate"]);
            Assert.Equal("2021-01-27", (string)json["endDate"]);
            Assert.Equal(_multiFactor1.SimAndDeviceId.ToString(), json["simAndDeviceId"]);
            Assert.Equal(_multiFactor1.EndUserId.ToString(), json["endUserId"]);
        }

        /// <summary>
        /// 全部正常 利用終了日:不在
        /// </summary>
        [Fact]
        public void Case09()
        {
            var obj = new
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(_multiFactor1.Id, json["id"]);
            Assert.Equal(obj.ClosedNwIp, json["closedNwIp"].ToString());
            Assert.Equal("2020-02-17", (string)json["startDate"]);
            Assert.Equal(_multiFactor1.SimAndDeviceId.ToString(), json["simAndDeviceId"]);
            Assert.Equal(_multiFactor1.EndUserId.ToString(), json["endUserId"]);
            Assert.Empty(json["endDate"]);
        }

        /// <summary>
        /// 利用開始日 'SIM&端末組合せ利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case10()
        {
            var obj = new
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-01-01",
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用開始日 'SIM&端末組合せ利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case11()
        {
            var obj = new
            {
                SimAndDeviceId = _simDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2021-02-15",
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用開始日 'ユーザー利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case12()
        {
            var obj = new
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-07",
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用開始日 'ユーザー利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case13()
        {
            var obj = new
            {
                SimAndDeviceId = _simDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2021-03-07",
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用終了日:'SIM&端末組合せ利用開始日 (2020-02-01) 未満の日付
        /// </summary>
        [Fact]
        public void Case14()
        {
            var obj = new
            {
                SimAndDeviceId = _simDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-01-31",
                EndDate = "2020-01-31",
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日:'SIM&端末組合せ利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case15()
        {
            var obj = new
            {
                SimAndDeviceId = _simDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-02-25",
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日:''ユーザー利用開始日 (2020-02-10) 未満の日付
        /// </summary>
        [Fact]
        public void Case16()
        {
            var obj = new
            {
                SimAndDeviceId = _simDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-09",
                EndDate = "2020-02-09",
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日:'ユーザー利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case17()
        {
            var obj = new
            {
                SimAndDeviceId = _simDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-03-25",
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_multiFactor1.Id}", Utils.CreateJsonContent(obj), "user0", "user0");// スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
        }

    }
}
