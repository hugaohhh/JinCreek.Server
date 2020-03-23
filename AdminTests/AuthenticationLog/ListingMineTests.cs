using JinCreek.Server.Admin.Controllers;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.AuthenticationLog
{
    /// <summary>
    /// 自分認証ログ一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ListingMineTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/authentication-logs/mine";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly User _user0;
        private readonly User _user1;
        private readonly User _user2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Device _device1;
        private readonly Device _device2;
        private readonly Sim _sim1A;
        private readonly Sim _sim1B;
        private readonly Sim _sim2;
        private readonly SimAndDevice _simDevice1;
        private readonly SimAndDevice _simDevice2;
        private readonly MultiFactor _multiFactor1;
        private readonly MultiFactor _multiFactor2;

        public ListingMineTests(CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", OrganizationCode = _org1.Code });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain2", OrganizationCode = _org2.Code });
            _context.Add(_user0 = new SuperAdmin { AccountName = "user0", Name = "", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user1 = new UserAdmin { AccountName = "user1", Name = "", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_user2 = new GeneralUser { AccountName = "xuser2a", Name = "", Domain = _domain1 });
            _context.Add(_simGroup1 = Utils.CreateSimGroup(organization: _org1, name: "simGroup1", isolatedNw1IpPool: "isolatedNw1IpPool"));
            _context.Add(_simGroup2 = Utils.CreateSimGroup(organization: _org2, name: "simGroup2", isolatedNw1IpPool: "isolatedNw1IpPool"));
            _context.Add(_sim1A = new Sim { SimGroup = _simGroup1, Msisdn = "1", Imsi = "1a", IccId = "1a", UserName = "userName", Password = "password" });
            _context.Add(_sim1B = new Sim { SimGroup = _simGroup1, Msisdn = "0123456789", Imsi = "1b", IccId = "1b", UserName = "userName", Password = "password" });
            _context.Add(_sim2 = new Sim { SimGroup = _simGroup2, Msisdn = "2", Imsi = "2", IccId = "2", UserName = "userName", Password = "password" });
            _context.Add(_device1 = Utils.CreateDevice(domain: _domain1, name: "device1"));
            _context.Add(_device2 = Utils.CreateDevice(domain: _domain1, name: "xdevice2a"));
            _context.Add(_simDevice1 = new SimAndDevice { Id = Guid.NewGuid(), SimId = _sim1A.Id, Device = _device1, IsolatedNw2Ip = "" });
            _context.Add(_simDevice2 = new SimAndDevice { Id = Guid.NewGuid(), SimId = _sim1A.Id, Device = _device2, IsolatedNw2Ip = "" });
            _context.Add(_multiFactor1 = new MultiFactor { Id = Guid.NewGuid(), SimAndDevice = _simDevice1, EndUserId = _user1.Id, ClosedNwIp = "" });
            _context.Add(_multiFactor2 = new MultiFactor { Id = Guid.NewGuid(), SimAndDevice = _simDevice2, EndUserId = _user2.Id, ClosedNwIp = "" });

            //_context.Add(new SimAndDeviceAuthenticationSuccessLog { Id = Guid.NewGuid(), SimAndDevice = _simDevice1 });
            //_context.Add(new SimAndDeviceAuthenticationFailureLog { Id = Guid.NewGuid(), Sim = _sim1 });
            //_context.Add(new MultiFactorAuthenticationSuccessLog { Id = Guid.NewGuid(), MultiFactor = _factorCombination1 });
            //_context.Add(new MultiFactorAuthenticationFailureLog { Id = Guid.NewGuid(), SimAndDevice = _simDevice1 });
            //_context.Add(new DeauthenticationLog { Id = Guid.NewGuid(), MultiFactor = _factorCombination1 });
            _context.SaveChanges();
        }

        /// <summary>
        /// フィルター：不在、ページ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A });
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim2 }); // 他組織
            _context.SaveChanges();

            var query = new
            {
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(5, (int)json["count"]); // トータル件数
            Assert.Equal("2020-01-01T04:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T03:00:00", (string)list[1]["time"]);
            Assert.Equal("2020-01-01T02:00:00", (string)list[2]["time"]);
            Assert.Equal("2020-01-01T01:00:00", (string)list[3]["time"]);
            Assert.Equal("2020-01-01T00:00:00", (string)list[4]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// ページ：0
        /// </summary>
        [Fact]
        public void Case02()
        {
            var query = new
            {
                page = 0,
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]["Page"]);
        }

        /// <summary>
        /// フィルター：不在、ページ：最終ページ超過
        /// </summary>
        [Fact]
        public void Case03()
        {
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A });
            _context.SaveChanges();

            var query = new
            {
                page = 2,
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
        }

        /// <summary>
        /// フィルター：通信時刻From、ページ：中間ページ、ページサイズ：不在、ソート：通信時刻昇順
        /// </summary>
        [Fact]
        public void Case04()
        {
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            for (var i = 0; i < 20; i++) _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1A });
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim2 }); // 他組織
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T05:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            for (var i = 0; i < 19; i++) _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T06:00:00"), Sim = _sim1A });
            _context.SaveChanges();

            var query = new
            {
                timeFrom = "2020-01-01T01:00:00",
                page = 2,
                sortBy = "time",
                orderBy = "asc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(42, (int)json["count"]); // トータル件数
            Assert.Equal(20, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T02:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T03:00:00", (string)list[1]["time"]);
            Assert.Equal("2020-01-01T05:00:00", (string)list[2]["time"]); // 通信時刻昇順
        }

        /// <summary>
        /// フィルター：SIM、ページ：最終ページ、ページサイズ：不在、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case05()
        {
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            for (var i = 0; i < 39; i++) _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1B });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1B });
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1B, MultiFactor = _multiFactor1 });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim2 }); // 他組織
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T05:00:00"), Sim = _sim1B, SimAndDevice = _simDevice1 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T07:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                simMsisdn = "3456",
                page = 3,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(42, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T01:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T01:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：端末、ページ：中間ページ、ページサイズ：2、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case06()
        {
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A, MultiFactor = _multiFactor2 });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim2 }); // 他組織
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, MultiFactor = _multiFactor2 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim1A, MultiFactor = _multiFactor2 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T05:00:00"), Sim = _sim1A, MultiFactor = _multiFactor2 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T06:00:00"), Sim = _sim1A, MultiFactor = _multiFactor2 });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T07:00:00"), Sim = _sim1B }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                deviceName = "device2",
                page = 2,
                pageSize = 2,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(5, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T04:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T03:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：ユーザ、ページ：1、ページサイズ：2、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1B, MultiFactor = _multiFactor1 }); // 範囲外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1B, MultiFactor = _multiFactor2 });
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1B, MultiFactor = _multiFactor2 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, MultiFactor = _multiFactor2 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                userAccountName = "user2",
                page = 1,
                pageSize = 4,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(3, (int)json["count"]); // トータル件数
            Assert.True(4 > list.Count); // 1ページ当たり表示件数 : '指定件数未満
            Assert.Equal("2020-01-01T03:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T02:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：内容=SIM＆端末認証、ページ：中間ページ、ページサイズ：不在、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case08()
        {
            for (var i = 0; i < 20; i++) _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            for (var i = 0; i < 20; i++) _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A }); // 範囲外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T05:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T07:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                type = nameof(AuthenticationLogsController.TypeKey.SimAndDeviceAuthenticationSuccessLog),
                page = 2,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(42, (int)json["count"]); // トータル件数
            Assert.Equal(20, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T02:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T01:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：内容=SIM＆端末認証失敗、ページ：最終ページ、ページサイズ：2、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1A });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A });
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim1A });
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                type = nameof(AuthenticationLogsController.TypeKey.SimAndDeviceAuthenticationFailureLog),
                page = 2,
                pageSize = 3,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(5, (int)json["count"]); // トータル件数
            Assert.True(3 > list.Count); // 1ページ当たり表示件数 : '指定件数未満
            Assert.Equal("2020-01-01T01:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T00:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：内容=多要素認証、ページ：中間ページ、ページサイズ：不在、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case10()
        {
            for (var i = 0; i < 20; i++) _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            for (var i = 0; i < 20; i++) _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A }); // 範囲外
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                type = nameof(AuthenticationLogsController.TypeKey.MultiFactorAuthenticationSuccessLog),
                page = 2,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(42, (int)json["count"]); // トータル件数
            Assert.Equal(20, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T02:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T01:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：内容=多要素認証失敗、ページ：1、ページサイズ：不在、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case11()
        {
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 });
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A }); // 範囲外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                type = nameof(AuthenticationLogsController.TypeKey.MultiFactorAuthenticationFailureLog),
                page = 1,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(2, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T01:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T00:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：内容=認証解除、ページ：中間ページ、ページサイズ：2、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case12()
        {
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T04:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A }); // 範囲外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, SimAndDevice = _simDevice1 }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                type = nameof(AuthenticationLogsController.TypeKey.DeauthenticationLog),
                page = 2,
                pageSize = 2,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(5, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T02:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T01:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：通信時刻・SIM・端末・ユーザ・内容=多要素認証、ページ：最終ページ、ページサイズ：不在、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case13()
        {
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T00:00:00"), Sim = _sim1B, MultiFactor = _multiFactor2 }); // 範囲外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A, MultiFactor = _multiFactor2 }); // 範囲外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 範囲外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1B, MultiFactor = _multiFactor2 });
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1B, MultiFactor = _multiFactor2 });
            for (var i = 0; i < 20; i++) _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1B, MultiFactor = _multiFactor2 });
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T06:00:00"), Sim = _sim1B, MultiFactor = _multiFactor2 }); // 範囲外
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1B, SimAndDevice = _simDevice2 }); // 範囲外
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1B }); // 範囲外
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1B, SimAndDevice = _simDevice2 }); // 範囲外
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1B, MultiFactor = _multiFactor2 }); // 範囲外
            _context.SaveChanges();

            var query = new
            {
                timeFrom = "2020-01-01T01:00:00",
                timeTo = "2020-01-01T03:00:00",
                simMsisdn = "345",
                deviceName = "device2",
                userAccountName = "user2",
                type = nameof(AuthenticationLogsController.TypeKey.MultiFactorAuthenticationSuccessLog),
                page = 2,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(22, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T02:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T01:00:00", (string)list[1]["time"]); // 通信時刻降順
        }

        /// <summary>
        /// フィルター：通信時刻・SIM・端末・ユーザ・内容=多要素認証失敗、ページ：1、ページサイズ：不在、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case14()
        {
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T00:00:00"), SimAndDevice = _simDevice2 }); // 対象外(time)
            _context.Add(new MultiFactorAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), SimAndDevice = _simDevice1 }); // 対象外(SimAndDevice)
            _context.Add(new SimAndDeviceAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), SimAndDevice = _simDevice2 }); // 対象外
            _context.Add(new SimAndDeviceAuthenticationFailureLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1B }); //対象外
            _context.Add(new MultiFactorAuthenticationSuccessLog { Time = DateTime.Parse("2020-01-01T01:00:00"), MultiFactor = _multiFactor2 }); // 対象外
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T01:00:00"), MultiFactor = _multiFactor2 }); // 対象外
            _context.SaveChanges();

            var query = new
            {
                timeFrom = "2020-01-01T01:00:00",
                timeTo = "2020-01-01T03:00:00",
                simMsisdn = "345",
                deviceName = "device2",
                userAccountName = "user2",
                type = nameof(AuthenticationLogsController.TypeKey.MultiFactorAuthenticationFailureLog),
                page = 1,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(0, (int)json["count"]); // トータル件数
        }

        /// <summary>
        /// フィルター：通信時刻To、ページ：1、ページサイズ：不在、ソート：通信時刻降順
        /// </summary>
        [Fact]
        public void Case15()
        {
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T01:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T02:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 });
            _context.Add(new DeauthenticationLog { Time = DateTime.Parse("2020-01-01T03:00:00"), Sim = _sim1A, MultiFactor = _multiFactor1 }); // 対象外
            _context.SaveChanges();

            var query = new
            {
                timeTo = "2020-01-01T02:00:00",
                page = 1,
                sortBy = "time",
                orderBy = "desc",
            };
            var (response, body, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, "domain1"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(2, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count); // 1ページ当たり表示件数
            Assert.Equal("2020-01-01T02:00:00", (string)list[0]["time"]);
            Assert.Equal("2020-01-01T01:00:00", (string)list[1]["time"]); // 通信時刻降順
        }
    }
}
