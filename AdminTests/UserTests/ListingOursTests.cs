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
    /// 自分ユーザー一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ListingOursTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/users/ours";

        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly Domain _domain3;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly UserGroup _userGroup3;
        private readonly UserGroup _userGroup4;

        public ListingOursTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            Organization org2;
            Organization org1;
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = org1 });
            _context.Add(_domain3 = new Domain { Id = Guid.NewGuid(), Name = "domain03", Organization = org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup1" });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup2" });
            _context.Add(_userGroup3 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain2, Name = "userGroup3" });
            _context.Add(_userGroup4 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain3, Name = "userGroup4" });

            _context.Add(new SuperAdmin { Name = "user00", AccountName = "user00", Password = Utils.HashPassword("user00") }); // スーパー管理者
            UserAdmin au;
            _context.Add(au = new UserAdmin { Name = "user01", AccountName = "user01", Password = Utils.HashPassword("user01"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = au });
            _context.Add(new AvailablePeriod() { EndUser = au, StartDate = DateTime.Parse("2020-01-14") });

            _context.SaveChanges();
        }

        /// <summary>
        /// フィルター：不在、ページ：不在、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            GeneralUser gu2, gu3, gu4;
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain2 });
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain1 });
            _context.Add(gu4 = new GeneralUser { Name = "user04", AccountName = "user04", Domain = _domain3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup3, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup4, EndUser = gu4 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-15") });

            for (var i = 5; i <= 43; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"user{i:00}", AccountName = $"USER{i:00}", Domain = _domain1 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            //Assert.Single(array);
            //Assert.Equal("user01", array[0]["name"].ToString());
            Assert.Equal(42, array.Count);
            Assert.Equal("user01", array[0]["name"].ToString());
            Assert.Equal("user03", array[1]["name"].ToString());
            Assert.Equal("user05", array[2]["name"].ToString());
        }

        /// <summary>
        /// フィルター：不在、ページ：0、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, body, json) = Utils.Get(_client, $"{Url}?page=0", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var array = (JArray)json["results"];
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// フィルター：不在、ページ：最終ページ超過、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, body, json) = Utils.Get(_client, $"{Url}?page=2", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }

        /// <summary>
        /// フィルター：ドメイン、ページ：中間ページ、ページサイズ：存在、ソート：ドメイン名昇順
        /// </summary>
        [Fact]
        public void Case04()
        {
            GeneralUser gu2, gu3, gu4, gu5, gu6;
            // ドメインフィルターに合致しない
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain2 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-15") });

            // ドメインフィルターに合致
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain1 });
            _context.Add(gu4 = new GeneralUser { Name = "user04", AccountName = "user04", Domain = _domain1 });
            _context.Add(gu5 = new GeneralUser { Name = "user05", AccountName = "user05", Domain = _domain1 });
            _context.Add(gu6 = new GeneralUser { Name = "user06", AccountName = "user06", Domain = _domain1 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu6 });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu6, StartDate = DateTime.Parse("2020-01-15") });
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?domainId={_domain1.Id}&page=2&pageSize=2&sortBy=domainName&orderBy=asc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(5, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("user04", array[0]["name"].ToString());
            Assert.Equal("user05", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：ユーザーグループ、ページ：最終ページ、ページサイズ：不在、ソート：ドメイン名降順
        /// </summary>
        [Fact]
        public void Case05()
        {
            GeneralUser gu2, gu3;
            // ユーザーグループフィルターに合致しない
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain1 });
            // 他組織
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-15") });

            // ユーザーグループフィルターに合致
            for (var i = 4; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"user{i:00}", AccountName = $"USER{i:00}", Domain = _domain1 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?userGroupId={_userGroup1.Id}&page=3&sortBy=domainName&orderBy=desc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("user43", array[0]["name"].ToString());
            Assert.Equal("user44", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：アカウント名、ページ：初期ページ、ページサイズ：存在、ソート：アカウント名昇順
        /// </summary>
        [Fact]
        public void Case06()
        {
            GeneralUser gu2, gu3;
            // アカウント名フィルターに部分合致しない
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "accoutname02", Domain = _domain1 });
            // 他組織
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-15") });

            // アカウント名フィルターに部分合致
            //_context.Add(new GeneralUser { Name = "user03", AccountName = "X_USER03", Domain = Domain1, UserGroup = UserGroup1 });
            //_context.SaveChanges();
            for (var i = 4; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"user{i:00}", AccountName = $"x_user{i:00}", Domain = _domain1 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?accountName=user&page=1&pageSize=2&sortBy=accountName&orderBy=asc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("user01", array[0]["name"].ToString());
            Assert.Equal("user04", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：氏名、ページ：中間ページ、ページサイズ：不在、ソート：アカウント名降順
        /// </summary>
        [Fact]
        public void Case07()
        {
            GeneralUser gu2, gu3;
            // 氏名フィルターに部分合致しない
            _context.Add(gu2 = new GeneralUser { Name = "name02", AccountName = "user02", Domain = _domain1 });
            // 他組織
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-15") });

            // 氏名フィルターに部分合致
            for (var i = 4; i <= 44; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain1 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?name=user&page=2&sortBy=accountName&orderBy=desc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            Assert.Equal("x_user24", array[0]["name"].ToString());
            Assert.Equal("x_user23", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：利用開始日範囲開始、ページ：最終ページ、ページサイズ：存在、ソート：氏名昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            GeneralUser gu2, gu3, gu4, gu5;
            // 利用開始日範囲開始フィルターに部分合致しない
            _context.Add(gu2 = new GeneralUser { Name = "name02", AccountName = "user02", Domain = _domain1 });
            // 利用開始日範囲開始フィルターに部分合致しない他組織
            _context.Add(gu3 = new GeneralUser { Name = "name03", AccountName = "user03", Domain = _domain3 });
            // 利用開始日範囲開始同日設定
            _context.Add(gu4 = new GeneralUser { Name = "name04", AccountName = "user04", Domain = _domain1 });
            // 利用開始日範囲開始フィルターに合致する他組織
            _context.Add(gu5 = new GeneralUser { Name = "name05", AccountName = "user05", Domain = _domain3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-13") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-13") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-15") });

            // 利用開始日範囲開始フィルターに部分合致
            for (var i = 6; i <= 45; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain1 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?startDateFrom=2020-01-14&page=3&sortBy=name&orderBy=asc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user44", array[0]["name"].ToString());
            Assert.Equal("x_user45", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：利用開始日範囲終了、ページ：初期ページ、ページサイズ：不在、ソート：氏名降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            GeneralUser gu2, gu3, gu4, gu5;
            // 利用開始範囲終了フィルターに部分合致しない
            _context.Add(gu2 = new GeneralUser { Name = "name02", AccountName = "user02", Domain = _domain1 });
            // 利用開始範囲終了フィルターに部分合致しない他組織
            _context.Add(gu3 = new GeneralUser { Name = "name03", AccountName = "user03", Domain = _domain3 });
            // 利用開始範囲終了同日を設定
            _context.Add(gu4 = new GeneralUser { Name = "name04", AccountName = "user04", Domain = _domain1 });
            // 利用開始範囲終了フィルターに合致する他組織
            _context.Add(gu5 = new GeneralUser { Name = "name05", AccountName = "user05", Domain = _domain3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-13") });

            // 利用開始範囲終了フィルターに部分合致
            for (var i = 6; i <= 45; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain1 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-13") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?startDateTo=2020-01-14&page=1&sortBy=name&orderBy=desc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(20, array.Count);
            Assert.Equal("x_user45", array[0]["name"].ToString());
            Assert.Equal("x_user44", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：利用終了日範囲開始、ページ：中間ページ、ページサイズ：存在、ソート：利用開始日昇順
        /// </summary>
        [Fact]
        public void Case10()
        {
            GeneralUser gu2, gu3, gu4, gu5;
            // 利用終了日範囲開始フィルターに部分合致しない
            _context.Add(gu2 = new GeneralUser { Name = "name02", AccountName = "user02", Domain = _domain1 });
            // 利用終了日範囲開始フィルターに部分合致しない他組織
            _context.Add(gu3 = new GeneralUser { Name = "name03", AccountName = "user03", Domain = _domain3 });
            // 利用終了日範囲開始同日
            _context.Add(gu4 = new GeneralUser { Name = "name04", AccountName = "user04", Domain = _domain1 });
            // 利用終了日範囲開始フィルターに合致する他組織
            _context.Add(gu5 = new GeneralUser { Name = "name05", AccountName = "user05", Domain = _domain3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-13"), EndDate = DateTime.Parse("2021-01-13") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-13"), EndDate = DateTime.Parse("2021-01-13") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-15") });

            // 利用終了日範囲開始フィルターに合致
            for (var i = 6; i <= 45; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain1 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15"), EndDate = DateTime.Parse("2021-01-15") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?endDateFrom=2021-01-14&page=2&pageSize=2&sortBy=startDate&orderBy=asc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(41, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user07", array[0]["name"].ToString());
            Assert.Equal("x_user08", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：利用終了日範囲終了、ページ：最終ページ、ページサイズ：不在、ソート：利用開始日降順
        /// </summary>
        [Fact]
        public void Case11()
        {
            GeneralUser gu2, gu3, gu4, gu5;
            // 利用終了日範囲終了フィルターに合致しない
            _context.Add(gu2 = new GeneralUser { Name = "name02", AccountName = "user02", Domain = _domain1 });
            // 利用終了日範囲終了フィルターに合致しない他組織
            _context.Add(gu3 = new GeneralUser { Name = "name03", AccountName = "user03", Domain = _domain3 });
            // 利用終了日範囲終了同日
            _context.Add(gu4 = new GeneralUser { Name = "name04", AccountName = "user04", Domain = _domain1 });
            // 利用終了日範囲終了フィルターに合致する他組織
            _context.Add(gu5 = new GeneralUser { Name = "name05", AccountName = "user05", Domain = _domain3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-13"), EndDate = DateTime.Parse("2021-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-13"), EndDate = DateTime.Parse("2021-01-15") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });

            // 氏名フィルターに部分合致
            for (var i = 6; i <= 46; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"USER{i:00}", Domain = _domain1 });
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-15"), EndDate = DateTime.Parse("2021-01-13") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?endDateTo=2021-01-14&page=3&sortBy=startDate&orderBy=desc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user46", array[0]["name"].ToString());
            Assert.Equal("name04", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：全部、ページ：最終ページ、ページサイズ：不在、ソート：利用終了日昇順
        /// </summary>
        [Fact]
        public void Case12()
        {
            GeneralUser gu2, gu3, gu4, gu5, gu6, gu7, gu8, gu9;
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain2 }); // ドメイン不一致
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain1 }); // ユーザーグループ不一致
            _context.Add(gu4 = new GeneralUser { Name = "user04", AccountName = "account04", Domain = _domain1 }); // アカウント名部分不一致
            _context.Add(gu5 = new GeneralUser { Name = "name05", AccountName = "user05", Domain = _domain1 }); // 氏名部分不一致
            _context.Add(gu6 = new GeneralUser { Name = "name06", AccountName = "user06", Domain = _domain1 }); // 利用開始日範囲開始不一致
            _context.Add(gu7 = new GeneralUser { Name = "name07", AccountName = "user07", Domain = _domain1 }); // 利用開始日範囲終了不一致
            _context.Add(gu8 = new GeneralUser { Name = "name08", AccountName = "user08", Domain = _domain1 }); // 利用終了日範囲開始不一致
            _context.Add(gu9 = new GeneralUser { Name = "name09", AccountName = "user09", Domain = _domain1 }); // 利用終了日範囲終了不一致
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu6 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu7 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu8 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu9 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu6, StartDate = DateTime.Parse("2020-01-12"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu7, StartDate = DateTime.Parse("2020-01-16"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu8, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu9, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-16") });
            for (var i = 10; i <= 30; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain1 }); // 全部一致
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            }
            for (var i = 31; i <= 51; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain1 }); // 全部一致
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-13") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?domainId={_domain1.Id}&userGroupId={_userGroup1.Id}&accountName=USER&name=user&startDateFrom=2020-01-13&startDateTo=2020-01-15&endDateFrom=2021-01-13&endDateTo=2021-01-15&page=3&sortBy=endDate&orderBy=asc", "user01", "user01", 1, "domain01"); // ユーザー管理者
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
        public void Case13()
        {
            GeneralUser gu2, gu3, gu4, gu5, gu6, gu7, gu8, gu9;
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain2 }); // ドメイン不一致
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain1 }); // ユーザーグループ不一致
            _context.Add(gu4 = new GeneralUser { Name = "user04", AccountName = "account04", Domain = _domain1 }); // アカウント名部分不一致
            _context.Add(gu5 = new GeneralUser { Name = "name05", AccountName = "user05", Domain = _domain1 }); // 氏名部分不一致
            _context.Add(gu6 = new GeneralUser { Name = "name06", AccountName = "user06", Domain = _domain1 }); // 利用開始日範囲開始不一致
            _context.Add(gu7 = new GeneralUser { Name = "name07", AccountName = "user07", Domain = _domain1 }); // 利用開始日範囲終了不一致
            _context.Add(gu8 = new GeneralUser { Name = "name08", AccountName = "user08", Domain = _domain1 }); // 利用終了日範囲開始不一致
            _context.Add(gu9 = new GeneralUser { Name = "name09", AccountName = "user09", Domain = _domain1 }); // 利用終了日範囲終了不一致
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu6 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu7 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu8 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu9 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu6, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu7, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu8, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu9, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-16") });
            for (var i = 10; i <= 30; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain1 }); // 全部一致
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            }
            for (var i = 31; i <= 51; i++)
            {
                GeneralUser gu;
                _context.Add(gu = new GeneralUser { Name = $"x_user{i:00}", AccountName = $"x_USER{i:00}", Domain = _domain1 }); // 全部一致
                _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu });
                _context.Add(new AvailablePeriod() { EndUser = gu, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-13") });
            }
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}?domainId={_domain1.Id}&userGroupId={_userGroup1.Id}&accountName=USER&name=user&startDateFrom=2020-01-13&startDateTo=2020-01-15&endDateFrom=2021-01-13&endDateTo=2021-01-15&page=1&pageSize=2&sortBy=endDate&orderBy=desc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(42, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Equal(2, array.Count);
            Assert.Equal("x_user10", array[0]["name"].ToString());
            Assert.Equal("x_user11", array[1]["name"].ToString());
        }

        /// <summary>
        /// フィルター：全部、ページ：初期ページ
        /// </summary>
        [Fact]
        public void Case14()
        {
            GeneralUser gu2, gu3, gu4, gu5, gu6, gu7, gu8, gu9;
            _context.Add(gu2 = new GeneralUser { Name = "user02", AccountName = "user02", Domain = _domain2 }); // ドメイン不一致
            _context.Add(gu3 = new GeneralUser { Name = "user03", AccountName = "user03", Domain = _domain1 }); // ユーザーグループ不一致
            _context.Add(gu4 = new GeneralUser { Name = "user04", AccountName = "account04", Domain = _domain1 }); // アカウント名部分不一致
            _context.Add(gu5 = new GeneralUser { Name = "name05", AccountName = "user05", Domain = _domain1 }); // 氏名部分不一致
            _context.Add(gu6 = new GeneralUser { Name = "name06", AccountName = "user06", Domain = _domain1 }); // 利用開始日範囲開始不一致
            _context.Add(gu7 = new GeneralUser { Name = "name07", AccountName = "user07", Domain = _domain1 }); // 利用開始日範囲終了不一致
            _context.Add(gu8 = new GeneralUser { Name = "name08", AccountName = "user08", Domain = _domain1 }); // 利用終了日範囲開始不一致
            _context.Add(gu9 = new GeneralUser { Name = "name09", AccountName = "user09", Domain = _domain1 }); // 利用終了日範囲終了不一致
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu2 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup2, EndUser = gu3 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu4 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu5 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu6 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu7 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu8 });
            _context.Add(new UserGroupEndUser() { UserGroup = _userGroup1, EndUser = gu9 });
            _context.Add(new AvailablePeriod() { EndUser = gu2, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu3, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu4, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu5, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu6, StartDate = DateTime.Parse("2020-01-12"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu7, StartDate = DateTime.Parse("2020-01-16"), EndDate = DateTime.Parse("2021-01-14") });
            _context.Add(new AvailablePeriod() { EndUser = gu8, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-12") });
            _context.Add(new AvailablePeriod() { EndUser = gu9, StartDate = DateTime.Parse("2020-01-14"), EndDate = DateTime.Parse("2021-01-16") });

            //var (response, body, json) = Utils.Get(_client, $"{Url}?domainId={Domain1.Id}&userGroupId={UserGroup1.Id}&accountName=USER&name=name&pageSize=2&sortBy=name&orderBy=desc", "user01", "user01", 1, "domain01"); // ユーザー管理者
            var (response, body, json) = Utils.Get(_client, $"{Url}?domainId={_domain1.Id}&userGroupId={_userGroup1.Id}&accountName=USER&name=name&startDateFrom=2020-01-13&startDateTo=2020-01-15&endDateFrom=2021-01-13&endDateTo=2021-01-15&page=1", "user01", "user01", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var array = (JArray)json["results"];
            Assert.Empty(array);
        }
    }
}
