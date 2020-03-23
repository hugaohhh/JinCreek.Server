using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.OrganizationTests
{
    /// <summary>
    /// 組織削除
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    public class DeleteTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/organizations";

        private readonly Organization _org1;
        private readonly Domain _domain1;

        public DeleteTests(CustomWebApplicationFactoryWithMariaDb<Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);
            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_domain1 = new Domain { Name = "domain1", Organization = _org1 });
            _context.Add(new SuperAdmin { AccountName = "user0", Name = "", Password = Utils.HashPassword("user0") });
            _context.Add(new UserAdmin { AccountName = "user1", Name = "", Password = Utils.HashPassword("user1"), Domain = _domain1 });
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, body, _) = Utils.Delete(_client, $"{Url}/{_org1.Code}", "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// codeがない
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, body, _) = Utils.Delete(_client, $"{Url}/", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// codeが数字以外
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/a", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["code"]);
        }

        /// <summary>
        /// 組織が不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response, _, json) = Utils.Delete(_client, $"{Url}/3", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Organization)]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5"));
            _context.SaveChanges();

            var (response, _, json) = Utils.Delete(_client, $"{Url}/5", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(5, json["code"]);
        }

        /// <summary>
        /// 組織の子オブジェクトにSIMグループあり
        /// </summary>
        [Fact]
        public void Case06()
        {
            var organization = Utils.CreateOrganization(code: 5, name: "org5");
            var simGroup = new SimGroup
            {
                Id = Guid.NewGuid(),
                Name = "simGroup1",
                Organization = organization,
                Apn = "apn",
                AuthenticationServerIp = "AuthenticationServerIp",
                IsolatedNw1IpPool = "IsolatedNw1IpPool",
                IsolatedNw1SecondaryDns = "IsolatedNw1SecondaryDns",
                IsolatedNw1IpRange = "IsolatedNw1IpRange",
                IsolatedNw1PrimaryDns = "IsolatedNw1PrimaryDns",
                NasIp = "NasIp",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns",
                UserNameSuffix = "",
            };
            _context.AddRange(organization, simGroup);
            _context.SaveChanges();

            var (response, _, json) = Utils.Delete(_client, $"{Url}/5", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(SimGroup)]); // 存在
            Assert.Null(json["errors"][nameof(Domain)]);
            Assert.Null(json["errors"][nameof(OrganizationClientApp)]);
        }

        /// <summary>
        /// 組織の子オブジェクトにドメインあり
        /// </summary>
        [Fact]
        public void Case07()
        {
            var organization = Utils.CreateOrganization(code: 5, name: "org5");
            var domain = new Domain { Name = "domain", Organization = organization };
            _context.AddRange(organization, domain);
            _context.SaveChanges();

            var (response, _, json) = Utils.Delete(_client, $"{Url}/5", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"][nameof(SimGroup)]);
            Assert.NotNull(json["errors"][nameof(Domain)]); // 存在
            Assert.Null(json["errors"][nameof(OrganizationClientApp)]);
        }

        /// <summary>
        /// 組織の子オブジェクトに組織端末アプリバージョンあり
        /// </summary>
        [Fact]
        public void Case08()
        {
            var organization = Utils.CreateOrganization(code: 5, name: "org5");
            var clientOs = new ClientOs { Name = "Windows 10"};
            var clientApp = new ClientApp { ClientOs = clientOs, Version = "1904" };
            var organizationClientApp = new OrganizationClientApp { ClientApp = clientApp, Organization = organization };
            _context.AddRange(organization, organizationClientApp);
            _context.SaveChanges();

            var (response, _, json) = Utils.Delete(_client, $"{Url}/5", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"][nameof(SimGroup)]);
            Assert.Null(json["errors"][nameof(Domain)]);
            Assert.NotNull(json["errors"][nameof(OrganizationClientApp)]); // 存在
        }
    }
}
