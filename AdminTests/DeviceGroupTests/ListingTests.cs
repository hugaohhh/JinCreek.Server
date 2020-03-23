using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.DeviceGroupTests
{
    /// <summary>
    /// 端末グループ一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ListingTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/device-groups";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain1A;
        private readonly Domain _domain2;
        private readonly Domain _domain3;

        public ListingTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain1A = new Domain { Id = Guid.NewGuid(), Name = "domain01a", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_domain3 = new Domain { Id = Guid.NewGuid(), Name = "domain03", Organization = _org1 });
            _context.Add(new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.SaveChanges();
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
        /// 組織コード：不在、フィルタ：不在、ページ：不在、ソート：不在
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
        /// 組織コード：数字以外、フィルタ：不在、ページ：不在、ソート：不在
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
        /// 組織：不在、フィルタ：不在、ページ：不在、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=2", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// フィルタ：不在、ページ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            _context.Add(new DeviceGroup { Name = "deviceGroup01", Domain = _domain1 }); // 自組織
            _context.Add(new DeviceGroup { Name = "deviceGroup02", Domain = _domain2 }); // 他組織
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Single(array);
            Assert.Equal("deviceGroup01", array[0]["name"].ToString());
        }

        /// <summary>
        /// フィルタ：不在、ページ：0、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case06()
        {
            _context.Add(new DeviceGroup { Name = "deviceGroup01", Domain = _domain1 }); // 自組織
            _context.Add(new DeviceGroup { Name = "deviceGroup02", Domain = _domain2 }); // 他組織
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=0", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var array = (JArray)json["results"];
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// フィルタ：不在、ページ：最終ページ超過、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case07()
        {
            _context.Add(new DeviceGroup { Name = "deviceGroup01", Domain = _domain1 }); // 自組織
            _context.Add(new DeviceGroup { Name = "deviceGroup02", Domain = _domain2 }); // 他組織
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=2", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// フィルタ：ドメイン、ページ：1、ページサイズ：存在、ソート：ドメイン名昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            _context.Add(new DeviceGroup { Name = "deviceGroup01", Domain = _domain1 }); // ドメインフィルターに合致せず
            _context.Add(new DeviceGroup { Name = "deviceGroup02", Domain = _domain1A }); // ドメインフィルターに合致
            _context.Add(new DeviceGroup { Name = "deviceGroup03", Domain = _domain1A }); // ドメインフィルターに合致
            _context.Add(new DeviceGroup { Name = "deviceGroup04", Domain = _domain2 }); // 他組織
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&domainId={_domain1A.Id}&page=1&pageSize=2&sortBy=domainName&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("deviceGroup02", array[0]["name"].ToString());
            Assert.Equal("deviceGroup03", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルタ：名前、ページ：中間ページ、ページサイズ：不在、ソート：ドメイン名降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            _context.Add(new DeviceGroup { Name = "deviceGroup01", Domain = _domain2 }); // 他組織
            _context.Add(new DeviceGroup { Name = "dGroup02", Domain = _domain1 }); // 名前フィルターに合致しない
            _context.Add(new DeviceGroup { Name = "deviceGroup03", Domain = _domain3 }); // 自組織ドメインソート確認
            for (var i = 3; i <= 43; i++) _context.Add(new DeviceGroup { Name = $"X_deviceGroup{i:00}", Domain = _domain1 }); // 自組織
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?name=deviceGroup&organizationCode=1&page=2&sortBy=domainName&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            Assert.Equal("X_deviceGroup22", array[0]["name"].ToString());
            Assert.Equal("X_deviceGroup23", array[1]["name"].ToString()); // ドメイン名降順
        }

        /// <summary>
        /// フィルタ：ドメイン・名前、ページ：最終ページ、ページサイズ：不在、ソート：端末グループ名昇順
        /// </summary>
        [Fact]
        public void Case10()
        {
            _context.Add(new DeviceGroup { Name = "deviceGroup01", Domain = _domain1 }); // ドメインフィルターに合致せず
            _context.Add(new DeviceGroup { Name = "dGroup02", Domain = _domain1A }); // 名前フィルターに合致せず
            for (var i = 3; i <= 44; i++) _context.Add(new DeviceGroup { Name = $"deviceGroup{i:00}", Domain = _domain1A }); // ドメインフィルターに合致
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&domainId={_domain1A.Id}&name=deviceGroup&page=3&sortBy=name&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("deviceGroup43", array[0]["name"].ToString());
            Assert.Equal("deviceGroup44", array[1]["name"].ToString()); // ドメイン名昇順
        }

        /// <summary>
        /// フィルタ：不在、ページ：中間ページ、ページサイズ：存在、ソート：端末グループ名降順
        /// </summary>
        [Fact]
        public void Case11()
        {
            _context.Add(new DeviceGroup { Name = "deviceGroup01", Domain = _domain2 }); // 他組織
            for (var i = 2; i <= 43; i++) _context.Add(new DeviceGroup { Name = $"deviceGroup{i:00}", Domain = _domain1 }); // 自組織
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&page=2&pageSize=2&sortBy=name&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("deviceGroup41", array[0]["name"].ToString());
            Assert.Equal("deviceGroup40", array[1]["name"].ToString()); // 端末グループ名降順
        }

        /// <summary>
        /// フィルタ：ドメイン・名前、ページ：１、ページサイズ：不在、ソート：ユーザーグループ名昇順
        /// </summary>
        [Fact]
        public void Case12()
        {
            _context.Add(new DeviceGroup { Name = "deviceGroup01", Domain = _domain1 }); // 自組織
            _context.Add(new DeviceGroup { Name = "dGroup02", Domain = _domain1A }); // 自組織

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&domainId={_domain1A.Id}&name=deviceGroup&page=1&sortBy=name&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }
    }
}
