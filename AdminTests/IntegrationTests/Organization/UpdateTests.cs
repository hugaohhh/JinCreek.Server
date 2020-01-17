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
        private const string Url = "/api/organizations";

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
            context.Organization.RemoveRange();
            context.Organization.Add(_org1);
            context.Organization.Add(_org2);
            context.Domain.RemoveRange();
            context.Domain.Add(domain1);
            context.Domain.Add(domain2);
            context.User.RemoveRange();
            context.User.Add(user0);
            context.User.Add(user1);
            context.User.Add(user2);
            context.SaveChanges();

            //// Arrange
            //using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            //var context = scope.ServiceProvider.GetService<MainDbContext>();
            //context.Organization.Add(_org1);
            //if (context.User.Count(user => true) > 0) return;
            //context.User.Add(new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") });
            //context.User.Add(new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1") });
            //context.SaveChanges();
        }

        /// <summary>
        /// 異常：全て不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new { };
            var result = Utils.Put(_client, $"{Url}/{_org1.Code}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Url"]);
            Assert.NotNull(json["errors"]?["Name"]);
            Assert.NotNull(json["errors"]?["Address"]);
            Assert.NotNull(json["errors"]?["AdminMail"]);
            Assert.NotNull(json["errors"]?["AdminPhone"]);
            Assert.NotNull(json["errors"]?["DelegatePhone"]);
        }

        /// <summary>
        /// 異常：不正な形式
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                name = "org1",
                address = "address1",
                delegatePhone = "1234567890a", // 数字以外の文字含む
                url = "fuga", // 先頭文字列"https://" or "http://"以外
                adminPhone = "2345678901b", // 数字以外の文字含む
                adminMail = "piyo", // "xxx@xxx"形式でない
            };
            var result = Utils.Put(_client, $"{Url}/{_org1.Code}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            //Assert.NotNull(json["errors"]?["AdminPhone"]);
            //Assert.NotNull(json["errors"]?["DelegatePhone"]);
            //Assert.NotNull(json["errors"]?["AdminMail"]);
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
                name = "org1",
                address = "address1",
                delegatePhone = "123456789", // 9文字以下 or 12文字以上
                url = "https://example.com",
                adminPhone = "2345678901b", // 数字以外の文字含む
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{_org1.Code}", Utils.CreateJsonContent(obj), token);
            //Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["AdminPhone"]);
            Assert.NotNull(json["errors"]?["DelegatePhone"]);
        }

        /// <summary>
        /// 異常：DBに組織が不在
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
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{obj.code}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
        }

        /// <summary>
        /// 異常：DBに組織が存在（他組織）
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                name = "org1",
                // address = "address1", // 不在
                telno = "1234567890",
                url = "https://example.com",
                delegatePhone = "1234567890",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{_org1.Code}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }

        /// <summary>
        /// 正常：DBに組織が存在（自組織）
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var obj = new
            {
                Code = "1",
                name = "org1",
                address = "address1",
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{_org1.Code}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            //var body = result.Content.ReadAsStringAsync().Result;
            //var json = JObject.Parse(body);
            //Assert.NotNull(json["traceId"]);
            //Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }
    }
}
