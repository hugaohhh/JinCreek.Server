using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AdminTests.IntegrationTests.Organization
{
    /// <summary>
    /// 組織一覧照会のケース8～19
    /// ページとソートがあるケース
    /// </summary>
    [Collection("Sequential")]
    public class ListInquiryTests08_19 : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/organizations";

        public ListInquiryTests08_19(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            var orgs = new List<JinCreek.Server.Common.Models.Organization>();
            for (var i = 1; i <= 42; i++)
            {
                // 有効
                orgs.Add(new JinCreek.Server.Common.Models.Organization {Code = i, Name = $"org{i:00}", StartDay = DateTime.Parse("2020-01-01").AddDays(i), EndDay = DateTime.Parse("2021-01-31").AddDays(-i), IsValid = true });
            }
            for (var i = 43; i <= 44; i++)
            {
                // 無効
                orgs.Add(new JinCreek.Server.Common.Models.Organization { Code = i, Name = $"org{i:00}", StartDay = DateTime.Parse("2020-01-01").AddDays(i), EndDay = DateTime.Parse("2021-01-31").AddDays(-i), IsValid = false });
            }

            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            if (context.User.Count(user => true) > 0) return;
            context.User.Add(new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") });
            context.User.Add(new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1") });
            foreach (var org in orgs) context.Organization.Add(org);
            context.SaveChanges();
        }

        /// <summary>
        /// ページ：不在、ページサイズ：存在、ソート：コード昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&pageSize=2&sortBy=code&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(2, array.Count);
            Assert.Equal(1, array[0]["code"]);
            Assert.Equal(2, array[1]["code"]);
        }

        /// <summary>
        /// ページ：不在、ページサイズ：存在、ソート：コード降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&pageSize=2&sortBy=code&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(2, array.Count);
            Assert.Equal(42, array[0]["code"]);
            Assert.Equal(41, array[1]["code"]);
        }

        /// <summary>
        /// ページ：0以下、ページサイズ：存在、ソート：コード降順
        /// </summary>
        [Fact]
        public void Case10()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&page=0&pageSize=2&sortBy=code&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// ページ：中間ページ、ページサイズ：不在、ソート：名前昇順
        /// </summary>
        [Fact]
        public void Case11()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&page=2&sortBy=name&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(20, array.Count);
            for (var i = 0; i < 20; i++) Assert.Equal(i + 21, array[i]["code"]);
        }

        /// <summary>
        /// ページ：中間ページ、ページサイズ：不在、ソート：名前降順
        /// </summary>
        [Fact]
        public void Case12()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&page=2&sortBy=name&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(20, array.Count);
            for (var i = 0; i < 20; i++) Assert.Equal(22 - i, array[i]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：存在、ソート：利用開始日昇順
        /// </summary>
        [Fact]
        public void Case13()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&page=21&pageSize=2&sortBy=startDay&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(2, array.Count);
            Assert.Equal(41, array[0]["code"]);
            Assert.Equal(42, array[1]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：存在、ソート：利用開始日降順
        /// </summary>
        [Fact]
        public void Case14()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&page=21&pageSize=2&sortBy=startDay&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(2, array.Count);
            Assert.Equal(2, array[0]["code"]);
            Assert.Equal(1, array[1]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：不在、ソート：利用終了日昇順
        /// </summary>
        [Fact]
        public void Case15()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&page=3&sortBy=endDay&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(2, array.Count);
            Assert.Equal(2, array[0]["code"]);
            Assert.Equal(1, array[1]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：不在、ソート：利用終了日降順
        /// </summary>
        [Fact]
        public void Case16()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&page=3&sortBy=endDay&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(2, array.Count);
            Assert.Equal(41, array[0]["code"]);
            Assert.Equal(42, array[1]["code"]);
        }

        /// <summary>
        /// ページ：最終ページ超過、ページサイズ：不在、ソート：利用終了日降順
        /// </summary>
        [Fact]
        public void Case17()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?isValid=true&page=4&sortBy=endDay&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Empty(array);
        }

        /// <summary>
        /// ページ：不在、ページサイズ：不在、ソート：有効昇順（invalid -> valid）
        /// </summary>
        [Fact]
        public void Case18()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?sortBy=isValid&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(43, array[0]["code"]); // invalid
            Assert.Equal(44, array[1]["code"]); // invalid
            Assert.Equal(1, array[2]["code"]); // valid
            Assert.Equal(2, array[3]["code"]); // valid
        }

        /// <summary>
        /// ページ：不在、ページサイズ：不在、ソート：有効降順(valid -> invalid)
        /// </summary>
        [Fact]
        public void Case19()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/?sortBy=isValid&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var array = JArray.Parse(body);
            Assert.Equal(1, array[0]["code"]); // valid
            Assert.Equal(2, array[1]["code"]); // valid
            Assert.Equal(3, array[2]["code"]); // valid
            Assert.Equal(4, array[3]["code"]); // valid
        }
    }
}
