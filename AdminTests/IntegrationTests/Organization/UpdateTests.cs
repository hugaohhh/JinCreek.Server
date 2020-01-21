using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織更新
    /// </summary>
    [Collection("Sequential")]
    public class UpdateTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations/mine";

        private readonly JinCreek.Server.Common.Models.Organization _org1 =
            new JinCreek.Server.Common.Models.Organization
            {
                Code = 1,
                Name = "org1",
                StartDay = DateTime.Parse("2020-01-14"),
                EndDay = DateTime.Parse("2021-01-14"),
                IsValid = true
            };
        private readonly JinCreek.Server.Common.Models.Organization _org2 =
            new JinCreek.Server.Common.Models.Organization
            {
                Code = 2,
                Name = "org2",
                StartDay = DateTime.Parse("2020-01-14"),
                EndDay = DateTime.Parse("2021-01-14"),
                IsValid = true
            };

        public UpdateTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            var domain1 = new Domain { Id = Guid.Parse("93b25287-7516-4051-9c8e-d114fad099ab"), DomainName = "domain0", Organization = _org1 };
            var domain2 = new Domain { Id = Guid.Parse("1d0beb2a-15ad-452d-9ecd-e6caf66fa501"), DomainName = "domain1", Organization = _org2 };
            var user0 = new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") }; // スーパー管理者
            var user1 = new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1"), Domain = domain1 }; // ユーザー管理者1
            var user2 = new AdminUser { AccountName = "USER2", Password = Utils.HashPassword("user2"), Domain = domain2 }; // ユーザー管理者2

            // Arrange
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            context.Organization.RemoveRange(context.Organization);
            context.Domain.RemoveRange(context.Domain);
            context.User.RemoveRange(context.User);
            context.SaveChanges();
            context.Organization.Add(_org1);
            context.Organization.Add(_org2);
            context.Domain.Add(domain1);
            context.Domain.Add(domain2);
            context.User.Add(user0);
            context.User.Add(user1);
            context.User.Add(user2);
            context.SaveChanges();
        }

        /// <summary>
        /// 異常：入力が空
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new { };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["url"]);
            Assert.NotNull(json["errors"]?["name"]);
            Assert.NotNull(json["errors"]?["address"]);
            Assert.NotNull(json["errors"]?["adminMail"]);
            Assert.NotNull(json["errors"]?["adminPhone"]);
            Assert.NotNull(json["errors"]?["delegatePhone"]);
        }

        /// <summary>
        /// 異常：コードが数字以外
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                code = "a", //コード数字以外設定 
                name = "org1",
                address = "address1",
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com"
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["code"]);
        }

        /// <summary>
        /// 異常：不正な形式
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                code = 1,
                name = "org1",
                address = "address1",
                delegatePhone = "1234567890a", // 数字以外の文字含む
                url = "fuga", // 先頭文字列"https://" or "http://"以外
                adminPhone = "2345678901b", // 数字以外の文字含む
                adminMail = "piyo" // "xxx@xxx"形式でない
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["DelegatePhone"]);
            Assert.NotNull(json["errors"]?["Url"]);
            Assert.NotNull(json["errors"]?["AdminPhone"]);
            Assert.NotNull(json["errors"]?["AdminMail"]);
        }

        /// <summary>
        /// 異常：不正な形式
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                code = 1,
                name = "org1",
                address = "address1",
                delegatePhone = "123456789", // 9文字以下 or 12文字以上
                url = "https://example.com",
                adminPhone = "123456789012", // 9文字以下 or 12文字以上
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["DelegatePhone"]);
            Assert.NotNull(json["errors"]?["AdminPhone"]);
        }

        /// <summary>
        /// 異常：DBに組織が不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                code = 3,
                name = "org1",
                //address = "address1",　//TODO テストケースより、住所設定削除予定
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["organization"]);
        }

        /// <summary>
        /// 異常：DBに組織が存在（他組織）
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                code = 2,
                name = "org2",
                // address = "address1", // 不在
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["role"]);
        }

        /// <summary>
        /// 正常：DBに組織が存在（自組織）
        /// </summary>
        [Fact]
        public void Case07()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                code = 1,
                name = "org1",
                address = "address1",
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(body);
            Assert.Null(json["traceId"]); // 不在
            Assert.Equal(obj.code, json["code"]);
            Assert.Equal(obj.name, json["name"]);
            Assert.Equal(obj.address, json["address"]);
            Assert.Equal(obj.delegatePhone, json["delegatePhone"]);
            Assert.Equal(obj.url, json["url"]);
            Assert.Equal(obj.adminPhone, json["adminPhone"]);
            Assert.Equal(obj.adminMail, json["adminMail"]);
        }
    }
}
