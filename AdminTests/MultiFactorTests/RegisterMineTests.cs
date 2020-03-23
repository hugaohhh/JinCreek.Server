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
    /// 自分認証要素組合せ登録
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    public class RegisterMineTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;
        private const string Url = "/api/multi-factors/mine";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly EndUser _user3;
        private readonly EndUser _user4;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Sim _sim1;
        private readonly Sim _sim2;
        private readonly DeviceGroup _deviceGroup1;
        private readonly LteModule _lteModule1;
        private readonly Device _device1;
        private readonly Device _device2;

        private readonly SimAndDevice _simAndDevice1;
        private readonly SimAndDevice _simAndDevice2;
        private readonly SimAndDevice _simAndDevice3;

        public RegisterMineTests(CustomWebApplicationFactoryWithMariaDb<Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2021-01-30")));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2021-01-30")));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup1" });
            _context.Add(_user1 = new SuperAdmin {AccountName = "user0", Name = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin {AccountName = "user1", Name = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_user3 = new GeneralUser {AccountName = "user2", Name = "user2", Domain = _domain1 });
            _context.Add(new UserGroupEndUser { EndUser = _user2, UserGroup = _userGroup1 });
            _context.Add(new AvailablePeriod { EndUser = _user2, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(new AvailablePeriod { EndUser = _user3, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(_user4 = new GeneralUser {AccountName = "user4", Name = "user4", Domain = _domain2 });
            _context.Add(_deviceGroup1 = new DeviceGroup { Id = Guid.NewGuid(), Name = "_deviceGroup1", Domain = _domain1 });
            _context.Add(_lteModule1 = new LteModule { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.Add(_device1 = new Device
            {
                LteModule = _lteModule1,
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
            _context.Add(_device2 = new Device() // 他組織
            {
                LteModule = _lteModule1,
                Domain = _domain2,
                Name = "device02",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });

            _context.Add(_simGroup1 = Utils.CreateSimGroup(organization: _org1, name: "simGroup1", isolatedNw1IpPool: "Nw1IpAddressPool"));
            _context.Add(_simGroup2 = Utils.CreateSimGroup(organization: _org2, name: "simGroup2", isolatedNw1IpPool: "Nw1IpAddressPool"));
            _context.Add(_sim1 = new Sim() // 組織 : '自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
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
            _context.Add(_simAndDevice1 = Utils.CreateSimAndDevice(sim: _sim1, device: _device1, startDate: DateTime.Parse("2020-02-01"), endDate: DateTime.Parse("2021-02-01")));
            _context.Add(_simAndDevice2 = Utils.CreateSimAndDevice(sim: _sim2, device: _device1)); // sim 他組織
            _context.Add(_simAndDevice3 = Utils.CreateSimAndDevice(sim: _sim1, device: _device2)); // 端末 他組織
            _context.SaveChanges();
        }

        /// <summary>
        /// 全部空
        /// </summary>
        [Fact]
        public void Case01()
        {
            var obj = new
            {

            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
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
        public void Case02A()
        {
            var obj = new
            {
                SimAndDeviceId = "1231546", // UUIDではない
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-06",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["SimAndDeviceId"]);
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case02B()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = "121566546",// UUIDではない
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-06",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["UserId"]);
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case02C()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.10000", // IpAddressではない
                StartDate = "2020-02-12",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["ClosedNwIp"]);
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case02D()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-1112", //'存在しない日付
                EndDate = "2020-02-2117" // '存在しない日付
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["StartDate"]);
            Assert.NotNull(json["errors"]?["EndDate"]);
        }

        /// <summary>
        /// 利用終了日:'利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case03()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-27",
                EndDate = "2020-02-12"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.InvalidEndDate, json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// DBに SIM＆端末組合せ不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var obj = new
            {
                SimAndDeviceId = Guid.NewGuid(),
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-02",
                EndDate = "2020-02-12"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimAndDevice)]);
        }

        /// <summary>
        /// DBに ユーザー不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = Guid.NewGuid(),
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-02",
                EndDate = "2020-02-12"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(EndUser)]);
        }

        /// <summary>
        /// DBに SIM:他組織
        /// </summary>
        [Fact]
        public void Case06()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice2.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-01-25"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// DBに Device:他組織
        /// </summary>
        [Fact]
        public void Case07()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice3.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-01-25"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// DBに ユーザー:他組織
        /// </summary>
        [Fact]
        public void Case08()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice3.Id,
                UserId = _user4.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-02",
                EndDate = "2020-02-12"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        ///'同組合せが他レコードに重複して存在
        /// </summary>
        [Fact]
        public void Case09()
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
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["MultiFactor"]);
        }

        /// <summary>
        /// 全部正常 利用終了日:存在
        /// </summary>
        [Fact]
        public void Case10()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2020-02-27"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
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
        public void Case11()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
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
        public void Case12()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-01-01",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用開始日 'SIM＆端末組合せ利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case13()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2021-02-15",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }


        /// <summary>
        /// 利用開始日 'ユーザー利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case14()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-07",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用開始日 'ユーザー利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case15()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2021-03-07",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用終了日:'SIM＆端末組合せ利用開始日 (2020-02-01) 未満の日付
        /// </summary>
        [Fact]
        public void Case16()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-01-31",
                EndDate = "2020-01-31",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日:'SIM＆端末組合せ利用終了日以降の日付
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
                EndDate = "2021-02-25",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日:''ユーザー利用開始日 (2020-02-10) 未満の日付
        /// </summary>
        [Fact]
        public void Case18()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-09",
                EndDate = "2020-02-09",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日:'ユーザー利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case19()
        {
            var obj = new
            {
                SimAndDeviceId = _simAndDevice1.Id,
                UserId = _user3.Id,
                ClosedNwIp = "127.0.0.1",
                StartDate = "2020-02-17",
                EndDate = "2021-03-25",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
        }
    }
}
