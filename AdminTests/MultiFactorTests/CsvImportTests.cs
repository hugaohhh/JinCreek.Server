using JinCreek.Server.Admin;
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
using Xunit;

namespace JinCreek.Server.AdminTests.MultiFactorTests
{
    /// <summary>
    /// 認証要素組合せインポート
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class CsvImportTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/multi-factors/csv";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly UserGroupEndUser _userGroupEndUser;
        private readonly SimGroup _simGroup1;
        private readonly Device _device1;
        private readonly Device _device2;
        private readonly Sim _sim1;
        private readonly Sim _sim2;

        private readonly User _user0;
        private readonly User _user1;
        private readonly GeneralUser _user3;
        private readonly GeneralUser _user2;

        private readonly SimAndDevice _simDevice1;
        private readonly SimAndDevice _simDevice2;
        private readonly SimAndDevice _simDevice3;

        public CsvImportTests(CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain03", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { AdObjectId = Guid.NewGuid(), Id = Guid.NewGuid(), Name = "userGroup1", Domain = _domain1 });
            _context.Add(_userGroup2 = new UserGroup { AdObjectId = Guid.NewGuid(), Id = Guid.NewGuid(), Name = "userGroup2", Domain = _domain2 });
            _context.Add(_user0 = new SuperAdmin {Name = "",AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user1 = new UserAdmin() { Name = "", AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_user2 = new GeneralUser() { Name = "", AccountName = "user2", DomainId = _domain1.Id });
            _context.Add(new AvailablePeriod()
                { EndUser = _user2, EndDate = DateTime.Parse("2021-03-01"), StartDate = DateTime.Parse("2020-02-10") });
            _context.Add(_user3 = new GeneralUser()
            {
                Name = "",
                AccountName = "user3",
                Domain = _domain2
            });
            _context.Add(_userGroupEndUser = new UserGroupEndUser() { EndUser = _user2, UserGroup = _userGroup1 });

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
            });
            _context.Add(_simDevice1 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.Add(_simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device2,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.Add(_simDevice3 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2021-02-01")
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// ロール : 不在 削除フラグ:'上記条件に合致せず存在
        /// </summary>
        [Fact]
        public void Case01()
        {
            var multiFactor = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            _context.Add(multiFactor);
            _context.SaveChanges();
            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor.SimAndDeviceId},{multiFactor.SimAndDevice.Sim.Msisdn},{multiFactor.EndUser.Domain.Name},{multiFactor.SimAndDevice.Device.Name},{multiFactor.EndUserId},{multiFactor.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("", body);
        }

        /// <summary>
        ///  SIM&端末組合せ ID:不在 端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case02()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",,{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["SimAndDeviceId"]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// SIM&端末組合せ ID：UUIDではない 端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case03()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",aaaaaaaaaaaaaaa,{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Sim And Device ID"]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// ユーザーID：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case04()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},,{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["UserId"]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// ユーザーID：UUIDではない　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case05()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},aaaaaaaaaaaaa,{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["User ID"]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 閉域NW IPアドレス：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case06()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},,2020-02-17,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("(Row:3) The ClosedNwIp field is required.", json["errors"]?["ClosedNwIp"].First().ToString());
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 閉域NW IPアドレス：'指定値以外(IP以外)　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case07()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},115664654,2020-02-17,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("(Row:3) The field is not a valid IP address.", json["errors"]?["ClosedNwIp"].First().ToString());
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用開始日：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case08()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["StartDate"]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用開始日：'存在しない日付　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case09()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-98,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Start Date"]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用終了日:'存在しない日付　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case10()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-91");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["End Date"]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用終了日:'利用開始日未満の日付　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case11()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-27,2020-02-26");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Contains(Messages.InvalidEndDate, (string)json["errors"]["EndDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 認証要素組合せ:不在(SIM&端末組合せID)　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case12()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{Guid.NewGuid()},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimAndDevice)]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 認証要素組合せ:不在(ユーザーID)　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case13()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{Guid.NewGuid()},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(EndUser)]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 削除フラグ:D; DBに認証要素組合せ存在そして 認証解除 も存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case14()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.Add(new DeauthenticationLog()
            {
                MultiFactor = multiFactor2,
                Time = DateTime.Now
            });
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($"D,{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($"D,{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"][nameof(DeauthenticationLog)]);
        }

        /// <summary>
        /// 削除フラグ:D; DBに認証要素組合せ存在そして 多要素認証成功 も存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case15()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.Add(new MultiFactorAuthenticationSuccessLog()
            {
                MultiFactor = multiFactor2,
                Time = DateTime.Now
            });
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($"D,{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($"D,{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"][nameof(MultiFactorAuthenticationSuccessLog)]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case16()
        {
            var multiFactor1 = new MultiFactor() // user他組織
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user3
            };
            var multiFactor2 = new MultiFactor() //自組織 利用終了日：不在
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor3 = new MultiFactor() //自組織 削除
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2, multiFactor3);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor2.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($"D,{multiFactor3.SimAndDeviceId},{multiFactor3.SimAndDevice.Sim.Msisdn},{multiFactor3.EndUser.Domain.Name},{multiFactor3.SimAndDevice.Device.Name},{multiFactor3.EndUserId},{multiFactor3.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{_simDevice3.Id},{_simDevice3.Sim.Msisdn},{_user2.Domain.Name},{_simDevice3.Device.Name},{_user2.Id},{_user2.AccountName},127.0.0.125,2020-02-17,2020-03-01");

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JArray.Parse(body);

            //更新データチェック
            Assert.Equal(multiFactor1.Id, list[0]["id"]);
            Assert.Equal(multiFactor1.SimAndDeviceId, list[0]["simAndDeviceId"]);
            Assert.Equal(multiFactor1.EndUserId, list[0]["endUserId"]);
            Assert.Equal("127.0.0.125", list[0]["closedNwIp"].ToString());
            Assert.Equal("2020-02-17", (string)list[0]["startDate"]);
            Assert.Equal("2020-03-01", (string)list[0]["endDate"]);

            Assert.Equal(multiFactor2.Id, list[1]["id"]);
            Assert.Equal(multiFactor2.SimAndDeviceId, list[1]["simAndDeviceId"]);
            Assert.Equal(multiFactor2.EndUserId, list[1]["endUserId"]);
            Assert.Equal("127.0.0.125", list[1]["closedNwIp"].ToString());
            Assert.Equal("2020-02-17", (string)list[1]["startDate"]);
            Assert.Equal("2020-03-01", (string)list[1]["endDate"]);

            //削除データチェック
            Assert.Empty(_context.MultiFactor.Where(s => s.Id == Guid.Parse(list[2]["id"].ToString())));

            //登録データチェック
            Assert.NotNull(list[3]["id"]);
        }

        /// <summary>
        /// 利用開始日 'SIM&端末組合せ利用開始日未満の日付　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case17()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-01-01,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用開始日 'SIM&端末組合せ利用終了日 (2021-02-01) 以降の日付　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case18()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2021-02-02,2021-02-02");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["StartDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用開始日 'ユーザー利用開始日未満の日付　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case19()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-07,2020-03-01");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["StartDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用開始日 'ユーザー利用終了日 (2021-03-01) 以降の日付　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case20()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2021-03-02,2021-03-02");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["StartDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用終了日:'SIM&端末組合せ利用開始日 (2020-02-01) 未満の日付　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case21()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-01-31,2020-01-31");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用終了日:'SIM&端末組合せ利用終了日以降の日付　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case22()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2021-02-25");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用終了日:''ユーザー利用開始日 (2020-02-10) 未満の日付　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case23()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-09,2020-02-09");

            var (response, body, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, (string)json["errors"]["EndDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

        /// <summary>
        /// 利用終了日:'ユーザー利用終了日以降の日付　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case24()
        {
            var multiFactor1 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Now,
                SimAndDevice = _simDevice1,
                EndUser = _user2
            };
            var multiFactor2 = new MultiFactor()
            {
                ClosedNwIp = "127.0.0.1",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = DateTime.Now,
                SimAndDevice = _simDevice2,
                EndUser = _user2
            };
            _context.AddRange(multiFactor1, multiFactor2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,Sim And Device ID,MSISDN,Domain,Device,User ID,User Name,Closed NW IP,Start Date,End Date");
            obj.AppendLine($",{multiFactor1.SimAndDeviceId},{multiFactor1.SimAndDevice.Sim.Msisdn},{multiFactor1.EndUser.Domain.Name},{multiFactor1.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor1.EndUser.AccountName},127.0.0.125,2020-02-17,2020-03-01");
            obj.AppendLine($",{multiFactor2.SimAndDeviceId},{multiFactor2.SimAndDevice.Sim.Msisdn},{multiFactor2.EndUser.Domain.Name},{multiFactor2.SimAndDevice.Device.Name},{multiFactor1.EndUserId},{multiFactor2.EndUser.AccountName},127.0.0.125,2020-02-17,2021-03-25");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(Messages.OutOfDate, json["errors"]["EndDate"][0]);
            Assert.Equal("127.0.0.1", multiFactor2.ClosedNwIp);
        }

    }
}
