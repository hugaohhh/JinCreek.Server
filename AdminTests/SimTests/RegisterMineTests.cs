using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimTests
{
    /// <summary>
    /// 自分Sim登録
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class RegisterMineTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;
        private const string Url = "/api/sims/mine";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;

        public RegisterMineTests(CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Name = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Name = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
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
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["SimGroupId"]);
            Assert.Equal("The Msisdn field is required.", json["errors"]?["Msisdn"].First.ToString());
            Assert.Equal("The Imsi field is required.", json["errors"]?["Imsi"].First.ToString());
            Assert.Equal("The IccId field is required.", json["errors"]?["IccId"].First.ToString());
            //Assert.Equal("The UserName field is required.", json["errors"]?["UserName"].First.ToString());
            //Assert.Equal("The Password field is required.", json["errors"]?["Password"].First.ToString());
        }

        /// <summary>
        /// 全部指定値以外
        /// </summary>
        [Fact]
        public void Case02()
        {
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "abcde",　// 数字以外
                Imsi = "ancdssf",　 // 数字以外
                IccId = "111111111111111111111111111111111111111111111111111", // 19文字以上
                UserName = "あああ",　　//ASCII以外
                Password = "いいい"    //ASCII以外
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("msisdn_is_only_number", json["errors"]?["Msisdn"].First.ToString());
            Assert.Equal("imsi_is_only_number", json["errors"]?["Imsi"].First.ToString());
            Assert.Equal("The field IccId must be a string with a maximum length of 19.", json["errors"]?["IccId"].First.ToString());
            Assert.Equal("UserName_is_only_ASCII", json["errors"]?["UserName"].First.ToString());
            Assert.Equal("Password_is_only_ASCII", json["errors"]?["Password"].First.ToString());
        }


        /// <summary>
        /// 全部正常  DBに：SIMグループ：'不在
        /// </summary>
        [Fact]
        public void Case03()
        {
            var obj = new
            {
                SimGroupId = Guid.NewGuid(),
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimGroup)]);
        }

        /// <summary>
        /// 全部正常  DBに：SIMグループ：他組織
        /// </summary>
        [Fact]
        public void Case04()
        {
            var obj = new
            {
                SimGroupId = _simGroup2.Id,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// MSISDM:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case05()
        {
            _context.Add(new Sim()
            {
                SimGroup = _simGroup1,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password"
            });
            _context.SaveChanges();
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1001",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim02",
                Password = "password"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Msisdn"]);
        }

        /// <summary>
        /// IMSI:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case06()
        {
            _context.Add(new Sim()
            {
                SimGroup = _simGroup1,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password"
            });
            _context.SaveChanges();
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1002",
                Imsi = "1001",
                IccId = "1002",
                UserName = "sim02",
                Password = "password"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Imsi"]);
        }

        /// <summary>
        /// IccId:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(new Sim()
            {
                SimGroup = _simGroup1,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password"
            });
            _context.SaveChanges();
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1001",
                UserName = "sim02",
                Password = "password"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["IccId"]);
        }

        /// <summary>
        /// ユーザー名+ユーザー名サフィックス:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case08()
        {
            _context.Add(new Sim()
            {
                SimGroup = _simGroup1,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password"
            });
            _context.SaveChanges();
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1002",
                Imsi = "1002",
                IccId = "1002",
                UserName = "sim01",
                Password = "password"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["UserName"]);
        }


        /// <summary>
        /// 全部正常  DBに：SIMグループ：自組織
        /// </summary>
        [Fact]
        public void Case09()
        {
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                UserName = "sim01",
                Password = "password"
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.Equal(obj.SimGroupId, json["simGroup"]["id"]);
            Assert.Equal(obj.Msisdn, json["msisdn"]);
            Assert.Equal(obj.Imsi, json["imsi"]);
            Assert.Equal(obj.IccId, json["iccId"]);
            Assert.Equal(obj.UserName, json["userName"]);
            Assert.Equal(obj.Password, json["password"]);
        }

        /// <summary>
        /// 全部正常 UserName と　Password がNULL
        /// </summary>
        [Fact]
        public void Case10()
        {
            var obj = new
            {
                SimGroupId = _simGroup1.Id,
                Msisdn = "1001",
                Imsi = "1001",
                IccId = "1001",
                Password = "" // 空の場合
            };
            var (response, _, json) = Utils.Post(_client, $"{Url}/", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.Equal(obj.SimGroupId.ToString(), json["simGroup"]["id"].ToString());
            Assert.Equal(obj.Msisdn, json["msisdn"]);
            Assert.Equal(obj.Imsi, json["imsi"]);
            Assert.Equal(obj.IccId, json["iccId"]);
            Assert.NotNull(json["userName"]);
            Assert.NotNull(json["password"]);
        }

    }
}
