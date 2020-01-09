using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織更新
    /// </summary>
    public class UpdateTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public UpdateTests(CustomWebApplicationFactory<Admin.Startup> factory)
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
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var obj = new { };
            var result = Put(_client, Url, Utils.CreateHttpContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }

        // TODO: 拡張メソッドにする？
        private static HttpResponseMessage Put(HttpClient client, string url, HttpContent content, string bearer)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.Content = content;
            return client.SendAsync(request).Result;
        }
    }
}
