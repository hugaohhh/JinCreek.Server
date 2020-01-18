using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織登録
    /// </summary>
    [Collection("Sequential")]
    public class RegisterTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public RegisterTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            if (context.User.Count(user => true) > 0) return;
            context.User.Add(new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") });
            context.User.Add(new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1") });
            context.SaveChanges();
        }

        // TODO: Theory (https://xunit.net/docs/getting-started/netfx/visual-studio#write-first-theory) にできない？
        private static JObject NewObject(string delegatePhone = null, string url = null, string adminPhone = null,
            string adminMail = null, string startDay = null, string endDay = null)
        {
            return JObject.FromObject(new
            {
                code = "code",
                name = "name",
                address = "address",
                delegatePhone = delegatePhone ?? "0123456789",
                url = url ?? "https://example.com",
                adminPhone = adminPhone ?? "1123456789",
                adminMail = adminMail ?? "admin@example.com",
                startDay = startDay ?? "2020-01-08",
                endDay = endDay ?? "2021-01-08",
                isValid = true,
            });
        }

        /// <summary>
        /// 異常：ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var obj = NewObject();
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.Empty(result.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// 異常：入力が空
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = new { };
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);

            // Assert
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]);
        }

        /// <summary>
        /// 異常：電話番語が数値以外の文字含む
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            Run(_client, token, NewObject(delegatePhone: "1234567890a", adminPhone: "2345678901b"));

            static void Run(HttpClient client, string token, JObject obj)
            {
                var result = Utils.Post(client, Url, Utils.CreateJsonContent(obj), token);
                var body = result.Content.ReadAsStringAsync().Result;
                var json = JObject.Parse(body);

                Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                Assert.NotNull(json["traceId"]);
                Assert.NotNull(json["errors"]?["DelegatePhone"]);
                Assert.NotNull(json["errors"]?["AdminPhone"]);
                Assert.Null(json["errors"]?["Url"]);
                Assert.Null(json["errors"]?["AdminMail"]);
                Assert.Null(json["errors"]?["StartDay"]);
                Assert.Null(json["errors"]?["EndDay"]);
                Assert.Null(json["errors"]?["IsValid"]);
            }
        }

        /// <summary>
        /// 異常：電話番語が9文字以下 or 12文字以上
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            Run(_client, token, NewObject(delegatePhone: "123456789", adminPhone: "123456789")); // 9文字
            Run(_client, token, NewObject(delegatePhone: "123456789012", adminPhone: "123456789012")); // 12文字

            static void Run(HttpClient client, string token, JObject obj)
            {
                var result = Utils.Post(client, Url, Utils.CreateJsonContent(obj), token);
                var body = result.Content.ReadAsStringAsync().Result;
                var json = JObject.Parse(body);

                Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                Assert.NotNull(json["traceId"]);
                Assert.NotNull(json["errors"]?["DelegatePhone"]);
                Assert.NotNull(json["errors"]?["AdminPhone"]);
                Assert.Null(json["errors"]?["Url"]);
                Assert.Null(json["errors"]?["AdminMail"]);
                Assert.Null(json["errors"]?["StartDay"]);
                Assert.Null(json["errors"]?["EndDay"]);
                Assert.Null(json["errors"]?["IsValid"]);
            }
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var obj = NewObject();
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Equal(obj["name"], json["name"]);
            Assert.Equal(obj["address"], json["address"]);
            Assert.Equal(obj["delegatePhone"], json["delegatePhone"]);
            Assert.Equal(obj["url"], json["url"]);
            Assert.Equal(obj["adminPhone"], json["adminPhone"]);
            Assert.Equal(obj["adminMail"], json["adminMail"]);
            Assert.Equal(obj["startDay"], json["startDay"]);
            Assert.Equal(obj["endDay"], json["endDay"]);
            Assert.Equal(obj["isValid"], json["isValid"]);
        }
    }
}
