using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
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

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user0@example.com", "User0#");
            var result = Utils.Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["role"]);
        }

        /// <summary>
        /// IDが不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// IDが数字以外
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// 組織が不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["organization"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var result = Utils.Delete(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]); // 不在
        }
    }
}
