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
    /// 自己組織照会
    /// </summary>
    [Collection("Sequential")]
    public class InquiryMineTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations/mine";

        private static readonly Organization Org1 = Utils.CreateOrganization(code: 1, name: "org1");
        private static readonly Domain Domain1 = new Domain { Name = "domain1", Organization = Org1 };
        private static readonly User User1 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = Domain1 }; // ユーザー管理者

        public InquiryMineTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(context);
            context.Add(Org1);
            context.Add(Domain1);
            context.Add(User1);
            context.SaveChanges();
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain1"); // ユーザー管理者
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(Org1.Code, json["code"]);
            Assert.Equal(Org1.Name, json["name"]);
            Assert.Equal(Org1.Address, json["address"]);
            Assert.Equal(Org1.Phone, json["phone"]);
            Assert.Equal(Org1.Url, json["url"]);
            Assert.Equal(Org1.AdminPhone, json["adminPhone"]);
            Assert.Equal(Org1.AdminMail, json["adminMail"]);
            Assert.Equal(Org1.StartDate, json["startDate"]);
            Assert.Equal(Org1.EndDate, json["endDate"]);
            Assert.Equal(Org1.IsValid, json["isValid"]);
        }
    }
}
