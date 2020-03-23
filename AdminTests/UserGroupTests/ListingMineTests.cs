using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.UserGroupTests
{
    /// <summary>
    /// 自分ユーザーグループ一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ListingMineTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/user-groups/mine";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain1A;
        private readonly Domain _domain2;

        public ListingMineTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _context.SaveChanges();
            //_context.Organization.RemoveRange(_context.Organization);
            //_context.Domain.RemoveRange(_context.Domain);
            //_context.UserGroup.RemoveRange(_context.UserGroup);
            //_context.User.RemoveRange(_context.User);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain1A = new Domain { Id = Guid.NewGuid(), Name = "domain01a", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.SaveChanges();
        }

        /// <summary>
        /// フィルタ：不在、ページ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            _context.Add(new UserGroup { Name = "userGroup01", Domain = _domain2 }); // 他組織
            for (var i = 2; i <= 43; i++) _context.Add(new UserGroup { Name = $"userGroup{i:00}", Domain = _domain1 }); // 自組織
            _context.SaveChanges();

            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(body);
            var array = (JArray)json["results"];
            Assert.Equal(42, array.Count);
            Assert.Equal("userGroup02", array[0]["name"].ToString());
            Assert.Equal("userGroup03", array[1]["name"].ToString()); // ドメイン名昇
        }

        /// <summary>
        /// フィルタ：不在、ページ：0、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?page=0", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var json = JObject.Parse(body);
            var array = (JArray)json["results"];
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// フィルタ：不在、ページ：最終ページ超過、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?page=4", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(body);
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// フィルタ：ドメイン、ページ：1、ページサイズ：存在、ソート：ドメイン名昇順
        /// </summary>
        [Fact]
        public void Case04()
        {
            _context.Add(new UserGroup { Name = "userGroup01", Domain = _domain1 }); // ドメインフィルターに合致せず
            for (var i = 2; i <= 43; i++) _context.Add(new UserGroup { Name = $"userGroup{i:00}", Domain = _domain1A }); // ドメインフィルターに合致
            _context.SaveChanges();

            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?domainId={_domain1A.Id}&page=1&pageSize=2&sortBy=domainName&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(body);
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("userGroup02", array[0]["name"].ToString());
            Assert.Equal("userGroup03", array[1]["name"].ToString()); // ドメイン名昇順
        }

        /// <summary>
        /// フィルタ：名前、ページ：中間ページ、ページサイズ：不在、ソート：ドメイン名降順
        /// </summary>
        [Fact]
        public void Case05()
        {
            _context.Add(new UserGroup { Name = "userGroup01", Domain = _domain2 }); // 他組織
            _context.Add(new UserGroup { Name = "uGroup02", Domain = _domain1 }); // 名前フィルターに合致しない
            for (var i = 3; i <= 43; i++) _context.Add(new UserGroup { Name = $"userGroup{i:00}", Domain = _domain1 }); // 自組織
            _context.SaveChanges();

            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?name=userGroup&page=2&sortBy=domainName&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(body);
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            Assert.Equal("userGroup23", array[0]["name"].ToString());
            Assert.Equal("userGroup24", array[1]["name"].ToString()); // ドメイン名降順
        }

        /// <summary>
        /// フィルタ：ドメイン・名前、ページ：最終ページ、ページサイズ：不在、ソート：ユーザーグループ名昇順
        /// </summary>
        [Fact]
        public void Case06()
        {
            _context.Add(new UserGroup { Name = "userGroup01", Domain = _domain1 }); // ドメインフィルターに合致せず
            _context.Add(new UserGroup { Name = "uGroup02", Domain = _domain1A }); // 名前フィルターに合致せず
            _context.Add(new UserGroup { Name = "userGroup03", Domain = _domain2 }); // 名前フィルターに合致するが他組織
            for (var i = 4; i <= 45; i++) _context.Add(new UserGroup { Name = $"userGroup{i:00}", Domain = _domain1A }); // ドメインフィルターに合致
            _context.SaveChanges();

            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?domainId={_domain1A.Id}&name=userGroup&page=3&sortBy=name&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(body);
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("userGroup44", array[0]["name"].ToString());
            Assert.Equal("userGroup45", array[1]["name"].ToString()); // ドメイン名昇順
        }

        /// <summary>
        /// フィルタ：不在、ページ：中間ページ、ページサイズ：存在、ソート：ユーザーグループ名降順
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(new UserGroup { Name = "userGroup01", Domain = _domain2 }); // 他組織
            for (var i = 2; i <= 43; i++) _context.Add(new UserGroup { Name = $"userGroup{i:00}", Domain = _domain1 }); // 自組織
            _context.SaveChanges();

            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?page=2&pageSize=2&sortBy=name&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(body);
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("userGroup41", array[0]["name"].ToString());
            Assert.Equal("userGroup40", array[1]["name"].ToString()); // ユーザーグループ名降順
        }

        /// <summary>
        /// フィルタ：ドメイン・名前、ページ：初期ページ、ページサイズ：不在、ソート：ユーザーグループ名昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            for (var i = 1; i <= 21; i++) _context.Add(new UserGroup { Name = $"userGroup{i:00}", Domain = _domain1 }); // 名前フィルターに合致するが、ドメインフィルターに合致しない
            _context.SaveChanges();
            for (var i = 22; i <= 43; i++) _context.Add(new UserGroup { Name = $"uGroup{i:00}", Domain = _domain1A }); // 名前フィルターに合致しないが、ドメインフィルターに合致
            _context.SaveChanges();

            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?domainId={_domain1A.Id}&name=userGroup&page=1&sortBy=name&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var json = JObject.Parse(body);
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }
    }
}
