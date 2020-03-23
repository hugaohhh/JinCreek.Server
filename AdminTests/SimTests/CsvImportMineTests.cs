using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JinCreek.Server.AdminTests.SimTests
{
    /// <summary>
    /// 自分simインポート
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class CsvImportMineTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/sims/mine/csv";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup1a;
        private readonly SimGroup _simGroup2;
        private readonly Device _device1;

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
            _context.Add(new SuperAdmin { Name = "", AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { AdObjectId = Guid.NewGuid(), Name = "", AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_simGroup1 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup1",
                Organization = _org1,
                Apn = "apn",
                AuthenticationServerIp = "AuthenticationServerIp",
                IsolatedNw1IpPool = "IsolatedNw1IpPool1",
                IsolatedNw1SecondaryDns = "IsolatedNw1SecondaryDns",
                IsolatedNw1IpRange = "IsolatedNw1IpRange",
                IsolatedNw1PrimaryDns = "IsolatedNw1PrimaryDns",
                NasIp = "NasIp",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns",
                UserNameSuffix = "UserNameSuffix"
            });
            _context.Add(_simGroup1a = new SimGroup
            {
                Id = Guid.NewGuid(),
                Name = "simGroup1a",
                Organization = _org1,
                Apn = "apn",
                AuthenticationServerIp = "AuthenticationServerIp",
                IsolatedNw1IpPool = "IsolatedNw1IpPool1a",
                IsolatedNw1SecondaryDns = "IsolatedNw1SecondaryDns",
                IsolatedNw1IpRange = "IsolatedNw1IpRange",
                IsolatedNw1PrimaryDns = "IsolatedNw1PrimaryDns",
                NasIp = "NasIp",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns",
                UserNameSuffix = "UserNameSuffix"
            });
            _context.Add(_simGroup2 = new SimGroup
            {
                Id = Guid.NewGuid(),
                Name = "simGroup2",
                Organization = _org2,
                Apn = "apn",
                AuthenticationServerIp = "AuthenticationServerIp",
                IsolatedNw1IpPool = "IsolatedNw1IpPool2",
                IsolatedNw1SecondaryDns = "IsolatedNw1SecondaryDns",
                IsolatedNw1IpRange = "IsolatedNw1IpRange",
                IsolatedNw1PrimaryDns = "IsolatedNw1PrimaryDns",
                NasIp = "NasIp",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns",
                UserNameSuffix = "UserNameSuffix",

            });
            _context.Add(_device1 = new Device()
            {
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                ProductName = "",
                SerialNumber = "",
                WindowsSignInListCacheDays = 1,
            });
            _context.SaveChanges();
        }

        /// <summary>
        ///削除フラグ：D ; ID:不在 端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case01()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"D,,{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");//削除フラグ：D ; ID:不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Id"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// 端末ID：UUIDではない 削除フラグ:'上記条件に合致せず存在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case02()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($",aaaaaaaaa,{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");//端末ID：UUIDではない

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["ID"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// MSISDM：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case03()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},,{sim1.Imsi},{sim1.IccId},sim001,123456");//MSISDM：不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Msisdn"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// MSISDM：'指定値以外(数字以外)　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case04()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},abcdes123,{sim1.Imsi},{sim1.IccId},sim001,123456");// MSISDM：'指定値以外(数字以外)

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Msisdn"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// IMSI：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case05()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},,{sim1.IccId},sim001,123456");//IMSI：不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Imsi"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// IMSI：'指定値以外(数字以外)　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case06()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},acbderw123,{sim1.IccId},sim001,123456");//IMSI：'指定値以外(数字以外)

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Imsi"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// ICC ID：不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case07()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},,sim001,123456");//ICC ID：不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["IccId"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// ICC ID：'指定値以外(19文字以上)　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case08()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},111111111111111111111111111111111111111111111,sim001,123456");//ICC ID：'指定値以外(19文字以上)

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["IccId"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// ユーザー名:不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case09()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},,123456");//ユーザー名:不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["UserName"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// ユーザー名:'指定値以外(ASCIIチェック)　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case10()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},ああああ,123456");//ユーザー名:'指定値以外(ASCIIチェック)

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["UserName"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// パスワード:不在　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case11()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim002,");//パスワード:不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Password"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// パスワード:'指定値以外(ASCIIチェック)　端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case12()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim002,長門有希");//パスワード:'指定値以外(ASCIIチェック)

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Password"]);
            Assert.Equal("sim01", sim1.UserName);
        }

        /// <summary>
        /// DBにSIM：不在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case13()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{Guid.NewGuid()},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim002,123456");// DBにSIM：不在

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Sim)]);
        }

        /// <summary>
        /// DBにSIM：他組織　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case14()
        {
            var sim1 = new Sim()　//自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            var sim2 = new Sim() // 他組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup2
            };
            _context.AddRange(sim1, sim2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim2.Id},{_simGroup2.Id},{_simGroup2.Name},{sim2.Msisdn},{sim2.Imsi},{sim2.IccId},sim002,123456");//DBにSIM：他組織

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// DBにSIM：MSISDM:'他レコードと重複して存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case15()
        {
            var sim1 = new Sim()　//自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            var sim2 = new Sim() // 自組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.AddRange(sim1, sim2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim2.Id},{_simGroup1.Id},{_simGroup1.Name},1001,{sim2.Imsi},{sim2.IccId},sim002,123456");// MSISDM:'他レコードと重複して存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["Msisdn"]);
        }

        /// <summary>
        /// DBにSIM：IMSI:'他レコードと重複して存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case16()
        {
            var sim1 = new Sim()　//自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            var sim2 = new Sim() // 自組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.AddRange(sim1, sim2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim2.Id},{_simGroup1.Id},{_simGroup1.Name},1003,1001,{sim2.IccId},sim002,123456"); //IMSI:'他レコードと重複して存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["Imsi"]);
        }

        /// <summary>
        /// DBにSIM：ICC ID:'他レコードと重複して存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case17()
        {
            var sim1 = new Sim()　//自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            var sim2 = new Sim() // 自組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.AddRange(sim1, sim2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim2.Id},{_simGroup1.Id},{_simGroup1.Name},1003,1003,1001,sim002,123456");// ICC ID:他レコードと重複して存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["IccId"]);
        }

        /// <summary>
        /// DBにSIM：ユーザー名+ユーザー名サフィックス:'他レコードと重複して存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case18()
        {
            var sim1 = new Sim()　//自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            var sim2 = new Sim() // 自組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.AddRange(sim1, sim2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"a,{sim2.Id},{_simGroup1.Id},{_simGroup1.Name},{sim2.Msisdn},{sim2.Imsi},{sim2.IccId},sim001,123456");// ユーザー名+ユーザー名サフィックス:他レコードと重複して存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["UserName"]);
        }

        /// <summary>
        /// DBにsim存在そしてSimDeviceも存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case19()
        {
            var sim1 = new Sim()　//自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            var sim2 = new Sim() // 自組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            };
            var simAndDevice = new SimAndDevice()
            {
                Sim = sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1/18",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse("2020-02-09")
            };
            _context.AddRange(sim1, sim2, simAndDevice);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"D,{sim2.Id},{_simGroup1.Id},{_simGroup1.Name},{sim2.Msisdn},{sim2.Imsi},{sim2.IccId},sim003,123456");//SimDeviceも存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"][nameof(SimAndDevice)]);
        }

        /// <summary>
        /// DBにsim存在そして SIM＆端末認証失敗 も存在　；端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case20()
        {
            var sim1 = new Sim()　//自組織
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            var sim2 = new Sim() // 自組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(new SimAndDeviceAuthenticationFailureLog()
            {
                Sim = sim2,
                Time = DateTime.Now
            });
            _context.AddRange(sim1, sim2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},{sim1.Imsi},{sim1.IccId},sim001,123456");
            obj.AppendLine($"D,{sim2.Id},{_simGroup1.Id},{_simGroup1.Name},{sim2.Msisdn},{sim2.Imsi},{sim2.IccId},sim003,123456");//SIM＆端末認証失敗 も存在

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"][nameof(SimAndDeviceAuthenticationFailureLog)]);
        }

        /// <summary>
        /// 端末レコード：'存在（自組織）
        /// </summary>
        [Fact]
        public void Case21()
        {
            var sim1 = new Sim()
            {
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            };
            var sim2 = new Sim() // 自組織
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.AddRange(sim1, sim2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine("is Delete,ID,SIM Group ID,SIM Group Name,MSISDN,IMSI,ICC ID,User Name,Password");
            obj.AppendLine($"a,{sim1.Id},{_simGroup1.Id},{_simGroup1.Name},{sim1.Msisdn},1101,1101,sim001,123456");　// D以外更新
            obj.AppendLine($"D,{sim2.Id},{_simGroup1.Id},{_simGroup1.Name},{sim2.Msisdn},1102,1102,sim002,123456");　// D 削除
            obj.AppendLine($"a,,{_simGroup1.Id},{_simGroup1.Name},1103,1103,1103,sim003,123456");　// D ではない　そしてIDない 登録

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JArray.Parse(body);

            Assert.Equal(sim1.Id, list[0]["id"]);
            Assert.Equal("sim001", list[0]["userName"].ToString());
            Assert.Equal("123456", list[0]["password"]);
            Assert.Equal("1101", list[0]["iccId"]);
            Assert.Equal("1101", list[0]["imsi"]);
            Assert.Equal(sim1.Msisdn, list[0]["msisdn"]);
            Assert.Equal(sim1.SimGroup.Id, list[0]["simGroup"]?["id"]);
            Assert.Equal(sim1.SimGroup.Name, list[0]["simGroup"]?["name"].ToString());
            Assert.Equal(sim1.SimGroup.Apn, list[0]["simGroup"]?["apn"].ToString());
            Assert.Equal(sim1.SimGroup.AuthenticationServerIp, list[0]["simGroup"]?["authenticationServerIp"].ToString());
            Assert.Equal(sim1.SimGroup.IsolatedNw1IpPool, list[0]["simGroup"]?["isolatedNw1IpPool"].ToString());
            Assert.Equal(sim1.SimGroup.IsolatedNw1IpRange, list[0]["simGroup"]?["isolatedNw1IpRange"].ToString());
            Assert.Equal(sim1.SimGroup.PrimaryDns, list[0]["simGroup"]?["primaryDns"].ToString());
            Assert.Equal(sim1.SimGroup.SecondaryDns, list[0]["simGroup"]?["secondaryDns"].ToString());
            Assert.Equal(sim1.SimGroup.IsolatedNw1PrimaryDns, list[0]["simGroup"]?["isolatedNw1PrimaryDns"].ToString());
            Assert.Equal(sim1.SimGroup.IsolatedNw1SecondaryDns, list[0]["simGroup"]?["isolatedNw1SecondaryDns"].ToString());
            Assert.Equal(sim1.SimGroup.Organization.Code, list[0]["simGroup"]?["organization"]?["code"]);
            Assert.Equal(sim1.SimGroup.Organization.Name, list[0]["simGroup"]?["organization"]?["name"].ToString());
            //削除データチェック
            Assert.Empty(_context.Sim.Where(s => s.Id == Guid.Parse(list[1]["id"].ToString())));

            //登録データチェック
            Assert.NotNull(list[2]["id"]);
        }
    }
}
