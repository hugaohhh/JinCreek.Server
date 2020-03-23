using JinCreek.Server.Admin;
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
    /// 認証要素組合せ登録
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    public class RegisterTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;
        private const string Url = "/api/multi-factors";

        private readonly Organization _org1;
        private readonly Domain _domain1;
        private readonly UserGroup _userGroup1;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly EndUser _user3;
        private readonly SimGroup _simGroup1;
        private readonly Sim _sim1;
        private readonly Sim _sim2;
        private readonly DeviceGroup _deviceGroup1;
        private readonly LteModule _lteModule1;
        private readonly Device _device1;

        private readonly SimAndDevice _simAndDevice1;

        public RegisterTests(CustomWebApplicationFactoryWithMariaDb<Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2021-01-30")));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup1" });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Name = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Name = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_user3 = new GeneralUser { AccountName = "user2", Name = "user2", DomainId = _domain1.Id });
            _context.Add(new UserGroupEndUser() { EndUser = _user2, UserGroup = _userGroup1 });
            _context.Add(new AvailablePeriod { EndUser = _user2, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(new AvailablePeriod { EndUser = _user3, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(_deviceGroup1 = new DeviceGroup { Id = Guid.NewGuid(), Name = "_deviceGroup1", Domain = _domain1 });
            _context.Add(_lteModule1 = new LteModule { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.Add(_device1 = new Device()
            {
                LteModule = _lteModule1,
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
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
            _context.Add(_sim1 = new Sim() // 組織 : '自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_sim2 = new Sim() // 組織 : '自組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_simAndDevice1 = new SimAndDevice()
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// 異常：ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var obj = new { };
            var (response, body, _) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// 全部空
        /// </summary>
        [Fact]
        public void Case02()
        {
            var obj = new
            {

            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("The SimAndDeviceId field is required.", json["errors"]?["SimAndDeviceId"].First.ToString());
            Assert.Equal("The UserId field is required.", json["errors"]?["UserId"].First.ToString());
            Assert.Equal("The ClosedNwIp field is required.", json["errors"]?["ClosedNwIp"].First.ToString());
            Assert.Equal("The StartDate field is required.", json["errors"]?["StartDate"].First.ToString());
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case03A()
        {
            var obj = new
            {
                SimAndDeviceId = "1231546", // UUIDではない
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-06",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["SimAndDeviceId"]);
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case03B()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = "121566546",// UUIDではない
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-06",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["UserId"]);
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case03C()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.10000", // IpAddressではない
                StartDate = "2020-02-12",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["ClosedNwIp"]);
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case03D()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-1112", //'存在しない日付
                EndDate = "2020-02-2117" // '存在しない日付
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["StartDate"]);
            Assert.NotNull(json["errors"]?["EndDate"]);
        }

        /// <summary>
        /// 利用終了日:'利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case04()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-27",
                EndDate = "2020-02-26"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.InvalidEndDate, json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// DBに SIM＆端末組合せ不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var obj = new
            {
                SimAndDeviceId = Guid.NewGuid(),
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-02",
                EndDate = "2020-02-12"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimAndDevice)]);
        }

        /// <summary>
        /// DBに ユーザー不在
        /// </summary>
        [Fact]
        public void Case06()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = Guid.NewGuid(),
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2020-02-18"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(EndUser)]);
        }


        /// <summary>
        ///'同組合せが他レコードに重複して存在
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(new MultiFactor()
            {
                SimAndDeviceId = _simAndDevice1.Id,
                EndUserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse("2020-02-09")
            });
            _context.SaveChanges();
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-01-12"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["MultiFactor"]);
        }

        /// <summary>
        /// 全部正常 利用終了日:存在
        /// </summary>
        [Fact]
        public void Case08()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.NotNull(json["id"]);
            Assert.Equal(obj.ClosedNwIp, json["closedNwIp"].ToString());
            Assert.Equal("2020-02-17", (string)json["startDate"]);
            Assert.Equal("2020-02-27", (string)json["endDate"]);
            Assert.NotNull(json["simAndDeviceId"]);
            Assert.NotNull(json["endUserId"]);
        }

        /// <summary>
        /// 全部正常 利用終了日:不在
        /// </summary>
        [Fact]
        public void Case09()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.NotNull(json["id"]);
            Assert.Equal(obj.ClosedNwIp, json["closedNwIp"].ToString());
            Assert.Equal("2020-02-17", (string)json["startDate"]);
            Assert.NotNull(json["simAndDeviceId"]);
            Assert.NotNull(json["endUserId"]);
            Assert.Empty(json["endDate"]);
        }

        /// <summary>
        /// 利用開始日 'SIM＆端末組合せ利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case10()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-01-01", 
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用開始日 'SIM＆端末組合せ利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case11()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2021-02-15",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
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
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-07",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
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
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2021-03-07",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用終了日:'SIM＆端末組合せ利用開始日 (2020-02-01) 未満の日付
        /// </summary>
        [Fact]
        public void Case14()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-01-31",
                EndDate = "2020-01-31",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日:'SIM＆端末組合せ利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case15()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-02-25",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
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
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-09",
                EndDate = "2020-02-09",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
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
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-03-25",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); //スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
        }

    }
}
