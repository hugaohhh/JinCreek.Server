using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.OrganizationTests
{
    /// <summary>
    /// 自己組織更新
    /// </summary>
    [Collection("Sequential")]
    public class UpdateMineTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<Admin.Startup>>
    {
        private const string Url = "/api/organizations/mine";
        private readonly HttpClient _client;

        public UpdateMineTests(CustomWebApplicationFactoryWithMariaDb<Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();

            var org1 = Utils.CreateOrganization(code: 1, name: "org1");
            var org2 = Utils.CreateOrganization(code: 2, name: "org2");
            var domain1 = new Domain { Id = Guid.Parse("93b25287-7516-4051-9c8e-d114fad099ab"), Name = "domain1", Organization = org1 };
            var user0 = new SuperAdmin { AccountName = "user0", Name = "", Password = Utils.HashPassword("user0") }; // スーパー管理者
            var user1 = new UserAdmin { AccountName = "user1", Name = "", Password = Utils.HashPassword("user1"), Domain = domain1 }; // ユーザー管理者

            Utils.RemoveAllEntities(context);
            context.Add(org1);
            context.Add(org2);
            context.Add(domain1);
            context.Add(user0);
            context.Add(user1);
            context.SaveChanges();
        }

        /// <summary>
        /// 異常：入力が空
        /// </summary>
        [Fact]
        public void Case01()
        {
            var obj = new { };
            var (response, _, json) = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Name"]);
            Assert.NotNull(json["errors"]["Address"]);
            Assert.NotNull(json["errors"]["Phone"]);
            Assert.NotNull(json["errors"]["Url"]);
            Assert.NotNull(json["errors"]["AdminPhone"]);
            Assert.NotNull(json["errors"]["AdminMail"]);
            Assert.Null(json["errors"]["StartDate"]);
            Assert.Null(json["errors"]["EndDate"]);
            Assert.Null(json["errors"]["IsValid"]);
        }

        /// <summary>
        /// 異常：不正な形式
        /// </summary>
        [Fact]
        public void Case02()
        {
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "1234567890a", // 数字以外の文字含む
                url = "fuga", // 先頭文字列"https://" or "http://"以外
                adminPhone = "2345678901b", // 数字以外の文字含む
                adminMail = "piyo" // "xxx@xxx"形式でない
            };
            var (response, _, json) = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]["Name"]);
            Assert.Null(json["errors"]["Address"]);
            Assert.NotNull(json["errors"]["Phone"]);
            Assert.NotNull(json["errors"]["Url"]);
            Assert.NotNull(json["errors"]["AdminPhone"]);
            Assert.NotNull(json["errors"]["AdminMail"]);
            Assert.Null(json["errors"]["StartDate"]);
            Assert.Null(json["errors"]["EndDate"]);
            Assert.Null(json["errors"]["IsValid"]);
        }

        /// <summary>
        /// 異常：不正な形式
        /// </summary>
        [Fact]
        public void Case03()
        {
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "123456789", // 9文字
                url = "https://example.com",
                adminPhone = "123456789012", // 12文字以上
                adminMail = "admin@example.com",
            };
            var (response, _, json) = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]["Name"]);
            Assert.Null(json["errors"]["Address"]);
            Assert.NotNull(json["errors"]["Phone"]);
            Assert.Null(json["errors"]["Url"]);
            Assert.NotNull(json["errors"]["AdminPhone"]);
            Assert.Null(json["errors"]["AdminMail"]);
            Assert.Null(json["errors"]["StartDate"]);
            Assert.Null(json["errors"]["EndDate"]);
            Assert.Null(json["errors"]["IsValid"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case04()
        {
            var obj = new
            {
                code = 111,
                name = "org111",
                address = "address1",
                phone = "1234567890",
                url = "https://example.com",
                adminPhone = "1234567890",
                adminMail = "admin@example.com",
                distributionServerIp = "127.0.0.1"
            };
            var (response, _, json) = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
        }

        /// <summary>
        /// 名前重複チェック
        /// </summary>
        [Fact]
        public void Case05()
        {
            var obj = new
            {
                code = 1,
                name = "org2",
                address = "address1",
                phone = "1234567890",
                url = "https://example.com",
                adminPhone = "1234567890",
                adminMail = "admin@example.com",
                distributionServerIp = "127.0.0.1"
            };
            var (response, _, json) = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Name"]);
            Assert.Null(json["errors"]["Address"]);
            Assert.Null(json["errors"]["Phone"]);
            Assert.Null(json["errors"]["Url"]);
            Assert.Null(json["errors"]["AdminPhone"]);
            Assert.Null(json["errors"]["AdminMail"]);
            Assert.Null(json["errors"]["StartDate"]);
            Assert.Null(json["errors"]["EndDate"]);
            Assert.Null(json["errors"]["IsValid"]);
        }
    }
}
