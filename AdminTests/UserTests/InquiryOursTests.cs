using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.UserTests
{
    /// <summary>
    /// 自分ユーザー照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public class InquiryOursTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> _factory;

        private const string Url = "/api/users/ours";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly SuperAdmin _user0;
        private readonly EndUser _user1;
        private readonly EndUser _user2;

        public InquiryOursTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            // Arrange
            using var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(context);

            context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = _org1 });
            context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain2", Organization = _org2 });
            context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup1" });
            context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup2" });
            context.Add(_user0 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") });  // スーパー管理者;
            context.Add(_user1 = new UserAdmin // ユーザー管理者
            {
                Name = "user1",
                AccountName = "user1",
                Password = Utils.HashPassword("user1"),
                Domain = _domain1,
                UserGroupEndUsers = new HashSet<UserGroupEndUser> { new UserGroupEndUser { UserGroup = _userGroup1 } },
                AvailablePeriods = new HashSet<AvailablePeriod> { new AvailablePeriod { StartDate = DateTime.Parse("2020-02-03"), EndDate = DateTime.Parse("2021-01-03") } }
            });
            context.User.Add(_user2 = new UserAdmin { Name = "user2", AccountName = "user2", Password = Utils.HashPassword("user2"), Domain = _domain2 }); // ユーザー管理者2
            context.SaveChanges();
        }

        /// <summary>
        /// ID：UUID以外
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/a", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// DBにユーザー不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/{Guid.NewGuid()}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(EndUser)]);
        }

        /// <summary>
        /// 他組織
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/{_user2.Id}", token);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/{_user1.Id}", token);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_user1.Id, json["id"]);
            Assert.Equal(_user1.AccountName, json["accountName"].ToString());
            Assert.Equal(_user1.Name, json["name"].ToString());
            Assert.Equal(_user1.AuthenticateWhenUnlockingScreen, json["authenticateWhenUnlockingScreen"]);
            Assert.Equal(_user1.UserGroupEndUsers.First().UserGroup.Name, json["userGroups"]?[0]?["name"].ToString());
            Assert.Equal(_user1.Domain.Name, json["domain"]?["name"].ToString());
            Assert.Equal(_user1.Domain.Organization.Name, json["domain"]?["organization"]?["name"].ToString());
            Assert.Equal(_user1.AvailablePeriods.First().StartDate, json["startDate"]);
            Assert.Equal(_user1.AvailablePeriods.First().EndDate, json["endDate"]);
        }
    }
}
