using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using JinCreek.Server.Admin;
using Xunit;

namespace JinCreek.Server.AdminTests.SimAndDevicesTests
{
    /// <summary>
    /// 自分SimDevice登録
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class RegisterMineTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;
        private const string Url = "/api/sim-and-devices/mine";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroupEndUser _userGroupEndUser;
        private readonly AvailablePeriod _availablePeriod;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Sim _sim1;
        private readonly Sim _sim2;
        private readonly Sim _sim3;
        private readonly DeviceGroup _deviceGroup1;
        private readonly LteModule _lte1;
        private readonly Device _device1;
        private readonly Device _device2;

        public RegisterMineTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup1" });
            _context.Add(_userGroupEndUser = new UserGroupEndUser() { EndUser = _user2, UserGroup = _userGroup1 });
            _context.Add(_availablePeriod = new AvailablePeriod()
                { EndUser = _user2, EndDate = DateTime.Now.AddHours(6.00), StartDate = DateTime.Now.AddHours(-6.00) });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin() { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_deviceGroup1 = new DeviceGroup() { Id = Guid.NewGuid(), Name = "_deviceGroup1", Domain = _domain1 });
            _context.Add(_lte1 = new LteModule() { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.Add(_device1 = new Device()
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-01-30"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.Add(_device2 = new Device()
            {
                LteModule = _lte1,
                Domain = _domain2,
                Name = "device02",
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
            _context.Add(_simGroup2 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup2",
                Organization = _org2,
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
            _context.Add(_sim3 = new Sim() // 組織 : '他組織
            {
                Msisdn = "1003",
                Imsi = "1003",
                IccId = "1003",
                UserName = "sim03",
                Password = "password",
                SimGroup = _simGroup2
            });
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
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("The SimId field is required.", json["errors"]?["SimId"].First.ToString());
            Assert.Equal("The DeviceId field is required.", json["errors"]?["DeviceId"].First.ToString());
            Assert.Equal("The IsolatedNw2Ip field is required.", json["errors"]?["IsolatedNw2Ip"].First.ToString());
            Assert.Equal("The StartDate field is required.", json["errors"]?["StartDate"].First.ToString());
            Assert.Equal("The AuthenticationDuration field is required.", json["errors"]?["AuthenticationDuration"].First.ToString());
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case02A()
        {
            var obj = new
            {
                SimId = "acbcdsfsf",
                DeviceId = "1346",
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = "abdsakjf",
                StartDate = "2020-02-98",
                EndDate = "2020-02-98"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["AuthenticationDuration"]);
            Assert.NotNull(json["errors"]?["SimId"]);
            Assert.NotNull(json["errors"]?["DeviceId"]);
            Assert.NotNull(json["errors"]?["StartDate"].First.ToString());
            Assert.NotNull(json["errors"]?["EndDate"].First.ToString());
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case02B()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
                EndDate = "2020-02-09"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("The field is not a valid CIDR.", json["errors"]?["IsolatedNw2Ip"].First.ToString());
        }

        /// <summary>
        /// 利用終了日:'利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case03()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-21",
                EndDate = "2020-02-09"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.InvalidEndDate, json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// DBにSim不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var obj = new
            {
                SimId = Guid.NewGuid(),
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
                EndDate = "2020-02-09"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Sim)]);
        }

        /// <summary>
        /// DBにDevice不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = Guid.NewGuid(),
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
                EndDate = "2020-02-09"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Device)]);
        }

        /// <summary>
        /// SIM:他組織
        /// </summary>
        [Fact]
        public void Case06()
        {
            var obj = new
            {
                SimId = _sim3.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
                EndDate = "2020-02-09"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// Device:他組織
        /// </summary>
        [Fact]
        public void Case07()
        {
            var obj = new
            {
                SimId = _sim3.Id,
                DeviceId = _device2.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
                EndDate = "2020-02-09"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        ///'同組合せが他レコードに重複して存在
        /// </summary>
        [Fact]
        public void Case08()
        {
            _context.Add(new SimAndDevice()
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse("2020-02-09")
            });
            _context.SaveChanges();
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
                EndDate = "2020-02-09"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimAndDevice)]);
        }

        /// <summary>
        /// 全部正常
        /// </summary>
        [Fact]
        public void Case09()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
                EndDate = "2020-02-09"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.NotNull(json["id"]);
            Assert.Equal(obj.SimId, json["sim"]["id"]);
            Assert.Equal(obj.DeviceId, json["device"]["id"]);
            Assert.Equal(obj.IsolatedNw2Ip, json["isolatedNw2Ip"]);
            Assert.Equal("2020-02-01", json["startDate"]);
            Assert.Equal("2020-02-09", json["endDate"]);
        }

        /// <summary>
        /// 全部正常 利用終了日:不在
        /// </summary>
        [Fact]
        public void Case10()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.NotNull(json["id"]);
            Assert.Equal(obj.SimId, json["sim"]["id"]);
            Assert.Equal(obj.DeviceId, json["device"]["id"]);
            Assert.Equal(obj.IsolatedNw2Ip, json["isolatedNw2Ip"]);
            Assert.Equal("2020-02-01", json["startDate"]);
            Assert.Empty(json["endDate"]);
        }

        /// <summary>
        /// 利用開始日 '端末の利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case11()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-01-01",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用開始日 '端末の利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case12()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2021-03-01",
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用終了日 '端末の利用開始日 (2020-01-30) 未満の日付
        /// </summary>
        [Fact]
        public void Case13()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-01-28",
                EndDate = "2020-01-29"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日 '端末の利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case14()
        {
            var obj = new
            {
                SimId = _sim1.Id,
                DeviceId = _device1.Id,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = "2020-02-01",
                EndDate = "2021-03-01"
            };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
        }

    }
}
