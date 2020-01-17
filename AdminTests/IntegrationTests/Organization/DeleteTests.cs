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
    /// 組織削除
    /// </summary>
    [Collection("Sequential")]
    public class DeleteTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        private readonly JinCreek.Server.Common.Models.Organization _org1 =
            new JinCreek.Server.Common.Models.Organization
            {
                Code = 1,
                Name = "org1",
                StartDay = DateTime.Parse("2020-01-14"),
                EndDay = DateTime.Parse("2021-01-14"), IsValid = true
            };

        public DeleteTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            context.Organization.RemoveRange();
            context.Organization.Add(_org1);
            if (context.User.Count(user => true) > 0) return;
            context.User.Add(new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") });
            context.User.Add(new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1") });
            context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Delete(_client, $"{Url}/{_org1.Code}", token);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Empty(body);
        }

        /// <summary>
        /// codeがない
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Delete(_client, $"{Url}/", token);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Empty(body);
        }

        /// <summary>
        /// codeが数字以外
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Delete(_client, $"{Url}/aaaaaaaaaaaaaaaaaaaa", token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var json = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["code"]);
        }

        /// <summary>
        /// 組織が不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Delete(_client, $"{Url}/3", token);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Delete(_client, $"{Url}/{_org1.Code}", token);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(_org1.Code, json["code"]);
        }
    }
}
