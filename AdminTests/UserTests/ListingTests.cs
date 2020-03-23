using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.UserTests
{
    /// <summary>
    /// ユーザー一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ListingTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/users";

        private readonly Domain _domain11;
        private readonly Domain _domain12;
        private readonly Domain _domain21;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;

        public ListingTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            Organization org2;
            Organization org1;
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain11 = new Domain { Id = Guid.NewGuid(), Name = "domain11", Organization = org1 });
            _context.Add(_domain12 = new Domain { Id = Guid.NewGuid(), Name = "domain12", Organization = org1 });
            _context.Add(_domain21 = new Domain { Id = Guid.NewGuid(), Name = "domain21", Organization = org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain11, Name = "userGroup1" });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain11, Name = "userGroup2" });
            _context.Add(new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            UserAdmin au;
            _context.Add(au = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain21}); // ユーザー管理者
            _context.Add(new AvailablePeriod() { EndUser = au, StartDate = DateTime.Parse("2020-01-14") });
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, body, _) = Utils.Get(_client, Url, "user1", "user1", 2, "domain21"); // ユーザー管理者
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
        /// コード：数字以外
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=a", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var array = (JArray)json["results"];
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["OrganizationCode"]);
        }

        /// <summary>
        /// DBに組織不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=5", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// フィルター：不在、ページ：不在、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            GeneralUser gu21, gu11, gu12, gu13, gu14;
            // Arrange
            _context.Add(gu21 = new GeneralUser { Name = "user21", AccountName = "user21", Domain = _domain21 }); // 他組織
            _context.Add(gu11 = new GeneralUser { Name = "user11", AccountName = "user11", Domain = _domain11 }); // 自組織
            _context.Add(gu12 = new GeneralUser { Name = "user12", AccountName = "user12", Domain = _domain11 }); // 自組織
            _context.Add(gu13 = new GeneralUser { Name = "user13", AccountName = "user13", Domain = _domain12 }); // 自組織
            _context.Add(gu14 = new GeneralUser { Name = "user14", AccountName = "user14", Domain = _domain12 }); // 自組織
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu21 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu11 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu12 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu13 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu14 });
            _context.Add(new AvailablePeriod() { EndUser = gu21, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu11, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu12, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu13, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu14, StartDate = DateTime.Parse("2020-01-14") });

            _context.SaveChanges();

            // Run
            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(4, array.Count);
            Assert.Equal("user11", array[0]["name"].ToString());
            Assert.Equal("user12", array[1]["name"].ToString());
            Assert.Equal("user13", array[2]["name"].ToString());
            Assert.Equal("user14", array[3]["name"].ToString());
        }

        /// <summary>
        /// フィルター：不在、ページ：0、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case06()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&page=0", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var array = (JArray)json["results"];
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// フィルター：不在、ページ：最終ページ超過、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case07()
        {
            // Arrange
            for (var i = 1; i <= 42; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"user{i:00}", AccountName = $"USER{i:00}", Domain = _domain11 }); // user01～42
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&page=4", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// フィルター：ドメイン、ページ：中間ページ、ページサイズ：存在、ソート：ドメイン名昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            GeneralUser gu1, gu2, gu3, gu4, gu5;
            // ドメインフィルターに合致しない
            _context.Add(gu1 = new GeneralUser { Name = "user01", AccountName = "user01", Domain = _domain12 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-14") });

            // ドメインフィルターに合致
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain11 });
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain11 });
            _context.Add(gu4 = new GeneralUser { Name = "user04", AccountName = "user04", Domain = _domain11 });
            _context.Add(gu5 = new GeneralUser { Name = "user05", AccountName = "user05", Domain = _domain11 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-14") });
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&domainId={_domain11.Id}&page=2&pageSize=2&sortBy=domainName&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("user04", array[0]["name"].ToString());
            Assert.Equal("user05", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：ユーザーグループ、ページ：最終ページ、ページサイズ：不在、ソート：ドメイン名降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            GeneralUser gu1, gu2;
            // ユーザーグループフィルターに合致しない
            _context.Add(gu1 = new GeneralUser { Name = "user01", AccountName = "user01", Domain = _domain11 });
            // 他組織
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain21 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });

            // ユーザーグループフィルターに合致
            for (var i = 3; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"user{i:00}", AccountName = $"USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&userGroupId={_userGroup1.Id}&page=3&sortBy=domainName&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("user43", array[0]["name"].ToString());
            Assert.Equal("user44", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：アカウント名、ページ：1ページ、ページサイズ：存在、ソート：アカウント名昇順
        /// </summary>
        [Fact]
        public void Case10()
        {
            GeneralUser gu1, gu2;
            // アカウント名フィルターに部分合致しない
            _context.Add(gu1 = new GeneralUser { Name = "user01", AccountName = "accoutname01", Domain = _domain11 });
            // 他組織
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain21 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });

            // アカウント名フィルターに部分合致
            for (var i = 3; i <= 5; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"user{i:00}", AccountName = $"X_USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&accountName=USER&page=1&pageSize=4&sortBy=accountName&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(3, array.Count);
            Assert.Equal("user03", array[0]["name"].ToString());
            Assert.Equal("user04", array[1]["name"].ToString());
            Assert.Equal("user05", array[2]["name"].ToString());
        }

        /// <summary>
        /// フィルター：氏名、ページ：中間ページ、ページサイズ：不在、ソート：アカウント名降順
        /// </summary>
        [Fact]
        public void Case11()
        {
            GeneralUser gu1, gu2;
            // 氏名フィルターに部分合致しない
            _context.Add(gu1 = new GeneralUser { Name = "name01", AccountName = "user01", Domain = _domain11 });
            // 他組織
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain21 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });

            // 氏名フィルターに部分合致
            for (var i = 3; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&name=user&page=2&sortBy=accountName&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            Assert.Equal("x_user24", array[0]["name"].ToString());
            Assert.Equal("x_user23", array[1]["name"].ToString()); // アカウント名降順の2ページ目
        }

        /// <summary>
        /// フィルター：利用開始日範囲開始、ページ：最終ページ、ページサイズ：存在、ソート：氏名昇順
        /// </summary>
        [Fact]
        public void Case12()
        {
            GeneralUser gu1, gu2, gu3;
            // 利用開始日範囲開始フィルターに合致しない
            _context.Add(gu1 = new GeneralUser { Name = "name01", AccountName = "user01", Domain = _domain11 });
            // 他組織
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain21 });
            // 利用開始日範囲開始同日
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain11 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-13") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14") });

            // 利用開始日範囲開始フィルターに合致
            for (var i = 4; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&startDateFrom=2020-01-14&page=21&pageSize=2&sortBy=name&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user43", array[0]["name"].ToString());
            Assert.Equal("x_user44", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：利用開始日範囲終了、ページ：1ページ、ページサイズ：不在、ソート：氏名降順
        /// </summary>
        [Fact]
        public void Case13()
        {
            GeneralUser gu1, gu2, gu3;
            // 利用開始日範囲終了フィルターに合致しない
            _context.Add(gu1 = new GeneralUser { Name = "name01", AccountName = "user01", Domain = _domain11 });
            // 他組織
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain21 });
            // 利用開始日範囲終了同日
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain11 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14") });

            // 利用開始日範囲終了フィルターに合致
            for (var i = 4; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-13") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&startDateTo=2020-01-14&page=1&sortBy=name&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            Assert.Equal("x_user44", array[0]["name"].ToString());
            Assert.Equal("x_user43", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：利用終了日範囲開始、ページ：中間ページ、ページサイズ：存在、ソート：利用開始日昇順
        /// </summary>
        [Fact]
        public void Case14()
        {
            GeneralUser gu1, gu2, gu3;
            // 利用終了日範囲開始フィルターに合致しない
            _context.Add(gu1 = new GeneralUser { Name = "name01", AccountName = "user01", Domain = _domain11 });
            // 他組織
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain21 });
            // 利用終了日範囲開始同日
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain11 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-13"), EndDate = DateTime.Parse("2021-01-13") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });

            // 利用終了日範囲開始フィルターに合致
            for (var i = 4; i <= 24; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15"), EndDate = DateTime.Parse("2021-01-15") });
            }

            for (var i = 25; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-13"), EndDate = DateTime.Parse("2021-01-15") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&endDateFrom=2021-01-14&page=2&pageSize=2&sortBy=startDate&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user27", array[0]["name"].ToString());
            Assert.Equal("x_user28", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：利用開始日範囲開始、ページ：最終ページ、ページサイズ：不在、ソート：利用開始日降順
        /// </summary>
        [Fact]
        public void Case15()
        {
            GeneralUser gu1, gu2, gu3;
            // 利用開始日範囲開始フィルターに合致しない
            _context.Add(gu1 = new GeneralUser { Name = "name01", AccountName = "user01", Domain = _domain11 });
            // 他組織
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain21 });
            // 利用開始日範囲開始同日
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain11 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-13"), EndDate = DateTime.Parse("2021-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });

            // 利用開始日範囲開始フィルターに合致
            for (var i = 4; i <= 22; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-13"), EndDate = DateTime.Parse("2021-01-13") });
            }

            for (var i = 23; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain11 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15"), EndDate = DateTime.Parse("2021-01-13") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=1&endDateTo=2021-01-14&page=3&sortBy=startDate&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user21", array[0]["name"].ToString());
            Assert.Equal("x_user22", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：全部、ページ：最終ページ、ページサイズ：不在、ソート：利用終了日昇順
        /// </summary>
        [Fact]
        public void Case16()
        {
            GeneralUser gu1, gu2, gu3, gu4, gu5, gu6, gu7, gu8;
            _context.Add(gu1 = new GeneralUser { Name = "user01", AccountName = "user01", Domain = _domain12 }); // ドメイン不一致
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain11 }); // ユーザーグループ不一致
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "account03", Domain = _domain11 }); // アカウント名不一致
            _context.Add(gu4 = new GeneralUser { Name = "name04", AccountName = "user04", Domain = _domain11 }); // 氏名不一致
            _context.Add(gu5 = new GeneralUser { Name = "user05", AccountName = "user05", Domain = _domain11 }); // 利用開始日範囲開始不一致
            _context.Add(gu6 = new GeneralUser { Name = "user06", AccountName = "user06", Domain = _domain11 }); // 利用開始日範囲終了不一致
            _context.Add(gu7 = new GeneralUser { Name = "user07", AccountName = "user07", Domain = _domain11 }); // 利用終了日範囲開始不一致
            _context.Add(gu8 = new GeneralUser { Name = "user08", AccountName = "user08", Domain = _domain11 }); // 利用終了日範囲終了不一致
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu6 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu7 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu8 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu6, StartDate = DateTime.Parse("2020-01-16") });
            _context.Add(new AvailablePeriod() { EndUser = gu7, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu8, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-16") });
            for (var i = 9; i <= 30; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain11 }); // 全部一致
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            }
            for (var i = 31; i <= 50; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain11 }); // 全部一致
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-13") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&domainId={_domain11.Id}&userGroupId={_userGroup1.Id}&accountName=USER&name=user&startDateFrom=2020-01-13&startDateTo=2020-01-15&endDateFrom=2021-01-13&endDateTo=2021-01-15&page=3&sortBy=endDate&orderBy=asc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user29", array[0]["name"].ToString());
            Assert.Equal("x_user30", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：全部、ページ：初期ページ、ページサイズ：存在、ソート：利用終了日降順
        /// </summary>
        [Fact]
        public void Case17()
        {
            GeneralUser gu1, gu2, gu3, gu4, gu5, gu6, gu7, gu8;
            _context.Add(gu1 = new GeneralUser { Name = "user01", AccountName = "user01", Domain = _domain12 }); // ドメイン不一致
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain11 }); // ユーザーグループ不一致
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "account03", Domain = _domain11 }); // アカウント名不一致
            _context.Add(gu4 = new GeneralUser { Name = "name04", AccountName = "user04", Domain = _domain11 }); // 氏名不一致
            _context.Add(gu5 = new GeneralUser { Name = "user05", AccountName = "user05", Domain = _domain11 }); // 利用開始日範囲開始不一致
            _context.Add(gu6 = new GeneralUser { Name = "user06", AccountName = "user06", Domain = _domain11 }); // 利用開始日範囲終了不一致
            _context.Add(gu7 = new GeneralUser { Name = "user07", AccountName = "user07", Domain = _domain11 }); // 利用終了日範囲開始不一致
            _context.Add(gu8 = new GeneralUser { Name = "user08", AccountName = "user08", Domain = _domain11 }); // 利用終了日範囲終了不一致
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu6 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu7 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu8 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu6, StartDate = DateTime.Parse("2020-01-16") });
            _context.Add(new AvailablePeriod() { EndUser = gu7, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu8, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-16") });
            for (var i = 9; i <= 30; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain11 }); // 全部一致
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            }
            for (var i = 31; i <= 50; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain11 }); // 全部一致
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-13") });
            }
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&domainId={_domain11.Id}&userGroupId={_userGroup1.Id}&accountName=USER&name=user&startDateFrom=2020-01-13&startDateTo=2020-01-15&endDateFrom=2021-01-13&endDateTo=2021-01-15&page=1&pageSize=2&sortBy=endDate&orderBy=desc", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user09", array[0]["name"].ToString());
            Assert.Equal("x_user10", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：全部、ページ：不在
        /// </summary>
        [Fact]
        public void Case18()
        {
            GeneralUser gu1, gu2, gu3, gu4, gu5, gu6, gu7, gu8;
            _context.Add(gu1 = new GeneralUser { Name = "user01", AccountName = "user01", Domain = _domain12 }); // ドメイン不一致
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain11 }); // ユーザーグループ不一致
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "account03", Domain = _domain11 }); // アカウント名不一致
            _context.Add(gu4 = new GeneralUser { Name = "name04", AccountName = "user04", Domain = _domain11 }); // 氏名不一致
            _context.Add(gu5 = new GeneralUser { Name = "user05", AccountName = "user05", Domain = _domain11 }); // 利用開始日範囲開始不一致
            _context.Add(gu6 = new GeneralUser { Name = "user06", AccountName = "user06", Domain = _domain11 }); // 利用開始日範囲終了不一致
            _context.Add(gu7 = new GeneralUser { Name = "user07", AccountName = "user07", Domain = _domain11 }); // 利用終了日範囲開始不一致
            _context.Add(gu8 = new GeneralUser { Name = "user08", AccountName = "user08", Domain = _domain11 }); // 利用終了日範囲終了不一致
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu6 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu7 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu8 });
            _context.Add(new AvailablePeriod() { EndUser = gu1, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu6, StartDate = DateTime.Parse("2020-01-16") });
            _context.Add(new AvailablePeriod() { EndUser = gu7, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu8, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-16") });
            _context.SaveChanges();

            var (response, _, json) = Utils.Get(_client, $"{Url}?organizationCode=1&domainId={_domain11.Id}&userGroupId={_userGroup1.Id}&accountName=USER&name=user&startDateFrom=2020-01-13&startDateTo=2020-01-15&endDateFrom=2021-01-13&endDateTo=2021-01-15&page=1", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }
    }
}
