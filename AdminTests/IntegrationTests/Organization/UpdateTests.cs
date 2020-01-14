using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
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

        //// TODO: Theory (https://xunit.net/docs/getting-started/netfx/visual-studio#write-first-theory) にできない？
        //private static JObject NewObject(string telNo = null, string url = null, string adminTelNo = null,
        //    string adminEmail = null, string startAt = null, string endAt = null)
        //{
        //    return JObject.FromObject(new
        //    {
        //        name = "Name",
        //        address = "Address",
        //        telNo = telNo ?? "0123456789",
        //        url = url ?? "https://example.com",
        //        adminTelNo = adminTelNo ?? "1123456789",
        //        adminEmail = adminEmail ?? "admin@example.com",
        //        startAt = startAt ?? "2020-01-08",
        //        endAt = endAt ?? "2021-01-08",
        //        isActive = true,
        //    });
        //}

        /// <summary>
        /// 異常：全て不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var obj = new { };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }

        /// <summary>
        /// 異常：不正な形式
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var obj = new
            {
                id = "hoge", // 数字以外
                name = "org1",
                address = "address1",
                telno = "1234567890a", // 数字以外の文字含む
                url = "fuga", // 先頭文字列"https://" or "http://"以外
                admintelno = "2345678901b", // 数字以外の文字含む
                adminemail = "piyo", // "xxx@xxx"形式でない
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }

        /// <summary>
        /// 異常：不正な形式
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var obj = new
            {
                id = "hoge", // TODO: GUID
                name = "org1",
                address = "address1",
                telno = "123456789", // 9文字以下 or 12文字以上
                url = "https://example.com",
                admintelno = "2345678901b", // 数字以外の文字含む
                adminemail = "admin@example.com",
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }

        /// <summary>
        /// 異常：DBに組織が不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var obj = new
            {
                id = "hoge", // TODO: GUID
                name = "org1",
                // address = "address1", // 不在
                telno = "1234567890",
                url = "https://example.com",
                admintelno = "2345678901",
                adminemail = "admin@example.com",
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }

        /// <summary>
        /// 異常：DBに組織が存在（他組織）
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var obj = new
            {
                id = "hoge", // TODO: GUID
                name = "org1",
                // address = "address1", // 不在
                telno = "1234567890",
                url = "https://example.com",
                admintelno = "2345678901",
                adminemail = "admin@example.com",
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }

        /// <summary>
        /// 正常：DBに組織が存在（自組織）
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
            var obj = new
            {
                id = "hoge", // TODO: GUID
                name = "org1",
                address = "address1",
                telno = "1234567890",
                url = "https://example.com",
                admintelno = "2345678901",
                adminemail = "admin@example.com",
            };
            var result = Utils.Put(_client, Url, Utils.CreateJsonContent(obj), token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["telNo"]); // TODO: 3..12
        }
    }
}
