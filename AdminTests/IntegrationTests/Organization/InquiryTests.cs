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
    public class InquiryTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        private readonly JinCreek.Server.Common.Models.Organization _org1 =
            new JinCreek.Server.Common.Models.Organization
            {
                Id = Guid.NewGuid(),
                Code = "1",
                Name = "org1",
                StartDay = DateTime.Parse("2020-01-14"),
                EndDay = DateTime.Parse("2021-01-14"),
                IsValid = true
            };

        public InquiryTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            if (context.User.Count(user => true) > 0) return;
            context.User.Add(new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") });
            context.User.Add(new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1") });
            context.Organization.Add(_org1);
            context.SaveChanges();
        }

        /// <summary>
        /// 異常：IDがない
        /// </summary>
        [Fact]
        public void Case1()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/", token);
            //Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            //var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            //Assert.NotNull(json["traceId"]);
            //Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// 異常：IDが数字以外
        /// </summary>
        [Fact]
        public void Case2()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/aaaaaaaaaaaaaaaaaaaa", token);
            //Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            //var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            //Assert.NotNull(json["traceId"]);
            //Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// 異常：組織が不在
        /// </summary>
        [Fact]
        public void Case3()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/c1788aa7-9308-4661-bb84-dbc04e849e72", token);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            //Assert.NotNull(json["errors"]?["organization"]);
        }

        /// <summary>
        /// 異常：他組織
        /// </summary>
        [Fact]
        public void Case4()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["role"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case5()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/{_org1.Id}", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            //Assert.NotNull(json["traceId"]);
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
