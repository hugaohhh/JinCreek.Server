using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        [Fact]
        public void Case1()
        {
            var token = Utils.GetAccessToken(_client, "user0@example.com", "User0#");

            var obj = NewObject();
            var result = Post(_client, Url, Utils.CreateHttpContent(obj), token);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
        }

        [Fact]
        public void Case2()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");

            var obj = new { };
            var result = Post(_client, Url, Utils.CreateHttpContent(obj), token);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
        }

        [Fact]
        public void Case3()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");

            Run(_client, token, NewObject(telNo: "1234567890a"));
            Run(_client, token, NewObject(adminTelNo: "1234567890a"));

            static void Run(HttpClient client, string token, JObject obj)
            {
                var result = Post(client, Url, Utils.CreateHttpContent(obj), token);

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

        [Fact]
        public void Case4()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");

            Run(_client, token, NewObject(telNo: "123456789", adminTelNo: "123456789")); // 9文字
            Run(_client, token, NewObject(telNo: "123456789012", adminTelNo: "123456789012")); // 12文字

            static void Run(HttpClient client, string token, JObject obj)
            {
                var result = Post(client, Url, Utils.CreateHttpContent(obj), token);

                Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
                var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                Assert.NotNull(json["traceId"]);
                Assert.NotNull(json["errors"]?["telNo"]);
                Assert.NotNull(json["errors"]?["adminTelNo"]);
            }
        }

        [Fact]
        public void Case5()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");

            var obj = NewObject();
            var result = Post(_client, Url, Utils.CreateHttpContent(obj), token);

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

        // TODO: 拡張メソッドにする？
        private static HttpResponseMessage Post(HttpClient client, string url, HttpContent content, string bearer)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.Content = content;
            return client.SendAsync(request).Result;
        }
    }
}
