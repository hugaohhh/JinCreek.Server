using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織照会
    /// </summary>
    public class InquiryTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public InquiryTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            Utils.RegisterUser(_client, "user0@example.com", "User0#"); // TODO: ユーザー管理者にする
            Utils.RegisterUser(_client, "user1@example.com", "User1#"); // TODO: スーパー管理者にする
        }

        [Fact]
        public void Case1()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        [Fact]
        public void Case2()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        [Fact]
        public void Case3()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["organization"]);
        }

        [Fact]
        public void Case4()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["role"]);
        }

        [Fact]
        public void Case5()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }
    }
}
