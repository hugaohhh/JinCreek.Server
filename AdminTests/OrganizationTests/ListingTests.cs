using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.OrganizationTests
{
    /// <summary>
    /// 組織一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ListingTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;
        private const string Url = "/api/organizations";

        public ListingTests(CustomWebApplicationFactoryWithMariaDb<Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);
            _context.Add(new SuperAdmin { AccountName = "user0", Name = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var org1 = Utils.CreateOrganization(code: 1, name: "org1");
            var domain1 = new Domain { Name = "domain1", Organization = org1 };
            var user1 = new UserAdmin { AccountName = "user1", Name = "user1", Password = Utils.HashPassword("user1"), Domain = domain1 };
            _context.AddRange(org1, domain1, user1);
            _context.SaveChanges();

            var (response, body, _) = Utils.Get(_client, Url, "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// フィルターなし
        /// </summary>
        [Fact]
        public void Case02()
        {
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1"));
            _context.SaveChanges();

            var query = new
            {
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(2, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count); // 1ページ件数
            Assert.Equal(1, (int)list[0]["code"]);
            Assert.Equal(2, (int)list[1]["code"]); // コード昇順
        }

        /// <summary>
        /// ページ：初期ページ
        /// </summary>
        [Fact]
        public void Case03()
        {
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-15"), endDate: DateTime.Parse("2120-01-15")));
            _context.Add(Utils.CreateOrganization(code: 3, name: "org3", startDate: DateTime.Parse("2020-01-16"), endDate: DateTime.Parse("2120-01-16")));
            _context.Add(Utils.CreateOrganization(code: 4, name: "org4", startDate: DateTime.Parse("2020-01-17"), endDate: DateTime.Parse("2120-01-17")));
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5", startDate: DateTime.Parse("2020-01-18"), endDate: DateTime.Parse("2120-01-18"), isValid: false));
            _context.Add(Utils.CreateOrganization(code: 11, name: "X_org11", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.SaveChanges();

            var query = new
            {
                page = 1,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(6, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            //Assert.NotNull(json["traceId"]);
            //Assert.Null(json["errors"]); // 不在
        }

        /// <summary>
        /// 名前フィルター 部分一致
        /// </summary>
        [Fact]
        public void Case04()
        {
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-15"), endDate: DateTime.Parse("2120-01-15")));
            _context.Add(Utils.CreateOrganization(code: 3, name: "org3", startDate: DateTime.Parse("2020-01-16"), endDate: DateTime.Parse("2120-01-16")));
            _context.Add(Utils.CreateOrganization(code: 4, name: "org4", startDate: DateTime.Parse("2020-01-17"), endDate: DateTime.Parse("2120-01-17")));
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5", startDate: DateTime.Parse("2020-01-18"), endDate: DateTime.Parse("2120-01-18"), isValid: false));
            _context.Add(Utils.CreateOrganization(code: 11, name: "X_org11", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.SaveChanges();

            var query = new
            {
                name = "org1",
                page = 1,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("1", array[0]["code"]);
            Assert.Equal("11", array[1]["code"]);
        }

        /// <summary>
        /// 利用開始日範囲開始フィルター
        /// </summary>
        [Fact]
        public void Case05()
        {
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-15"), endDate: DateTime.Parse("2120-01-15")));
            _context.Add(Utils.CreateOrganization(code: 3, name: "org3", startDate: DateTime.Parse("2020-01-16"), endDate: DateTime.Parse("2120-01-16")));
            _context.Add(Utils.CreateOrganization(code: 4, name: "org4", startDate: DateTime.Parse("2020-01-17"), endDate: DateTime.Parse("2120-01-17")));
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5", startDate: DateTime.Parse("2020-01-18"), endDate: DateTime.Parse("2120-01-18"), isValid: false));
            _context.Add(Utils.CreateOrganization(code: 11, name: "X_org11", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.SaveChanges();

            var query = new
            {
                startDateFrom = "2020-01-16",
                page = 1,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(3, array.Count);
            Assert.Equal("3", array[0]["code"]);
            Assert.Equal("4", array[1]["code"]);
            Assert.Equal("5", array[2]["code"]);
        }

        /// <summary>
        /// 利用開始日範囲終了フィルター
        /// </summary>
        [Fact]
        public void Case06()
        {
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-15"), endDate: DateTime.Parse("2120-01-15")));
            _context.Add(Utils.CreateOrganization(code: 3, name: "org3", startDate: DateTime.Parse("2020-01-16"), endDate: DateTime.Parse("2120-01-16")));
            _context.Add(Utils.CreateOrganization(code: 4, name: "org4", startDate: DateTime.Parse("2020-01-17"), endDate: DateTime.Parse("2120-01-17")));
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5", startDate: DateTime.Parse("2020-01-18"), endDate: DateTime.Parse("2120-01-18"), isValid: false));
            _context.Add(Utils.CreateOrganization(code: 11, name: "X_org11", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.SaveChanges();

            var query = new
            {
                startDateTo = "2020-01-16",
                page = 1,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(4, array.Count);
            Assert.Equal("1", array[0]["code"]);
            Assert.Equal("2", array[1]["code"]);
            Assert.Equal("3", array[2]["code"]);
            Assert.Equal("11", array[3]["code"]);
        }

        /// <summary>
        /// 利用終了日範囲開始フィルター
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-15"), endDate: DateTime.Parse("2120-01-15")));
            _context.Add(Utils.CreateOrganization(code: 3, name: "org3", startDate: DateTime.Parse("2020-01-16"), endDate: DateTime.Parse("2120-01-16")));
            _context.Add(Utils.CreateOrganization(code: 4, name: "org4", startDate: DateTime.Parse("2020-01-17"), endDate: DateTime.Parse("2120-01-17")));
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5", startDate: DateTime.Parse("2020-01-18"), endDate: DateTime.Parse("2120-01-18"), isValid: false));
            _context.Add(Utils.CreateOrganization(code: 11, name: "X_org11", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.SaveChanges();

            var query = new
            {
                endDateFrom = "2120-01-16",
                page = 1,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(3, array.Count);
            Assert.Equal("3", array[0]["code"]);
            Assert.Equal("4", array[1]["code"]);
            Assert.Equal("5", array[2]["code"]);
        }

        /// <summary>
        /// 利用終了日範囲終了フィルター
        /// </summary>
        [Fact]
        public void Case08()
        {
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-15"), endDate: DateTime.Parse("2120-01-15")));
            _context.Add(Utils.CreateOrganization(code: 3, name: "org3", startDate: DateTime.Parse("2020-01-16"), endDate: DateTime.Parse("2120-01-16")));
            _context.Add(Utils.CreateOrganization(code: 4, name: "org4", startDate: DateTime.Parse("2020-01-17"), endDate: DateTime.Parse("2120-01-17")));
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5", startDate: DateTime.Parse("2020-01-18"), endDate: DateTime.Parse("2120-01-18"), isValid: false));
            _context.Add(Utils.CreateOrganization(code: 11, name: "X_org11", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.SaveChanges();

            var query = new
            {
                endDateTo = "2120-01-16",
                page = 1,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(4, array.Count);
            Assert.Equal("1", array[0]["code"]);
            Assert.Equal("2", array[1]["code"]);
            Assert.Equal("3", array[2]["code"]);
            Assert.Equal("11", array[3]["code"]);
        }

        /// <summary>
        /// ページ：初期ページ、ページサイズ：存在、ソート：コード昇順
        /// </summary>
        [Fact]
        public void Case09()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 1,
                pageSize = 2,
                sortBy = "code",
                orderBy = "asc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal(1, array[0]["code"]);
            Assert.Equal(2, array[1]["code"]);
        }

        /// <summary>
        /// ページ：初期ページ、ページサイズ：存在、ソート：コード降順
        /// </summary>
        [Fact]
        public void Case10()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 1,
                pageSize = 2,
                sortBy = "code",
                orderBy = "desc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal(42, array[0]["code"]);
            Assert.Equal(41, array[1]["code"]);
        }

        /// <summary>
        /// ページ：0以下、ページサイズ：存在、ソート：コード降順
        /// </summary>
        [Fact]
        public void Case11()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                page = 0,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var array = (JArray)json["results"];
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// ページ：中間ページ、ページサイズ：不在、ソート：名前昇順
        /// </summary>
        [Fact]
        public void Case12()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 2,
                sortBy = "name",
                orderBy = "asc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            for (var i = 0; i < 20; i++) Assert.Equal(i + 21, array[i]["code"]);
        }

        /// <summary>
        /// ページ：中間ページ、ページサイズ：不在、ソート：名前降順
        /// </summary>
        [Fact]
        public void Case13()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 2,
                sortBy = "name",
                orderBy = "desc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            for (var i = 0; i < 20; i++) Assert.Equal(22 - i, array[i]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：存在、ソート：利用開始日昇順
        /// </summary>
        [Fact]
        public void Case14()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 21,
                pageSize = 2,
                sortBy = "startDate",
                orderBy = "asc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal(41, array[0]["code"]);
            Assert.Equal(42, array[1]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：存在、ソート：利用開始日降順
        /// </summary>
        [Fact]
        public void Case15()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 21,
                pageSize = 2,
                sortBy = "startDate",
                orderBy = "desc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal(2, array[0]["code"]);
            Assert.Equal(1, array[1]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：不在、ソート：利用終了日昇順
        /// </summary>
        [Fact]
        public void Case16()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 3,
                sortBy = "endDate",
                orderBy = "asc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal(2, array[0]["code"]);
            Assert.Equal(1, array[1]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：不在、ソート：利用終了日降順
        /// </summary>
        [Fact]
        public void Case17()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 3,
                sortBy = "endDate",
                orderBy = "desc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal(41, array[0]["code"]);
            Assert.Equal(42, array[1]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ超過、ページサイズ：不在、ソート：利用終了日降順
        /// </summary>
        [Fact]
        public void Case18()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = true,
                page = 4,
                sortBy = "endDate",
                orderBy = "desc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// ページ：初期ページ、ページサイズ：不在、ソート：有効昇順（invalid -> valid）
        /// </summary>
        [Fact]
        public void Case19()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = false,
                page = 1,
                sortBy = "isValid",
                orderBy = "asc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal(43, array[0]["code"]); // invalid
            Assert.Equal(44, array[1]["code"]); // invalid
        }

        /// <summary>
        /// ページ：初期ページ、ページサイズ：不在、ソート：有効降順(valid -> invalid)
        /// </summary>
        [Fact]
        public void Case20()
        {
            for (var i = 1; i <= 42; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i)));
            for (var i = 43; i <= 44; i++) _context.Add(Utils.CreateOrganization(code: i, name: $"org{i:00}", startDate: DateTime.Parse("2020-01-01").AddDays(i), endDate: DateTime.Parse("2120-01-31").AddDays(-i), isValid: false));
            _context.SaveChanges();

            var query = new
            {
                isValid = false,
                page = 1,
                sortBy = "isValid",
                orderBy = "desc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal(43, array[0]["code"]); // invalid
            Assert.Equal(44, array[1]["code"]); // invalid
        }

        /// <summary>
        /// 全フィルター
        /// </summary>
        [Fact]
        public void Case21()
        {
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-15"), endDate: DateTime.Parse("2120-01-15")));
            _context.Add(Utils.CreateOrganization(code: 3, name: "org3", startDate: DateTime.Parse("2020-01-16"), endDate: DateTime.Parse("2120-01-16")));
            _context.Add(Utils.CreateOrganization(code: 4, name: "org4", startDate: DateTime.Parse("2020-01-17"), endDate: DateTime.Parse("2120-01-17")));
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5", startDate: DateTime.Parse("2020-01-18"), endDate: DateTime.Parse("2120-01-18"), isValid: false));
            _context.Add(Utils.CreateOrganization(code: 11, name: "X_org11", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.SaveChanges();

            var query = new
            {
                name = "org3",
                startDateFrom = "2020-01-15",
                startDateTo = "2020-01-16",
                endDateFrom = "2120-01-16",
                endDateTo = "2120-01-17",
                isValid = true,
                page = 1,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Single(array);
            Assert.Equal("3", array[0]["code"]);
        }

        /// <summary>
        /// 全フィルター
        /// </summary>
        [Fact]
        public void Case22()
        {
            _context.Add(Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.Add(Utils.CreateOrganization(code: 2, name: "org2", startDate: DateTime.Parse("2020-01-15"), endDate: DateTime.Parse("2120-01-15")));
            _context.Add(Utils.CreateOrganization(code: 3, name: "org3", startDate: DateTime.Parse("2020-01-16"), endDate: DateTime.Parse("2120-01-16")));
            _context.Add(Utils.CreateOrganization(code: 4, name: "org4", startDate: DateTime.Parse("2020-01-17"), endDate: DateTime.Parse("2120-01-17")));
            _context.Add(Utils.CreateOrganization(code: 5, name: "org5", startDate: DateTime.Parse("2020-01-18"), endDate: DateTime.Parse("2120-01-18"), isValid: false));
            _context.Add(Utils.CreateOrganization(code: 11, name: "X_org11", startDate: DateTime.Parse("2020-01-14"), endDate: DateTime.Parse("2120-01-14")));
            _context.SaveChanges();

            var query = new
            {
                name = "org6",
                startDateFrom = "2020-01-19",
                startDateTo = "2020-01-13",
                endDateFrom = "2120-01-19",
                endDateTo = "2120-01-13",
                isValid = true,
                page = 1,
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
            //Assert.Equal(2, array.Count);
            //Assert.Equal("1", array[0]["code"]);
            //Assert.Equal("11", array[1]["code"]);
        }
    }
}
