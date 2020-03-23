using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.OrganizationTests
{
    /// <summary>
    /// 組織登録
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    public class RegisterTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<Admin.Startup>>
    {
        private const string Url = "/api/organizations";

        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private readonly Organization _org1;
        private readonly Domain _domain1;

        public RegisterTests(CustomWebApplicationFactoryWithMariaDb<Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);
            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_domain1 = new Domain { Name = "domain1", Organization = _org1 });
            _context.Add(new SuperAdmin { AccountName = "user0", Name = "", Password = Utils.HashPassword("user0") });
            _context.Add(new UserAdmin { AccountName = "user1", Name = "", Password = Utils.HashPassword("user1"), Domain = _domain1 });
            _context.SaveChanges();
        }

        private static JObject NewObject(int code = 2, string name = "name2", string address = "address",
            string phone = "0123456789", string url = "https://example.com", string adminPhone = "1123456789",
            string adminMail = "admin@example.com", string startDate = "2020-01-08", string endDate = "2021-01-08",
            string isValid = "true", string distributionServerIp = "127.0.0.1")
        {
            return JObject.FromObject(new
                {code, name, address, phone, url, adminPhone, adminMail, startDate, endDate, isValid, distributionServerIp });
        }

        /// <summary>
        /// 異常：ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var obj = NewObject();
            var (response, body, _) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// 異常：入力が空
        /// </summary>
        [Fact]
        public void Case02()
        {
            var obj = new { };
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Name"]);
            Assert.NotNull(json["errors"]?["Address"]);
            Assert.NotNull(json["errors"]?["Phone"]);
            Assert.NotNull(json["errors"]?["Url"]);
            Assert.NotNull(json["errors"]?["AdminPhone"]);
            Assert.NotNull(json["errors"]?["AdminMail"]);
            Assert.NotNull(json["errors"]?["StartDate"]);
            Assert.Null(json["errors"]?["EndDate"]); // 不在
            Assert.NotNull(json["errors"]?["IsValid"]);
        }

        /// <summary>
        /// 異常：電話番語が数値以外の文字含む
        /// </summary>
        [Fact]
        public void Case03()
        {
            var obj = NewObject(phone: "1234567890a", url: "example.com", adminPhone: "2345678901b", adminMail: "admin.example.com", startDate: "2020-00-32", endDate: "2020-13-00", isValid: "hoge");
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]?["Name"]);
            Assert.Null(json["errors"]?["Address"]);
            Assert.Null(json["errors"]?["Phone"]);
            Assert.Null(json["errors"]?["Url"]);
            Assert.Null(json["errors"]?["AdminPhone"]);
            Assert.Null(json["errors"]?["AdminMail"]);
            Assert.NotNull(json["errors"]?["startDate"]);
            Assert.NotNull(json["errors"]?["endDate"]);
            Assert.NotNull(json["errors"]?["isValid"]);
            // ↑型変換エラーが先に出る。 see https://dev.azure.com/initialpoint/JinCreek.Server/_workitems/edit/11/
        }

        /// <summary>
        /// Case03の型変換エラー以外：
        /// </summary>
        [Fact]
        public void Case03B()
        {
            var obj = NewObject(phone: "1234567890a", url: "example.com", adminPhone: "2345678901b", adminMail: "admin.example.com");
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]?["Name"]);
            Assert.Null(json["errors"]?["Address"]);
            Assert.NotNull(json["errors"]?["Phone"]);
            Assert.NotNull(json["errors"]?["Url"]);
            Assert.NotNull(json["errors"]?["AdminPhone"]);
            Assert.NotNull(json["errors"]?["AdminMail"]);
            Assert.Null(json["errors"]?["startDate"]);
            Assert.Null(json["errors"]?["endDate"]);
            Assert.Null(json["errors"]?["isValid"]);
        }

        /// <summary>
        /// 異常：電話番語が9文字以下 or 12文字以上
        /// </summary>
        [Fact]
        public void Case04()
        {
            var obj1 = NewObject(phone: "123456789", adminPhone: "123456789012");
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj1), "user0", "user0"); // スーパー管理者

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]?["Name"]);
            Assert.Null(json["errors"]?["Address"]);
            Assert.NotNull(json["errors"]?["Phone"]);
            Assert.Null(json["errors"]?["Url"]);
            Assert.NotNull(json["errors"]?["AdminPhone"]);
            Assert.Null(json["errors"]?["AdminMail"]);
            Assert.Null(json["errors"]?["StartDate"]);
            Assert.Null(json["errors"]?["EndDate"]);
            Assert.Null(json["errors"]?["IsValid"]);
        }

        /// <summary>
        /// 異常：利用開始日 > 利用終了日
        /// </summary>
        [Fact]
        public void Case05()
        {
            var obj = NewObject(startDate: "2020-01-08", endDate: "2020-01-07");
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Null(json["errors"]?["Name"]);
            Assert.Null(json["errors"]?["Address"]);
            Assert.Null(json["errors"]?["Phone"]);
            Assert.Null(json["errors"]?["Url"]);
            Assert.Null(json["errors"]?["AdminPhone"]);
            Assert.Null(json["errors"]?["AdminMail"]);
            Assert.Null(json["errors"]?["StartDate"]);
            Assert.NotNull(json["errors"]?["EndDate"]);
            Assert.Null(json["errors"]?["IsValid"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case06()
        {
            var obj = NewObject();
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Equal(obj["name"], json["name"]);
            Assert.Equal(obj["address"], json["address"]);
            Assert.Equal(obj["phone"], json["phone"]);
            Assert.Equal(obj["url"], json["url"]);
            Assert.Equal(obj["adminPhone"], json["adminPhone"]);
            Assert.Equal(obj["adminMail"], json["adminMail"]);
            Assert.Equal(obj["startDate"], json["startDate"]);
            Assert.Equal(obj["endDate"], json["endDate"]);
            Assert.Equal(obj["isValid"], json["isValid"]);
        }

        /// <summary>
        /// 正常 終了日は空
        /// </summary>
        [Fact]
        public void Case07()
        {
            var obj = NewObject(endDate: null);
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Equal(obj["name"], json["name"]);
            Assert.Equal(obj["address"], json["address"]);
            Assert.Equal(obj["phone"], json["phone"]);
            Assert.Equal(obj["url"], json["url"]);
            Assert.Equal(obj["adminPhone"], json["adminPhone"]);
            Assert.Equal(obj["adminMail"], json["adminMail"]);
            Assert.Equal(obj["startDate"], json["startDate"]);
            Assert.Equal(obj["isValid"], json["isValid"]);
            Assert.Empty(json["endDate"]);
        }

        /// <summary>
        /// 名前重複
        /// </summary>
        [Fact]
        public void Case08()
        {
            var obj = NewObject(name: "org1");
            var (response, _, json) = Utils.Post(_client, Url, Utils.CreateJsonContent(obj), "user0", "user0"); // スーパー管理者

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Name"]);
            Assert.Null(json["errors"]?["Address"]);
            Assert.Null(json["errors"]?["Phone"]);
            Assert.Null(json["errors"]?["Url"]);
            Assert.Null(json["errors"]?["AdminPhone"]);
            Assert.Null(json["errors"]?["AdminMail"]);
            Assert.Null(json["errors"]?["StartDate"]);
            Assert.Null(json["errors"]?["EndDate"]);
            Assert.Null(json["errors"]?["IsValid"]);
        }
    }
}
