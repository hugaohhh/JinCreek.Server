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
    /// 組織更新
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class UpdateTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public UpdateTests(CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup> factory)
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
        /// 異常：ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
                startDate = "2020-01-17",
                endDate = "2021-01-17",
                isValid = true
            };
            var (response, body, _) = Utils.Put(_client, $"{Url}/1", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// 異常：URLにCodeがない
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new { };
            var (response, body, _) = Utils.Put(_client, $"{Url}", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// 異常：Codeが数字以外
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new { };
            var (response, _, json) = Utils.Put(_client, $"{Url}/a", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["code"]);
            Assert.NotNull(json["errors"]["Name"]);
            Assert.NotNull(json["errors"]["Address"]);
            Assert.NotNull(json["errors"]["Phone"]);
            Assert.NotNull(json["errors"]["Url"]);
            Assert.NotNull(json["errors"]["AdminPhone"]);
            Assert.NotNull(json["errors"]["AdminMail"]);
            Assert.NotNull(json["errors"]["StartDate"]);
            Assert.Null(json["errors"]["EndDate"]); // 不在
            Assert.NotNull(json["errors"]["IsValid"]);
        }

        /// <summary>
        /// 異常：代表電話番号, コーポレートサイトURL, 管理者連絡先電話番号, 管理者連絡先メールアドレス, 利用開始日, 利用終了日, 有効フラグが不正な形式
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "1234567890a",  //数字以外の文字含む
                url = "ttps://example.com", // 先頭文字列"https://" or "http://"以外
                adminPhone = "2345678901b", // 数字以外の文字含む
                adminMail = "admin.example.com",    //xxx@xxx形式ではない
                startDate = "2020-13-32",    //存在しない日付
                endDate = "2021-00-00",  //存在しない日付
                isValid = "yes" //bool型以外
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/1", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["startDate"]);
            Assert.NotNull(json["errors"]?["endDate"]);
            Assert.NotNull(json["errors"]?["isValid"]);
            // ↑型変換エラーが先に出る (see https://dev.azure.com/initialpoint/JinCreek.Server/_workitems/edit/11/)
        }

        /// <summary>
        /// Case04の型変換エラー以外
        /// </summary>
        [Fact]
        public void Case04B()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "1234567890a",  //数字以外の文字含む
                url = "ttps://example.com", // 先頭文字列"https://" or "http://"以外
                adminPhone = "2345678901b", // 数字以外の文字含む
                adminMail = "admin.example.com",    //xxx@xxx形式ではない
                startDate = "2020-01-17",
                endDate = "2021-01-17",
                isValid = true
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/1", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]["code"]);
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
        /// 異常：代表電話番号, 管理者連絡先電話番号が不正な形式
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "123456789", // 9文字
                url = "https://example.com",
                adminPhone = "123456789012", // 12文字
                adminMail = "admin@example.com",
                startDate = "2020-01-17",
                endDate = "2020-01-16", // 利用開始日未満の日付
                isValid = true
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/1", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]["code"]);
            Assert.Null(json["errors"]["Name"]);
            Assert.Null(json["errors"]["Address"]);
            Assert.NotNull(json["errors"]["Phone"]);
            Assert.Null(json["errors"]["Url"]);
            Assert.NotNull(json["errors"]["AdminPhone"]);
            Assert.Null(json["errors"]["AdminMail"]);
            Assert.Null(json["errors"]["StartDate"]);
            Assert.Null(json["errors"]["EndDate"]); // ValidateProperty or ValidateObjectに失敗すると、IValidatableObject.Validate まで到達しない
            Assert.Null(json["errors"]["IsValid"]);
        }

        /// <summary>
        /// 利用開始日未満の日付
        /// </summary>
        [Fact]
        public void Case05B()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "1234567890",
                url = "https://example.com",
                adminPhone = "12345678901",
                adminMail = "admin@example.com",
                startDate = "2020-01-17",
                endDate = "2020-01-16", // 利用開始日未満の日付
                isValid = true,
                distributionServerIp = "127.0.0.1"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/1", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]["code"]);
            Assert.Null(json["errors"]["Name"]);
            Assert.Null(json["errors"]["Address"]);
            Assert.Null(json["errors"]["Phone"]);
            Assert.Null(json["errors"]["Url"]);
            Assert.Null(json["errors"]["AdminPhone"]);
            Assert.Null(json["errors"]["AdminMail"]);
            Assert.Null(json["errors"]["StartDate"]);
            Assert.NotNull(json["errors"]["EndDate"]);
            Assert.Null(json["errors"]["IsValid"]);
        }

        /// <summary>
        /// DBに組織が不在
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                name = "org3",
                address = "address1",
                phone = "1234567890",
                url = "https://example.com",
                adminPhone = "1234567890",
                adminMail = "admin@example.com",
                startDate = "2020-01-17",
                endDate = "2021-01-17",
                isValid = true,
                distributionServerIp = "127.0.0.1"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/3", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Organization)]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case07()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
                startDate = "2020-01-17",
                endDate = "2021-01-17",
                isValid = true,
                distributionServerIp = "127.0.0.1"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/1", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.NotNull(json["endDate"]);
        }

        /// <summary>
        /// 正常 終了日は空
        /// </summary>
        [Fact]
        public void Case08()
        {
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
                startDate = "2020-01-17",
                isValid = true,
                distributionServerIp = "127.0.0.1"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/1", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.Empty(json["endDate"]);
        }

        /// <summary>
        /// 名前重複チェック
        /// </summary>
        [Fact]
        public void Case09()
        {
            var obj = new
            {
                name = "org1",
                address = "address1",
                phone = "1234567890",
                url = "https://example.com",
                adminPhone = "2345678901",
                adminMail = "admin@example.com",
                startDate = "2020-01-17",
                endDate = "2021-01-17",
                isValid = true,
                distributionServerIp = "127.0.0.1"
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/2", Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]["code"]);
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
