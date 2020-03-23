using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.Controllers
{
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class AuthenticationControllerTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string LoginUrl = "/api/authentication/login";
        private const string RefreshUrl = "/api/authentication/refresh";

        public AuthenticationControllerTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            // Arrange
            Utils.RemoveAllEntities(_context);
            _context.Add(new SuperAdmin { AccountName = "t-suzuki@indigo.co.jp", Password = Utils.HashPassword("9Q'vl!") });
            _context.SaveChanges();
        }

        [Fact]
        public async void TestStatusCode()
        {
            {
                // GET, DELETE, POST, PUT, PATCH
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _client.GetAsync(LoginUrl)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _client.DeleteAsync(LoginUrl)).StatusCode);
                Assert.NotEqual(HttpStatusCode.MethodNotAllowed, (await _client.PostAsync(LoginUrl, null)).StatusCode); // POST: see below
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _client.PutAsync(LoginUrl, null)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _client.PatchAsync(LoginUrl, null)).StatusCode);

                // POSTs
                Assert.Equal(HttpStatusCode.OK, (await _client.PostAsync(LoginUrl, Utils.CreateJsonContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsync(LoginUrl, Utils.CreateJsonContent(new { username = "aaaaaaaaaaaaaaaaaaaaa", password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsync(LoginUrl, Utils.CreateJsonContent(new { username = "t-suzuki@indigo.co.jp", password = "aaaaaa" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsync(LoginUrl, Utils.CreateJsonContent(new { username = "t-suzuki@indigo.co.jp" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsync(LoginUrl, Utils.CreateJsonContent(new { password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsync(LoginUrl, Utils.CreateJsonContent(null))).StatusCode);
            }

            {
                HttpResponseMessage response = await _client.PostAsync(LoginUrl, Utils.CreateJsonContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }));
                string refreshToken = (string)JObject.Parse(await response.Content.ReadAsStringAsync())["refreshToken"];

                // GET, DELETE, POST, PUT, PATCH
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _client.GetAsync(RefreshUrl)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _client.DeleteAsync(RefreshUrl)).StatusCode);
                Assert.NotEqual(HttpStatusCode.MethodNotAllowed, (await _client.PostAsync(RefreshUrl, null)).StatusCode); // POST: see below
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _client.PutAsync(RefreshUrl, null)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _client.PatchAsync(RefreshUrl, null)).StatusCode);

                // POST
                Assert.Equal(HttpStatusCode.OK, (await _client.PostAsync(RefreshUrl, Utils.CreateJsonContent(new { refreshToken }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsync(RefreshUrl, Utils.CreateJsonContent(new { refreshToken = "aaaa" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsync(RefreshUrl, Utils.CreateJsonContent(null))).StatusCode);
            }
        }

        [Fact]
        public async void TestContent()
        {
            string refreshToken;
            {
                var json = JObject.Parse(await (await _client.PostAsync(LoginUrl, Utils.CreateJsonContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }))).Content.ReadAsStringAsync());
                Assert.NotNull(json["accessToken"]);
                Assert.NotNull(json["refreshToken"]);
                refreshToken = (string)json["refreshToken"];
            }

            {
                var json = JObject.Parse(await (await _client.PostAsync(RefreshUrl, Utils.CreateJsonContent(new { refreshToken }))).Content.ReadAsStringAsync());
                Assert.NotNull(json["accessToken"]);
            }
        }

        /// <summary>
        /// 組織がinvalidの場合はエラー
        /// </summary>
        [Fact]
        public void Case01()
        {
            var org1 = Utils.CreateOrganization(code: 1, name: "org1", isValid: false); // isValid: false
            var domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = org1 };
            var user1 = new UserAdmin { AccountName = "user1", Name = "", Domain = domain1, Password = Utils.HashPassword("user1") };
            _context.AddRange(org1, domain1, user1);
            _context.SaveChanges();

            var query = new { organizationCode = 1, domainName = "domain1", userName = "user1", password = "user1" };
            var response = _client.PostAsync(LoginUrl, Utils.CreateJsonContent(query)).Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// 組織の日付範囲外の場合はエラー
        /// </summary>
        [Fact]
        public void Case02()
        {
            var org1 = Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Today.AddDays(1)); // startDate: DateTime.Today.AddDays(1)
            var domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = org1 };
            var user1 = new UserAdmin { AccountName = "user1", Name = "", Domain = domain1, Password = Utils.HashPassword("user1") };
            _context.AddRange(org1, domain1, user1);
            _context.SaveChanges();

            var query = new { organizationCode = 1, domainName = "domain1", userName = "user1", password = "user1" };
            var response = _client.PostAsync(LoginUrl, Utils.CreateJsonContent(query)).Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// 組織の日付範囲外の場合はエラー
        /// </summary>
        [Fact]
        public void Case03()
        {
            var org1 = Utils.CreateOrganization(code: 1, name: "org1", endDate: DateTime.Today.AddDays(-1)); // endDate: DateTime.Today.AddDays(-1)
            var domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = org1 };
            var user1 = new UserAdmin { AccountName = "user1", Name = "", Domain = domain1, Password = Utils.HashPassword("user1") };
            _context.AddRange(org1, domain1, user1);
            _context.SaveChanges();

            var query = new { organizationCode = 1, domainName = "domain1", userName = "user1", password = "user1" };
            var response = _client.PostAsync(LoginUrl, Utils.CreateJsonContent(query)).Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// ユーザーの日付範囲外の場合はエラー
        /// </summary>
        [Fact]
        public void Case04()
        {
            var org1 = Utils.CreateOrganization(code: 1, name: "org1");
            var domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = org1 };
            var user1 = new UserAdmin { AccountName = "user1", Name = "", Domain = domain1, Password = Utils.HashPassword("user1") };
            var availablePeriod1 = new AvailablePeriod { StartDate = DateTime.Today.AddDays(1), EndDate = null, EndUser = user1 }; // StartDate = DateTime.Today.AddDays(1)
            _context.AddRange(org1, domain1, user1, availablePeriod1);
            _context.SaveChanges();

            var query = new { organizationCode = 1, domainName = "domain1", userName = "user1", password = "user1" };
            var response = _client.PostAsync(LoginUrl, Utils.CreateJsonContent(query)).Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// ユーザーの日付範囲外の場合はエラー
        /// </summary>
        [Fact]
        public void Case05()
        {
            var org1 = Utils.CreateOrganization(code: 1, name: "org1");
            var domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = org1 };
            var user1 = new UserAdmin { AccountName = "user1", Name = "", Domain = domain1, Password = Utils.HashPassword("user1") };
            var availablePeriod1 = new AvailablePeriod { StartDate = DateTime.MinValue, EndDate = DateTime.Today.AddDays(-1), EndUser = user1 }; // EndDate = DateTime.Today.AddDays(-1)
            _context.AddRange(org1, domain1, user1, availablePeriod1);
            _context.SaveChanges();

            var query = new { organizationCode = 1, domainName = "domain1", userName = "user1", password = "user1" };
            var response = _client.PostAsync(LoginUrl, Utils.CreateJsonContent(query)).Result;
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// 正常系
        /// </summary>
        [Fact]
        public void Case06()
        {
            var org1 = Utils.CreateOrganization(code: 1, name: "org1", startDate: DateTime.Today, endDate: DateTime.Today);
            var domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = org1 };
            var user1 = new UserAdmin { AccountName = "user1", Name = "", Domain = domain1, Password = Utils.HashPassword("user1") };
            var availablePeriod1 = new AvailablePeriod { StartDate = DateTime.Today, EndDate = DateTime.Today, EndUser = user1 };
            _context.AddRange(org1, domain1, user1, availablePeriod1);
            _context.SaveChanges();

            var query = new { organizationCode = 1, domainName = "domain1", userName = "user1", password = "user1" };
            var response = _client.PostAsync(LoginUrl, Utils.CreateJsonContent(query)).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
