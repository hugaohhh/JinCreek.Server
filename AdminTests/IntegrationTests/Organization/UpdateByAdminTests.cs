using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織更新byスーパー管理者
    /// </summary>
    public class UpdateByAdminTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        private readonly JinCreek.Server.Common.Models.Organization _org1 =
            new JinCreek.Server.Common.Models.Organization
            {
                Id = Guid.NewGuid(),
                Code = "1",
                Name = "org1",
                StartDay = DateTime.Parse("2020-01-14"),
                EndDay = DateTime.Parse("2021-01-14"),
                AdminMail = "admin@example.com",
                IsValid = true,
                Address = "Address",
                AdminPhone = "1234567890",
                DelegatePhone = "2345678901",
                Url = "https://example.com",
            };

        public UpdateByAdminTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            context.Organization.RemoveRange();
            context.Organization.Add(_org1);
            if (context.User.Count(user => true) > 0) return;
            context.User.Add(new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") });
            context.User.Add(new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1") });
            context.SaveChanges();
        }

        /// <summary>
        /// 異常：ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var obj = new
            {
                Code = "1",
                id = _org1.Id,
                name = "org1",
                address = "address1",
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{_org1.Id}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }

        /// <summary>
        /// 異常：入力が空
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new { };
            var result = Utils.Put(_client, $"{Url}/{_org1.Id}", Utils.CreateJsonContent(obj), token);
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
        /// 異常：ID, 代表電話番号, コーポレートサイトURL, 管理者連絡先電話番号, 管理者連絡先メールアドレス, 利用開始日, 利用終了日, 有効フラグが不正な形式
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                id = "aaaaaaaaaaaaaaaa",
                name = "org1",
                address = "address1",
                delegatePhone = "123456789", // 9文字以下 or 12文字以上
                url = "https://example.com",
                adminPhone = "2345678901b", // 数字以外の文字含む
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{_org1.Id}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["$.id"]);
        }

        /// <summary>
        /// 異常：代表電話番号, 管理者連絡先電話番号が不正な形式
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                id = _org1.Id,
                name = "org1",
                address = "address1",
                delegatePhone = "123456789", // 9文字
                url = "https://example.com",
                adminPhone = "123456789012", // 12文字
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{_org1.Id}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["AdminPhone"]);
            Assert.NotNull(json["errors"]?["DelegatePhone"]);
        }

        /// <summary>
        /// 異常：住所が不在、DBに組織が不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                id = Guid.NewGuid(),
                name = "org1",
                address = "address1",
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "1234567890",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{obj.id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                Code = "1",
                id = _org1.Id,
                name = "org1",
                address = "address1",
                delegatePhone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
            };
            var result = Utils.Put(_client, $"{Url}/{_org1.Id}", Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
