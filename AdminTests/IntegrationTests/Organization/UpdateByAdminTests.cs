using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織更新byスーパー管理者
    /// </summary>
    public class UpdateByAdminTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public UpdateByAdminTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            Utils.RegisterUser(_client, "user0@example.com", "User0#"); // TODO: ユーザー管理者にする
            Utils.RegisterUser(_client, "user1@example.com", "User1#"); // TODO: スーパー管理者にする
        }

        /// <summary>
        /// 異常：ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
        }
        
        /// <summary>
        /// 異常：入力が空
        /// </summary>
        [Fact]
        public void Case02()
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
        /// 異常：ID, 代表電話番号, コーポレートサイトURL, 管理者連絡先電話番号, 管理者連絡先メールアドレス, 利用開始日, 利用終了日, 有効フラグが不正な形式
        /// </summary>
        [Fact]
        public void Case03()
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
        /// 異常：代表電話番号, 管理者連絡先電話番号が不正な形式
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
        }
        
        /// <summary>
        /// 異常：住所が不在、DBに組織が不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
        }
        
        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
        }
    }
}
