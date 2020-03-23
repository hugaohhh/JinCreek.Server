using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimGroupTests
{
    /// <summary>
    /// Simグループ登録
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class RegisterTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;
        private const string Url = "/api/sim-groups";

        private readonly Organization _org1;
        private readonly Domain _domain1;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly SimGroup _simGroup1;

        public RegisterTests(CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Name = "", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Name = "", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_simGroup1 = Utils.CreateSimGroup(organization: _org1, name: "simGroup1", isolatedNw1IpPool: "isolatedNw1IpPool"));
            _context.SaveChanges();
        }

        /// <summary>
        /// 異常：ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new { };
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// 組織：不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                Name = "simGroup1",
                Apn = "apn",
                IsolatedNw1IpPool = "IsolatedNw1IpPool",
                IsolatedNw1IpRange = "192.168.1.32/27",
                AuthenticationServerIp = "127.0.0.1",
                Nw1SecondaryDns = "127.0.0.1",
                IsolatedNw1PrimaryDns = "127.0.0.1",
                NasIp = "127.0.0.1",
                PrimaryDns = "127.0.0.1",
                SecondaryDns = "127.0.0.1",
                UserNameSuffix = "a",
            };
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["OrganizationCode"]);
        }

        /// <summary>
        /// 組織:存在； 　他は全部空
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                OrganizationCode = "1",
            };
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Name"]);
            Assert.Equal("The Apn field is required.", json["errors"]?["Apn"].First.ToString());
            Assert.Equal("The IsolatedNw1IpPool field is required.", json["errors"]?["IsolatedNw1IpPool"].First.ToString());
            Assert.Equal("The IsolatedNw1IpRange field is required.", json["errors"]?["IsolatedNw1IpRange"].First.ToString());
            Assert.Equal("The AuthenticationServerIp field is required.", json["errors"]?["AuthenticationServerIp"].First.ToString());
            //Assert.Equal("The IsolatedNw1SecondaryDns field is required.", json["errors"]?["IsolatedNw1SecondaryDns"].First.ToString());
            //Assert.Equal("The IsolatedNw1PrimaryDns field is required.", json["errors"]?["IsolatedNw1PrimaryDns"].First.ToString());
            Assert.Equal("The NasIp field is required.", json["errors"]?["NasIp"].First.ToString());
            Assert.Equal("The PrimaryDns field is required.", json["errors"]?["PrimaryDns"].First.ToString());
            Assert.Equal("The SecondaryDns field is required.", json["errors"]?["SecondaryDns"].First.ToString());
        }

        /// <summary>
        /// 組織:存在；名前：存在 　他は全部指定値以外
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                Name = "simGroup1",
                OrganizationCode = "1",
                Apn = "ああああ",　// 半角以外
                IsolatedNw1IpPool = "いい",    // 半角以外
                IsolatedNw1IpRange = "192.168.1.32",  //CIDRではない
                AuthenticationServerIp = "aaaaaaa",  // ipAddressではない
                IsolatedNw1SecondaryDns = "aaaaaaa",    // ipAddressではない
                IsolatedNw1PrimaryDns = "aaaaaaa",  // ipAddressではない
                NasIp = "aaaaaaa",   // ipAddressではない
                PrimaryDns = "aaaaaaa", // ipAddressではない
                SecondaryDns = "aaaaaaa",    // ipAddressではない
                UserNameSuffix = "a",
            };
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("Apn_is_only_ASCII", json["errors"]?["Apn"].First.ToString());
            Assert.Equal("IsolatedNw1IpPool_is_only_ASCII", json["errors"]?["IsolatedNw1IpPool"].First.ToString());
            Assert.Equal("The field is not a valid CIDR.", json["errors"]?["IsolatedNw1IpRange"].First.ToString());
            Assert.Equal("The field is not a valid IP address.", json["errors"]?["AuthenticationServerIp"].First.ToString());
            Assert.Equal("The field is not a valid IP address.", json["errors"]?["IsolatedNw1SecondaryDns"].First.ToString());
            Assert.Equal("The field is not a valid IP address.", json["errors"]?["IsolatedNw1PrimaryDns"].First.ToString());
            Assert.Equal("The field is not a valid IP address.", json["errors"]?["NasIp"].First.ToString());
            Assert.Equal("The field is not a valid IP address.", json["errors"]?["PrimaryDns"].First.ToString());
            Assert.Equal("The field is not a valid IP address.", json["errors"]?["SecondaryDns"].First.ToString());
        }

        /// <summary>
        /// 名前:'他レコードと重複して存在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                OrganizationCode = "1",
                Name = "simGroup1",
                Apn = "apn",
                IsolatedNw1IpPool = "IsolatedNw1IpPool",
                IsolatedNw1IpRange = "192.168.1.32/27",
                AuthenticationServerIp = "127.0.0.1",
                IsolatedNw1SecondaryDns = "127.0.0.1",
                IsolatedNw1PrimaryDns = "127.0.0.1",
                NasIp = "127.0.0.1",
                PrimaryDns = "127.0.0.1",
                SecondaryDns = "127.0.0.1",
                UserNameSuffix = "a",
            };
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["SimGroup"]);
        }

        /// <summary>
        /// 全部正常
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                OrganizationCode = "1",
                Name = "simGroup2333",
                Apn = "apn",
                IsolatedNw1IpPool = "IsolatedNw1IpPool",
                IsolatedNw1IpRange = "192.168.1.32/27",
                AuthenticationServerIp = "127.0.0.1",
                IsolatedNw1SecondaryDns = "127.0.0.1",
                IsolatedNw1PrimaryDns = "127.0.0.1",
                NasIp = "127.0.0.1",
                PrimaryDns = "127.0.0.1",
                SecondaryDns = "127.0.0.1",
                UserNameSuffix = "a",
            };
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            var json = JObject.Parse(body);
            Assert.Null(json["traceId"]);
            Assert.NotNull(json["organization"]["name"]);
            Assert.Equal(obj.OrganizationCode, json["organization"]["code"]);
            Assert.Equal(obj.Name, json["name"]);
            Assert.Equal(obj.Apn, json["apn"]);
            Assert.Equal(obj.IsolatedNw1IpPool, json["isolatedNw1IpPool"]);
            Assert.Equal(obj.AuthenticationServerIp, json["authenticationServerIp"]);
            Assert.Equal(obj.IsolatedNw1IpRange, json["isolatedNw1IpRange"]);
            Assert.Equal(obj.IsolatedNw1SecondaryDns, json["isolatedNw1SecondaryDns"]);
            Assert.Equal(obj.IsolatedNw1PrimaryDns, json["isolatedNw1PrimaryDns"]);
            Assert.Equal(obj.NasIp, json["nasIp"]);
            Assert.Equal(obj.PrimaryDns, json["primaryDns"]);
            Assert.Equal(obj.SecondaryDns, json["secondaryDns"]);
        }

    }
}
