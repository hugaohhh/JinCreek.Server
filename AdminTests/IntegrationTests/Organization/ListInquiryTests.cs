using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織一覧照会
    /// </summary>
    public class ListInquiryTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public ListInquiryTests(CustomWebApplicationFactory<Admin.Startup> factory)
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
            var result = Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["role"]);
        }

        [Fact]
        public void Case2()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]); // 不在
        }

        [Fact]
        public void Case3()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Get(_client, $"{Url}?name=hoge", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]); // 不在
        }

        // TODO: 拡張メソッドにする？
        private static HttpResponseMessage Get(HttpClient client, string url, string bearer)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            return client.SendAsync(request).Result;
        }
    }
}
