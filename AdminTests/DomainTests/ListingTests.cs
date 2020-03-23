using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.DomainTests
{
    /// <summary>
    /// ドメイン一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ListingTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/domains";

        public ListingTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            var org1 = Utils.CreateOrganization(code: 1, name: "org1");
            var org3 = Utils.CreateOrganization(code: 3, name: "org3");
            var domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = org1 };
            var domain3 = new Domain { Id = Guid.NewGuid(), Name = "domain43", Organization = org3 };

            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(context);

            context.Add(org1);
            context.Add(org3);
            context.Add(domain1);
            context.Add(domain3);
            for (var i = 2; i <= 42; i++) context.Add(new Domain { Id = Guid.NewGuid(), Name = $"domain{i:00}", Organization = org1 }); // domain02～42
            context.Add(new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            context.Add(new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = domain1 }); // ユーザー管理者
            context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, body, _) = Utils.Get(_client, $"{Url}", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// コード：不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var array = (JArray)json["results"];
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["OrganizationCode"]);
        }

        /// <summary>
        /// コード：数字以外の文字含む
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=a", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var array = (JArray)json["results"];
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["OrganizationCode"]);
        }

        /// <summary>
        /// DBに組織が不在、ページ：1、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=2&page=1", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// ページ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(42, array.Count);
            Assert.Equal("domain01", array[0]["name"]);
            Assert.Equal("domain02", array[1]["name"]); // ドメイン名昇順
        }

        /// <summary>
        /// ページ：1、ページサイズ：存在、ソート：不在
        /// </summary>
        [Fact]
        public void Case06()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=1&pageSize=2", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("domain01", array[0]["name"]);
            Assert.Equal("domain02", array[1]["name"]); // ドメイン名昇順
        }

        /// <summary>
        /// ページ：0、ページサイズ：存在、ソート：不在
        /// </summary>
        [Fact]
        public void Case07()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=0&pageSize=2", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// ページ：中間ページ、ページサイズ：不在、ソート：ドメイン名昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=2&sortBy=name&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            Assert.Equal("domain21", array[0]["name"]);
            Assert.Equal("domain22", array[1]["name"]); // ドメイン名昇順
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：存在、ソート：ドメイン名降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=21&pageSize=2&sortBy=name&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("domain02", array[0]["name"]);
            Assert.Equal("domain01", array[1]["name"]); // ドメイン名降順
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case10()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=3", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("domain41", array[0]["name"]);
            Assert.Equal("domain42", array[1]["name"]); // ドメイン名昇順
        }

        /// <summary>
        /// ページ：最終ページ超過、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case11()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=4", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// ページ：中間ページ、ページサイズ：存在、ソート：不在
        /// </summary>
        [Fact]
        public void Case12()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=2&pageSize=2", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("domain03", array[0]["name"]);
            Assert.Equal("domain04", array[1]["name"]); // ドメイン名昇順
        }
    }
}
