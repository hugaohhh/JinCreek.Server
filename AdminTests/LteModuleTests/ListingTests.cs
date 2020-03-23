using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.LteModuleTests
{

    /// <summary>
    /// LTEモジュール　一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    public class ListingTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/lte-modules";

        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly Organization _org1;
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly Organization _org2;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly Domain _domain1;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private readonly Domain _domain1a;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")] private readonly Domain _domain2;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")] private readonly SuperAdmin _user1;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")] private readonly EndUser _user2;

        public ListingTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain1a = new Domain { Id = Guid.NewGuid(), Name = "domain01a", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1}); // ユーザー管理者
            _context.SaveChanges();
        }

        /// <summary>
        /// 全部空
        /// </summary>
        [Fact]
        public void Case01()
        {
            _context.Add(new LteModule { Name = "lte2", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, (int)json["count"]); // トータル件数
            Assert.NotEmpty(body);
            var list = (JArray)json["results"];
            Assert.Single(list);
            Assert.Equal("lte2", list[0]["name"]);
            Assert.Null(list[0]["traceId"]);
        }

        /// <summary>
        /// 名前フィルター: 不在　Software Radio State操作フィルター：不在　ページ数：（あり得ないページ）ソート：不在　PageSize：不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            _context.Add(new LteModule { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}?page=2000000000000000", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
            Assert.Equal("One or more validation errors occurred.", json["title"]);
        }

        /// <summary>
        /// 名前フィルター: 不在　Software Radio State操作フィルター：不在　ページ数：（最終ページ超過）ソート：不在　PageSize：不在 
        /// </summary>
        [Fact]
        public void Case03()
        {
            _context.Add(new LteModule { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?page=2", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, (int)json["count"]); // トータル件数
            var list = (JArray)json["results"];
            Assert.Empty(list);
        }

        /// <summary>
        /// 名前フィルター: 存在　Software Radio State操作フィルター：不在　ページ数：（初期ページ）名前ソート:'昇順　PageSize：存在
        /// </summary>
        [Fact]
        public void Case04()
        {
            _context.Add(new LteModule { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致
            _context.Add(new LteModule { Name = "lte2", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致
            _context.Add(new LteModule { Name = "abcd", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致せず
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}?Name=lte&page=1&pageSize=2&sortBy=name&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.NotEmpty(array);
            Assert.Equal(2, array.Count);
            Assert.Equal("lte1", array[0]["name"].ToString());
            Assert.Equal("lte2", array[1]["name"].ToString());// Lte名昇順
            foreach (var jToken in array)
            {
                Assert.NotEqual("abcd", jToken["name"].ToString());
            }

        }

        /// <summary>
        /// 名前フィルター: 不在　Software Radio State操作フィルター：ON　ページ数：（中間ページ）名前ソート:降順　PageSize：不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            for (var i = 2; i <= 43; i++)
                _context.Add(new LteModule { Name = $"lte{i:00}", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致　Software Radio State操作：'フィルターに合致
            for (var i = 44; i <= 83; i++)
                _context.Add(new LteModule { Name = $"lte{i:00}", UseSoftwareRadioState = false, NwAdapterName = "abc" });//名前：'フィルターに部分一致　Software Radio State操作：'フィルターに合致せず
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?page=2&sortBy=name&orderBy=desc&UseSoftwareRadioState=true", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            Assert.Equal("lte23", array[0]["name"].ToString());
            Assert.Equal("lte22", array[1]["name"].ToString()); // Lte名降順

        }

        /// <summary>
        /// 名前フィルター: 存在　Software Radio State操作フィルター：OFF　ページ数：（最終ページ）Software Radio State操作:昇順　PageSize：不在 
        /// </summary>
        [Fact]
        public void Case06()
        {
            for (var i = 2; i <= 43; i++)
                _context.Add(new LteModule { Name = $"lte{i:00}", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致　Software Radio State操作：'フィルターに合致せず
            for (var i = 44; i <= 89; i++)
                _context.Add(new LteModule { Name = $"lte{i:00}", UseSoftwareRadioState = false, NwAdapterName = "abc" });//名前：'フィルターに部分一致　Software Radio State操作：'フィルターに合致
            _context.Add(new LteModule { Name = "abcd", UseSoftwareRadioState = false, NwAdapterName = "abc" });//名前：'フィルターに部分一致せず　Software Radio State操作：'フィルターに合致
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?Name=lte&page=3&UseSoftwareRadioState=false&sortBy=UseSoftwareRadioState&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(46, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.InRange(array.Count, 1, 20);
            Assert.Equal("lte84", array[0]["name"].ToString());
            Assert.Equal("lte85", array[1]["name"].ToString()); // Software Radio State昇順
            foreach (var jToken in array)
            {
                Assert.NotEqual("abcd", jToken["name"].ToString());
            }

        }

        /// <summary>
        /// 名前フィルター: 存在　Software Radio State操作フィルター：ON　ページ数：（中間ページ）Software Radio State操作:降順　PageSize：存在 
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(new LteModule { Name = "lte2", UseSoftwareRadioState = false, NwAdapterName = "abc" });//名前：'フィルターに部分一致　Software Radio State操作：'フィルターに合致せず
            for (var i = 3; i <= 10; i++)
                _context.Add(new LteModule { Name = $"lte{i:00}", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致　Software Radio State操作：'フィルターに合致
            _context.Add(new LteModule { Name = "abcd", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致せず　Software Radio State操作：'フィルターに合致
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?Name=lte&page=3&pageSize=2&UseSoftwareRadioState=true&sortBy=UseSoftwareRadioState&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(8, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.NotEmpty(array);
            Assert.Equal(2, array.Count);
            Assert.Equal("lte07", array[0]["name"].ToString());
            Assert.Equal("lte08", array[1]["name"].ToString());// Software Radio State降順

        }

        /// <summary>
        /// 名前フィルター: 存在　Software Radio State操作フィルター：OFF　ページ数：（初期ページ）Software Radio State操作:降順　PageSize：存在 
        /// </summary>
        [Fact]
        public void Case08()
        {
            _context.Add(new LteModule { Name = "lte2", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致　Software Radio State操作：'フィルターに合致せず
            _context.Add(new LteModule { Name = "abcd", UseSoftwareRadioState = true, NwAdapterName = "abc" });//名前：'フィルターに部分一致せず　Software Radio State操作：'フィルターに合致
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?Name=lte&page=1&pageSize=2&UseSoftwareRadioState=false", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

    }
}
