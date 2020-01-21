using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
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
        private const string Url = "/api/domains/mine";

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
        /// ページ：不在、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// ページ：0以下、ページサイズ：存在、ソート：不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?page=0&pageSize=2", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// ページ：中間ページ、ページサイズ：不在、ソート：ドメイン名昇順
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?page=2&sortBy=domainName&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：存在、ソート：ドメイン名降順
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?page=21&pageSize=2&sortBy=domainName&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// ページ：最終ページ、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?page=3", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// ページ：最終ページ超過、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?page=4", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }

        /// <summary>
        /// ページ：不在、ページサイズ：不在、ソート：不在
        /// </summary>
        [Fact]
        public void Case07()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            // TODO: impl bellow
        }
    }
}
