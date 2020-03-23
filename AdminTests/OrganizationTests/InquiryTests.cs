using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.OrganizationTests
{
    /// <summary>
    /// 組織照会
    /// </summary>
    [Collection("Sequential")]
    public class InquiryTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        private readonly Organization _org1 = Utils.CreateOrganization(code: 1, name: "org1");
        private readonly Organization _org2 = Utils.CreateOrganization(code: 2, name: "org2");

        public InquiryTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            var domain1 = new Domain { Name = "domain0", Organization = _org1 };
            var domain2 = new Domain { Name = "domain1", Organization = _org2 };

            var user0 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }; // スーパー管理者
            var user1 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = domain1 }; // ユーザー管理者1
            var user2 = new UserAdmin { AccountName = "user2", Password = Utils.HashPassword("user2"), Domain = domain2 }; // ユーザー管理者2

            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(context);
            context.Add(_org1);
            context.Add(_org2);
            context.Add(domain1);
            context.Add(domain2);
            context.Add(user0);
            context.Add(user1);
            context.Add(user2);
            context.SaveChanges();
        }

        /// <summary>
        /// 異常：IDがない
        /// </summary>
        [Fact]
        public void Case1()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain0"); // ユーザー管理者1
            var result = Utils.Get(_client, $"{Url}/", token);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Empty(body);
        }

        /// <summary>
        /// 異常：IDが数字以外
        /// </summary>
        [Fact]
        public void Case2()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/aaaaaaaaaaaaaaaaaaaa", token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["code"]);
        }

        /// <summary>
        /// 異常：組織が不在
        /// </summary>
        [Fact]
        public void Case3()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/3", token);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Organization)]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case4()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/{_org1.Code}", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.Null(json["errors"]?["traceId"]);    //トレースID不在チェック
            Assert.Equal(_org1.Name, json["name"]);
            Assert.Equal(_org1.Address, json["address"]);
            Assert.Equal(_org1.Phone, json["phone"]);
            Assert.Equal(_org1.Url, json["url"]);
            Assert.Equal(_org1.AdminPhone, json["adminPhone"]);
            Assert.Equal(_org1.AdminMail, json["adminMail"]);
            Assert.Equal(_org1.StartDate, json["startDate"]);
            Assert.Equal(_org1.EndDate, json["endDate"]);
            Assert.Equal(_org1.IsValid, json["isValid"]);
        }
    }
}
