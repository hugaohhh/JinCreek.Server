using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimTests
{
    /// <summary>
    /// Sim更新
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public class UpdateTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<Admin.Startup>>
    {
        private readonly HttpClient _client;
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly MainDbContext _context;

        private const string Url = "/api/sims";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;

        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Sim _sim1;

        public UpdateTests(CustomWebApplicationFactoryWithMariaDb<Admin.Startup> factory)
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

            _context.Add(new SuperAdmin { AccountName = "user0", Name = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { AccountName = "user1", Name = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
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
            _context.Add(_sim1 = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new { };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// ID不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new { };
            var result = Utils.Put(_client, $"{Url}/", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.MethodNotAllowed, result.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// IDがUUIDでない
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "user01",
                Password = "password"
            };
            var result = Utils.Put(_client, $"{Url}/a", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// 入力が不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new { };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Equal("The Msisdn field is required.", json["errors"]?["Msisdn"].First.ToString());
            Assert.Equal("The Imsi field is required.", json["errors"]?["Imsi"].First.ToString());
            Assert.Equal("The IccId field is required.", json["errors"]?["IccId"].First.ToString());
            //Assert.Equal("The UserName field is required.", json["errors"]?["UserName"].First.ToString());
            //Assert.Equal("The Password field is required.", json["errors"]?["Password"].First.ToString());
        }

        /// <summary>
        /// 入力値が全部不正パターン
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "abcd", //  数字以外
                Imsi = "abcdes",　// 数字以外
                IccId = "13456666666666666666666666666666655555555",　//19文字以上
                UserName = "1346abdfs!@#ああ", // ASCII以外
                Password = "テスト145487/*-+", // ASCII以外
            };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Equal("msisdn_is_only_number", json["errors"]?["Msisdn"].First.ToString());
            Assert.Equal("imsi_is_only_number", json["errors"]?["Imsi"].First.ToString());
            Assert.Equal("The field IccId must be a string with a maximum length of 19.", json["errors"]?["IccId"].First.ToString());
            Assert.Equal("UserName_is_only_ASCII", json["errors"]?["UserName"].First.ToString());
            Assert.Equal("Password_is_only_ASCII", json["errors"]?["Password"].First.ToString());
        }

        /// <summary>
        /// DBにsimが不在
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "user01",
                Password = "password"
            };
            var result = Utils.Put(_client, $"{Url}/{Guid.NewGuid()}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"][nameof(Sim)]);
        }

        /// <summary>
        /// MSISDM:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case07()
        {
            var sim = new Sim()
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "user02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1002", // MSISDM:'他レコードと重複して存在
                Imsi = "1003",
                IccId = "1003",
                UserName = "user03",
                Password = "password"
            };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["Msisdn"]); // 存在
            Assert.Null(json["errors"]?["Imsi"]);
            Assert.Null(json["errors"]?["IccId"]);
        }

        /// <summary>
        /// IMSI:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case08()
        {
            var sim = new Sim()
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "user02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1003",
                Imsi = "1002", //IMSI:'他レコードと重複して存在
                IccId = "1003",
                UserName = "user03",
                Password = "password"
            };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Null(json["errors"]?["Msisdn"]);
            Assert.NotNull(json["errors"]?["Imsi"]); // 存在
            Assert.Null(json["errors"]?["IccId"]);
        }

        /// <summary>
        /// ICC ID:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case09()
        {
            var sim = new Sim()
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "user02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1003",
                Imsi = "1003",
                IccId = "1002", //ICC ID:'他レコードと重複して存在
                UserName = "user03",
                Password = "password"
            };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Null(json["errors"]?["Msisdn"]);
            Assert.Null(json["errors"]?["Imsi"]);
            Assert.NotNull(json["errors"]?["IccId"]); // 存在
        }

        /// <summary>
        /// ユーザー名+ユーザー名サフィックス:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case10()
        {
            var sim = new Sim()
            {
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "user02",
                Password = "password",
                SimGroup = _simGroup1
            };
            _context.Add(sim);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1003",
                Imsi = "1003",
                IccId = "1003",
                UserName = "user02", //ユーザー名+ユーザー名サフィックス:'他レコードと重複して存在
                Password = "password"
            };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["UserName"]);
        }

        /// <summary>
        /// 全部正常
        /// </summary>
        [Fact]
        public void Case11()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "user01",
                Password = "password"
            };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(_sim1.Id, json["id"]);
            Assert.Equal(obj.UserName, json["userName"].ToString());
            Assert.Equal(obj.Password, json["password"]);
            Assert.Equal(obj.IccId, json["iccId"]);
            Assert.Equal(obj.Imsi, json["imsi"]);
            Assert.Equal(obj.Msisdn, json["msisdn"]);
            Assert.Equal(_sim1.SimGroup.Id, json["simGroup"]?["id"]);
            Assert.Equal(_sim1.SimGroup.Name, json["simGroup"]?["name"].ToString());
            Assert.Equal(_sim1.SimGroup.Apn, json["simGroup"]?["apn"].ToString());
            Assert.Equal(_sim1.SimGroup.NasIp, json["simGroup"]?["nasIp"].ToString());
            Assert.Equal(_sim1.SimGroup.AuthenticationServerIp, json["simGroup"]?["authenticationServerIp"].ToString());
            Assert.Equal(_sim1.SimGroup.IsolatedNw1IpPool, json["simGroup"]?["isolatedNw1IpPool"].ToString());
            Assert.Equal(_sim1.SimGroup.IsolatedNw1IpRange, json["simGroup"]?["isolatedNw1IpRange"].ToString());
            Assert.Equal(_sim1.SimGroup.PrimaryDns, json["simGroup"]?["primaryDns"].ToString());
            Assert.Equal(_sim1.SimGroup.SecondaryDns, json["simGroup"]?["secondaryDns"].ToString());
            Assert.Equal(_sim1.SimGroup.IsolatedNw1PrimaryDns, json["simGroup"]?["isolatedNw1PrimaryDns"].ToString());
            Assert.Equal(_sim1.SimGroup.IsolatedNw1SecondaryDns, json["simGroup"]?["isolatedNw1SecondaryDns"].ToString());
            Assert.Equal(_sim1.SimGroup.Organization.Code, json["simGroup"]?["organization"]?["code"]);
            Assert.Equal(_sim1.SimGroup.Organization.Name, json["simGroup"]?["organization"]?["name"].ToString());
        }

        /// <summary>
        /// 全部正常 userName Password がNull
        /// </summary>
        [Fact]
        public void Case12()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
            };
            var result = Utils.Put(_client, $"{Url}/{_sim1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(_sim1.Id, json["id"]);
            Assert.NotNull(json["userName"].ToString());
            Assert.NotNull(json["password"]);
            Assert.Equal(obj.IccId, json["iccId"]);
            Assert.Equal(obj.Imsi, json["imsi"]);
            Assert.Equal(obj.Msisdn, json["msisdn"]);
            Assert.Equal(obj.SimGroupId, json["simGroup"]?["id"]);
            Assert.Equal(_sim1.SimGroup.Name, json["simGroup"]?["name"].ToString());
            Assert.Equal(_sim1.SimGroup.Apn, json["simGroup"]?["apn"].ToString());
            Assert.Equal(_sim1.SimGroup.NasIp, json["simGroup"]?["nasIp"].ToString());
            Assert.Equal(_sim1.SimGroup.AuthenticationServerIp, json["simGroup"]?["authenticationServerIp"].ToString());
            Assert.Equal(_sim1.SimGroup.IsolatedNw1IpPool, json["simGroup"]?["isolatedNw1IpPool"].ToString());
            Assert.Equal(_sim1.SimGroup.IsolatedNw1IpRange, json["simGroup"]?["isolatedNw1IpRange"].ToString());
            Assert.Equal(_sim1.SimGroup.PrimaryDns, json["simGroup"]?["primaryDns"].ToString());
            Assert.Equal(_sim1.SimGroup.SecondaryDns, json["simGroup"]?["secondaryDns"].ToString());
            Assert.Equal(_sim1.SimGroup.IsolatedNw1PrimaryDns, json["simGroup"]?["isolatedNw1PrimaryDns"].ToString());
            Assert.Equal(_sim1.SimGroup.IsolatedNw1SecondaryDns, json["simGroup"]?["isolatedNw1SecondaryDns"].ToString());
            Assert.Equal(_sim1.SimGroup.Organization.Code, json["simGroup"]?["organization"]?["code"]);
            Assert.Equal(_sim1.SimGroup.Organization.Name, json["simGroup"]?["organization"]?["name"].ToString());
        }
    }
}
