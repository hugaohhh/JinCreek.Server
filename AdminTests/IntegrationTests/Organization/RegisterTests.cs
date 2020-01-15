using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織登録
    /// </summary>
    public class RegisterTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public RegisterTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            Utils.RegisterUser(_client, "user0@example.com", "User0#"); // TODO: ユーザー管理者にする
            Utils.RegisterUser(_client, "user1@example.com", "User1#"); // TODO: スーパー管理者にする
        }

        // TODO: Theory (https://xunit.net/docs/getting-started/netfx/visual-studio#write-first-theory) にできない？
        private static JObject NewObject(string telNo = null, string url = null, string adminTelNo = null,
            string adminEmail = null, string startAt = null, string endAt = null)
        {
            return JObject.FromObject(new
            {
                name = "Name",
                address = "Address",
                telNo = telNo ?? "0123456789",
                url = url ?? "https://example.com",
                adminTelNo = adminTelNo ?? "1123456789",
                adminEmail = adminEmail ?? "admin@example.com",
                startAt = startAt ?? "2020-01-08",
                endAt = endAt ?? "2021-01-08",
                isActive = true,
            });
        }

        /// <summary>
        /// 異常：ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user0@example.com", "User0#"); // ユーザー管理者

            var obj = NewObject();
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
        }

        /// <summary>
        /// 異常：入力が空
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");

            var obj = new { };
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
        }

        /// <summary>
        /// 異常：電話番語が数値以外の文字含む
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");

            Run(_client, token, NewObject(telNo: "1234567890a"));
            Run(_client, token, NewObject(adminTelNo: "1234567890a"));

            static void Run(HttpClient client, string token, JObject obj)
            {
                var result = Utils.Post(client, Url, Utils.CreateJsonContent(obj), token);

                Assert.Equal(HttpStatusCode.Created, result.StatusCode);
                var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                Assert.NotNull(json["traceId"]);
                Assert.NotNull(json["errors"]?["telNo"]);
                Assert.NotNull(json["errors"]?["url"]);
                Assert.NotNull(json["errors"]?["adminEmail"]);
                Assert.NotNull(json["errors"]?["startAt"]);
                Assert.NotNull(json["errors"]?["endAt"]);
                Assert.NotNull(json["errors"]?["isActive"]);
            }
        }

        /// <summary>
        /// 異常：電話番語が9文字以下 or 12文字以上
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");

            Run(_client, token, NewObject(telNo: "123456789", adminTelNo: "123456789")); // 9文字
            Run(_client, token, NewObject(telNo: "123456789012", adminTelNo: "123456789012")); // 12文字

            static void Run(HttpClient client, string token, JObject obj)
            {
                var result = Utils.Post(client, Url, Utils.CreateJsonContent(obj), token);

                Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
                var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                Assert.NotNull(json["traceId"]);
                Assert.NotNull(json["errors"]?["telNo"]);
                Assert.NotNull(json["errors"]?["adminTelNo"]);
            }
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");

            var obj = NewObject();
            var result = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), token);

            // Assert
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.Equal(obj["name"], json["name"]);
            Assert.Equal(obj["address"], json["address"]);
            Assert.Equal(obj["telNo"], json["telNo"]);
            Assert.Equal(obj["url"], json["url"]);
            Assert.Equal(obj["adminTelNo"], json["adminTelNo"]);
            Assert.Equal(obj["adminEmail"], json["adminEmail"]);
            Assert.Equal(obj["startAt"], json["startAt"]);
            Assert.Equal(obj["endAt"], json["endAt"]);
            Assert.Equal(obj["isActive"], json["isActive"]);
        }
    }
}
