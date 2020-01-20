using System;
using System.Net;
using System.Net.Http;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AdminTests.IntegrationTests.DomainTests
{
    /// <summary>
    /// ドメイン一覧照会
    /// </summary>
    [Collection("Sequential")]
    public class ListInquiryTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private const string Url = "/api/domains";

        public ListInquiryTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();

            // Arrange
            var org1 = new JinCreek.Server.Common.Models.Organization { Code = 1, Name = "org1", StartDay = DateTime.Parse("2020-01-14"), EndDay = DateTime.Parse("2021-01-14"), IsValid = true };
            var org2 = new JinCreek.Server.Common.Models.Organization { Code = 2, Name = "org2", StartDay = DateTime.Parse("2020-01-14"), EndDay = DateTime.Parse("2021-01-14"), IsValid = true };
            var domain1 = new Domain { Id = Guid.Parse("93b25287-7516-4051-9c8e-d114fad099ab"), DomainName = "domain0", Organization = org1 };
            var domain2 = new Domain { Id = Guid.Parse("1d0beb2a-15ad-452d-9ecd-e6caf66fa501"), DomainName = "domain1", Organization = org2 };

            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            context.Domain.RemoveRange(context.Domain);
            context.User.RemoveRange(context.User);
            context.Organization.RemoveRange(context.Organization);
            context.SaveChanges();
            context.Domain.Add(domain1);
            context.Domain.Add(domain2);
            context.User.Add(new SuperAdminUser { AccountName = "USER0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            context.User.Add(new AdminUser { AccountName = "USER1", Password = Utils.HashPassword("user1"), Domain = domain1 }); // ユーザー管理者
            context.Organization.Add(org1);
            context.Organization.Add(org2);
            context.SaveChanges();
        }

        /// <summary>
        /// 異常：ユーザー管理者が組織コードを指定する
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=2", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// 正常：ユーザー管理者が組織コードを指定しない
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }


        /// <summary>
        /// 異常：スーパー管理者が組織コードを指定してない
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// 異常：スーパー管理者が組織コードに数字以外を指定している
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=a", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            // TODO: impl bellow
        }


        /// <summary>
        /// 異常：DBに組織がない場合
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// 正常：DBに組織がある場合
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }


        /// <summary>
        /// 正常：ページ数が不正（0以下）・1ページ当たり表示件数あり
        /// </summary>
        [Fact]
        public void Case07()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1&page=0&pageSize=10", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// 正常：ページ数が中間ページ・1ページ当たり表示件数なし・ドメイン名の昇順でソート
        /// </summary>
        [Fact]
        public void Case08()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1&page=2&sortBy=domainName&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// 正常：ページ数が最終ページ・1ページ当たり表示件数あり・ドメイン名の降順でソート
        /// </summary>
        [Fact]
        public void Case09()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1&page=3&pageSize=10&sortBy=domainName&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// 正常：ページ数が最終ページ・1ページ当たり表示件数なし
        /// </summary>
        [Fact]
        public void Case10()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1&page=2", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// 正常：ページ数が最終ページ超過
        /// </summary>
        [Fact]
        public void Case11()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1&page=3", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// 正常：ページ数指定なし
        /// </summary>
        [Fact]
        public void Case12()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }
    }
}
