using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using JinCreek.Server.Admin;
using Xunit;

namespace JinCreek.Server.AdminTests.SimAndDevicesTests
{
    /// <summary>
    /// 自分sim&端末インポート
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public class CsvImportMineTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/sim-and-devices/mine/csv";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly UserGroupEndUser _userGroupEndUser;
        private readonly AvailablePeriod _availablePeriod;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Device _device1;
        private readonly Device _device2;
        private readonly Sim _sim1;
        private readonly Sim _sim2;
        private readonly Sim _sim3;

        public CsvImportMineTests(CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            //_context.Database.EnsureDeleted();
            //_context.Database.EnsureCreated();
            //_context.Database.Migrate();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain03", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { AdObjectId = Guid.NewGuid(), Id = Guid.NewGuid(), Name = "userGroup1", Domain = _domain1 });
            _context.Add(_userGroup2 = new UserGroup { AdObjectId = Guid.NewGuid(), Id = Guid.NewGuid(), Name = "userGroup2", Domain = _domain2 });
            _context.Add(_user1 = new SuperAdmin { Name = "", AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { Name = "", AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_userGroupEndUser = new UserGroupEndUser() { EndUser = _user2, UserGroup = _userGroup1 });
            _context.Add(_availablePeriod = new AvailablePeriod()
            { EndUser = _user2, EndDate = DateTime.Now.AddHours(6.00), StartDate = DateTime.Now.AddHours(-6.00) });

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
                UserNameSuffix = "UserNameSuffix"
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
                UserNameSuffix = "UserNameSuffix"
            });
            _context.Add(_sim1 = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_sim2 = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn02",
                Imsi = "imsi02",
                IccId = "iccid02",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_sim3 = new Sim() // 組織 : '他組織
            {
                Msisdn = "msisdn03",
                Imsi = "imsi03",
                IccId = "iccid03",
                UserName = "sim03",
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
                ProductName = "",
                SerialNumber = "",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-01-30"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.Add(_device2 = new Device()
            {
                LteModule = null,
                Domain = _domain1,
                Name = "device02",
                UseTpm = true,
                ManagedNumber = "001",
                ProductName = "",
                SerialNumber = "",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-01-30"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.SaveChanges();
        }

        /// <summary>
        ///  SIM ID:不在 端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case01()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",,{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//SIM ID:不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["SimId"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// SIm ID：UUIDではない 端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case02()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",aaaaaaa,{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//SIm ID：UUIDではない

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["SIM ID"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 端末ID：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case03()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},,{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");// 端末ID：不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["DeviceId"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 端末ID：UUIDではない　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case04()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},11111111111,{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//端末ID：UUIDではない

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Device ID"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 検疫NW2 IPアドレス：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case05()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},,1,2020-02-01,2020-03-01");//検疫NW2 IPアドレス：不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["IsolatedNw2Ip"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 検疫NW2 IPアドレス：'指定値以外(CIDR以外)　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case06()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1,1,2020-02-01,2020-03-01");//検疫NW2 IPアドレス：'指定値以外(CIDR以外)

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("(Row:3) The field is not a valid CIDR.", json["errors"]?["IsolatedNw2Ip"].First().ToString());
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 利用開始日：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case07()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,,2020-03-01");//利用開始日：不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["StartDate"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 利用開始日：'存在しない日付　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case08()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-98,2020-03-01");//利用開始日：'存在しない日付

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Start Date"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 利用終了日:'存在しない日付　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case09()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-98");//利用終了日:'存在しない日付

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["End Date"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 利用終了日:'利用開始日未満の日付　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case10()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-17,2020-02-12");//利用終了日:'利用開始日未満の日付

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Contains(Messages.InvalidEndDate, (string)json["errors"]["EndDate"][0]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 認証期限:不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case11()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,,2020-02-01,2020-03-01");//認証期限:不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["AuthenticationDuration"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// 認証期限:''0以上の整数以外　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case12()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,aaa,2020-02-01,2020-03-01");//認証期限:''0以上の整数以外

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Authentication Duration"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// SIM&端末組合せ:他組織　　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case13()
        {
            var simDevice = new SimAndDevice() // 組織 : '他組織
            {
                Sim = _sim3,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//SIM&端末組合せ:他組織

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// SIM&端末組合せ:不在(SimID)　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case14()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{Guid.NewGuid()},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//SIM&端末組合せ:不在(SimID)

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Sim)]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// SIM&端末組合せ:不在(DeviceID)　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case15()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{Guid.NewGuid()},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");// SIM&端末組合せ:不在(DeviceID)

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Device)]);
            Assert.Equal("127.0.0.1/18", simDevice2.IsolatedNw2Ip);
        }

        /// <summary>
        /// DBにsimDevice存在そして '多要素組み合わせ も存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case16()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.Add(new MultiFactor()
            {
                SimAndDevice = simDevice,
                ClosedNwIp = "127.0.0.1",
                EndUser = _user2,
                StartDate = DateTime.Now,
            });
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($"D,{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//多要素組み合わせ も存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["FactorCombination"]);
        }

        /// <summary>
        /// DBにsimDevice存在そして '多要素認証失敗 も存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case17()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.Add(new MultiFactorAuthenticationFailureLog()
            {
                SimAndDevice = simDevice,
                Time = DateTime.Now,
                Sim = _sim1
            });
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($"D,{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//多要素認証失敗 も存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["MultiFactorAuthenticationFailure"]);
        }

        /// <summary>
        /// DBにsimDevice存在そして 'SIM&端末認証成功 も存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case18()
        {
            var simDevice = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.Add(new SimAndDeviceAuthenticationSuccessLog()
            {
                SimAndDevice = simDevice,
                Time = DateTime.Now,
                Sim = _sim1
            });
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($"D,{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//SIM&端末認証成功 も存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["SimAndDeviceAuthenticationSuccess"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case19()
        {
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice3 = new SimAndDevice() // 組織 : '自組織 D
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice2, simDevice3);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($"D,{simDevice3.SimId},{simDevice3.Sim.Msisdn},{simDevice3.Device.Domain.Name},{simDevice3.DeviceId},{simDevice3.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");
            obj.AppendLine($",{_sim1.Id},{_sim1.Msisdn},{_device2.Domain.Name},{_device2.Id},{_device2.Name},127.0.0.1/20,0,2020-02-01,2020-03-01");

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JArray.Parse(body);

            //更新データチェック

            Assert.Equal(simDevice2.Id, list[0]["id"]);
            Assert.Equal(simDevice2.SimId, list[0]["simId"]);
            Assert.Equal(simDevice2.DeviceId, list[0]["deviceId"]);
            Assert.Equal("127.0.0.1/20", list[0]["isolatedNw2Ip"].ToString());
            Assert.Equal("2020-02-01", (string)list[0]["startDate"]);
            Assert.Equal("2020-03-01", (string)list[0]["endDate"]);
            Assert.Equal("1", list[0]["authenticationDuration"]);

            //削除データチェック
            Assert.Empty(_context.SimAndDevice.Where(s => s.Id == Guid.Parse(list[1]["id"].ToString())));

            //登録データチェック
            Assert.NotNull(list[2]["id"]);
        }

        /// <summary>
        /// 利用開始日 '端末の利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case20()
        {
            var simDevice = new SimAndDevice() // 組織 : 自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//更新データ
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-01-01,2020-03-01");//更新データ

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用開始日 '端末の利用終了日 (2021-02-01) 以降の日付
        /// </summary>
        [Fact]
        public void Case21()
        {
            var simDevice = new SimAndDevice() // 組織 : 自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//更新データ
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2021-02-02,2021-02-03");//更新データ

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["StartDate"][0]);
        }

        /// <summary>
        /// 利用終了日 '端末の利用開始日 (2020-01-30) 未満の日付
        /// </summary>
        [Fact]
        public void Case22()
        {
            var simDevice = new SimAndDevice() // 組織 : 自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//更新データ
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-01-28,2020-01-29");//更新データ

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
        }

        /// <summary>
        /// 利用終了日 '端末の利用終了日以降の日付
        /// </summary>
        [Fact]
        public void Case23()
        {
            var simDevice = new SimAndDevice() // 組織 : 自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            _context.AddRange(simDevice, simDevice2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,SIM ID,MSISDN,Domain,Device ID,Device Name,Isolated NW2 IP,Authentication Duration,Start Date,End Date");
            obj.AppendLine($",{simDevice2.SimId},{simDevice2.Sim.Msisdn},{simDevice2.Device.Domain.Name},{simDevice2.DeviceId},{simDevice2.Device.Name},127.0.0.1/20,1,2020-02-01,2020-03-01");//更新データ
            obj.AppendLine($",{simDevice.SimId},{simDevice.Sim.Msisdn},{simDevice.Device.Domain.Name},{simDevice.DeviceId},{simDevice.Device.Name},127.0.0.1/20,1,2020-02-01,2021-03-01");//更新データ

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
        }
    }
}
