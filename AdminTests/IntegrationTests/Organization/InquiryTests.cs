using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織照会
    /// </summary>
    [Collection("Sequential")]
    public class InquiryTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        private readonly JinCreek.Server.Common.Models.Organization _org1 =
            new JinCreek.Server.Common.Models.Organization
            {
                Code = 1,
                Name = "org1",
                StartDay = DateTime.Parse("2020-01-14"),
                EndDay = DateTime.Parse("2021-01-14"),
                IsValid = true
            };
        private readonly JinCreek.Server.Common.Models.Organization _org2 =
            new JinCreek.Server.Common.Models.Organization
            {
                Code = 2,
                Name = "org2",
                StartDay = DateTime.Parse("2020-01-14"),
                EndDay = DateTime.Parse("2021-01-14"),
                IsValid = true
            };

        public InquiryTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            var domain1 = new Domain { Id = Guid.Parse("93b25287-7516-4051-9c8e-d114fad099ab"), DomainName = "domain0", Organization = _org1 };
            var domain2 = new Domain { Id = Guid.Parse("1d0beb2a-15ad-452d-9ecd-e6caf66fa501"), DomainName = "domain1", Organization = _org2 };

            var user0 = new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") }; // スーパー管理者
            var user1 = new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1"), Domain = domain1 }; // ユーザー管理者1
            var user2 = new AdminUser { AccountName = "USER2", Password = Utils.HashPassword("user2"), Domain = domain2 }; // ユーザー管理者2

            // Arrange
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            if (context.User.Count(user => true) > 0) return;
            context.Organization.Add(_org1);
            context.Organization.Add(_org2);
            context.Domain.Add(domain1);
            context.Domain.Add(domain2);
            context.User.Add(user0);
            context.User.Add(user1);
            context.User.Add(user2);
            context.SaveChanges();
        }

        /// <summary>
        /// 異常：IDがない
        /// </summary>
        [Fact]
        public void Case1()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
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
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
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
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var result = Utils.Get(_client, $"{Url}/3", token);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
        }

        /// <summary>
        /// 異常：他組織
        /// </summary>
        [Fact]
        public void Case4()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var result = Utils.Get(_client, $"{Url}/{_org2.Code}", token); // ユーザー管理者2の組織を照会
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case5()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者1
            var result = Utils.Get(_client, $"{Url}/{_org1.Code}", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.Null(json["errors"]?["id"]);
            Assert.Equal(_org1.Name, json["name"]);
            Assert.Equal(_org1.Address, json["address"]);
            Assert.Equal(_org1.DelegatePhone, json["delegatePhone"]);
            Assert.Equal(_org1.Url, json["url"]);
            Assert.Equal(_org1.AdminPhone, json["adminPhone"]);
            Assert.Equal(_org1.AdminMail, json["adminMail"]);
            Assert.Equal(_org1.StartDay, json["startDay"]);
            Assert.Equal(_org1.EndDay, json["endDay"]);
            Assert.Equal(_org1.IsValid, json["isValid"]);
        }
    }
}
