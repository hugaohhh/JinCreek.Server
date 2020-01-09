using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織削除
    /// </summary>
    public class DeleteTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public DeleteTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            Utils.RegisterUser(_client, "user0@example.com", "User0#"); // TODO: ユーザー管理者にする
            Utils.RegisterUser(_client, "user1@example.com", "User1#"); // TODO: スーパー管理者にする
        }

        [Fact]
        public void Case1()
        {
            var token = Utils.GetAccessToken(_client, "user0@example.com", "User0#");
            var result = Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["role"]);
        }

        [Fact]
        public void Case2()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        [Fact]
        public void Case3()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        [Fact]
        public void Case4()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["organization"]);
        }

        [Fact]
        public void Case5()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]); // 不在
        }

        // TODO: 拡張メソッドにする？
        private static HttpResponseMessage Delete(HttpClient client, string url, string bearer)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            return client.SendAsync(request).Result;
        }
    }
}
