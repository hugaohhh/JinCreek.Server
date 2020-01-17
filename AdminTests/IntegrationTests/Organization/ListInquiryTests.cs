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
    /// 組織一覧照会
    /// </summary>
    [Collection("Sequential")]
    public class ListInquiryTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public ListInquiryTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            if (context.User.Count(user => true) > 0) return;
            context.User.Add(new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") });
            context.User.Add(new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1") });
            context.Organization.Add(new JinCreek.Server.Common.Models.Organization { Id = Guid.NewGuid(), Code = "1", Name = "org1", StartDay = DateTime.Parse("2020-01-14"), EndDay = DateTime.Parse("2021-01-14"), IsValid = true, });
            context.Organization.Add(new JinCreek.Server.Common.Models.Organization { Id = Guid.NewGuid(), Code = "2", Name = "org2", StartDay = DateTime.Parse("2020-01-15"), EndDay = DateTime.Parse("2021-01-15"), IsValid = true, });
            context.Organization.Add(new JinCreek.Server.Common.Models.Organization { Id = Guid.NewGuid(), Code = "3", Name = "org3", StartDay = DateTime.Parse("2020-01-16"), EndDay = DateTime.Parse("2021-01-16"), IsValid = true, });
            context.Organization.Add(new JinCreek.Server.Common.Models.Organization { Id = Guid.NewGuid(), Code = "4", Name = "org4", StartDay = DateTime.Parse("2020-01-17"), EndDay = DateTime.Parse("2021-01-17"), IsValid = true, });
            context.Organization.Add(new JinCreek.Server.Common.Models.Organization { Id = Guid.NewGuid(), Code = "5", Name = "org5", StartDay = DateTime.Parse("2020-01-18"), EndDay = DateTime.Parse("2021-01-18"), IsValid = false, });
            context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Empty(body);
        }

        /// <summary>
        /// フィルターなし
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, Url, token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            //var json = JObject.Parse(body);
            //Assert.NotNull(json["traceId"]);
            //Assert.Null(json["errors"]); // 不在
        }

        /// <summary>
        /// 名前フィルター
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?name=org1", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Single(array);
            Assert.Equal("1", array[0]["code"]);
        }

        /// <summary>
        /// 利用開始日範囲開始フィルター
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?startdayfrom=2020-01-16", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Equal(3, array.Count);
            Assert.Equal("3", array[0]["code"]);
            Assert.Equal("4", array[1]["code"]);
            Assert.Equal("5", array[2]["code"]);
        }

        /// <summary>
        /// 利用開始日範囲終了フィルター
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?startdayto=2020-01-16", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Equal(3, array.Count);
            Assert.Equal("1", array[0]["code"]);
            Assert.Equal("2", array[1]["code"]);
            Assert.Equal("3", array[2]["code"]);
        }

        /// <summary>
        /// 利用終了日範囲開始フィルター
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?enddayfrom=2021-01-16", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Equal(3, array.Count);
            Assert.Equal("3", array[0]["code"]);
            Assert.Equal("4", array[1]["code"]);
            Assert.Equal("5", array[2]["code"]);
        }

        /// <summary>
        /// 利用終了日範囲終了フィルター
        /// </summary>
        [Fact]
        public void Case07()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?enddayto=2021-01-16", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Equal(3, array.Count);
            Assert.Equal("1", array[0]["code"]);
            Assert.Equal("2", array[1]["code"]);
            Assert.Equal("3", array[2]["code"]);
        }

        /// <summary>
        /// 有効フィルター：有効
        /// </summary>
        [Fact]
        public void Case08()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?isvalid=true", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Equal(4, array.Count);
            Assert.Equal("1", array[0]["code"]);
            Assert.Equal("2", array[1]["code"]);
            Assert.Equal("3", array[2]["code"]);
            Assert.Equal("4", array[3]["code"]);
        }

        /// <summary>
        /// 有効フィルター：無効
        /// </summary>
        [Fact]
        public void Case09()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?isvalid=false", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Single(array);
            Assert.Equal("5", array[0]["code"]);
        }

        /// <summary>
        /// 全フィルター
        /// </summary>
        [Fact]
        public void Case10()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?name=org3&startdayfrom=2020-01-15&startdayto=2020-01-16&enddayfrom=2021-01-16&enddayto=2021-01-17&isvalid=true", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Single(array);
            Assert.Equal("3", array[0]["code"]);
        }

        /// <summary>
        /// 全フィルター
        /// </summary>
        [Fact]
        public void Case11()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?name=org3&startdayfrom=2020-01-15&startdayto=2020-01-16&enddayfrom=2021-01-16&enddayto=2021-01-17&isvalid=true", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var array = JArray.Parse(body);
            Assert.Single(array);
            Assert.Equal("3", array[0]["code"]);
        }
    }
}
