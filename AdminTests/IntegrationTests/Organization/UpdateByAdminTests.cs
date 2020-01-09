using Newtonsoft.Json.Linq;
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

        [Fact]
        public void Case1()
        {
            var token = Utils.GetAccessToken(_client, "user1@example.com", "User1#");
        }
    }
}
