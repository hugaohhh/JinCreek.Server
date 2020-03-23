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
    /// ユーザー更新
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public class UpdateOursTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/users/ours";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain;
        private readonly Domain _domain12;
        private readonly Domain _domain21;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;


        public UpdateOursTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain = new Domain { Id = Guid.NewGuid(), Name = "domain", Organization = _org1 });
            _context.Add(_domain12 = new Domain { Id = Guid.NewGuid(), Name = "domain12", Organization = _org1 });
            _context.Add(_domain21 = new Domain { Id = Guid.NewGuid(), Name = "domain21", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain, Name = "userGroup1" });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain21, Name = "userGroup2" });
            _context.Add(new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain }); // ユーザー管理者
            _context.SaveChanges();
        }

        /// <summary>
        /// ID：不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            GeneralUser user = new GeneralUser
            {
                AccountName = "User03",
                Domain = _domain,
                Name = "user03",
                AuthenticateWhenUnlockingScreen = true
            };
            UserGroupEndUser userGroupEndUser = new UserGroupEndUser()
            {
                UserGroup = _userGroup1,
                EndUser = user
            };
            _context.Add(user);
            _context.AddRange(userGroupEndUser);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain"); // ユーザー管理者
            var obj = new
            {
                AuthenticateWhenUnlockingScreen = false
            };
            var result = Utils.Put(_client, $"{Url}/", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// ID：'UUID以外
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain"); // ユーザー管理者
            var obj = new
            {
                AuthenticateWhenUnlockingScreen = false
            };
            var result = Utils.Put(_client, $"{Url}/123456", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// クライアント画面ロック時制御 :不在
        /// </summary>
        [Fact]
        public void Case03()
        {
            GeneralUser user = new GeneralUser
            {
                AccountName = "User03",
                Domain = _domain,
                Name = "User03",
                AuthenticateWhenUnlockingScreen = true
            };
            UserGroupEndUser userGroupEndUser = new UserGroupEndUser()
            {
                UserGroup = _userGroup1,
                EndUser = user
            };
            _context.Add(user);
            _context.AddRange(userGroupEndUser);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain"); // ユーザー管理者
            var obj = new
            {
            };
            var result = Utils.Put(_client, $"{Url}/{user.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["AuthenticateWhenUnlockingScreen"]);
        }

        /// <summary>
        /// クライアント画面ロック時制御 :'bool型以外
        /// </summary>
        [Fact]
        public void Case04()
        {
            GeneralUser user = new GeneralUser
            {
                AccountName = "User03",
                Domain = _domain,
                Name = "User03",
                AuthenticateWhenUnlockingScreen = true
            };
            UserGroupEndUser userGroupEndUser = new UserGroupEndUser()
            {
                UserGroup = _userGroup1,
                EndUser = user
            };
            _context.Add(user);
            _context.AddRange(userGroupEndUser);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain"); // ユーザー管理者
            var obj = new
            {
                AuthenticateWhenUnlockingScreen = "string"
            };
            var result = Utils.Put(_client, $"{Url}/{user.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["AuthenticateWhenUnlockingScreen"]);
        }

        /// <summary>
        /// ユーザー：不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain"); // ユーザー管理者
            var obj = new
            {
                AuthenticateWhenUnlockingScreen = true
            };
            var result = Utils.Put(_client, $"{Url}/{Guid.NewGuid()}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(User)]);
        }

        /// <summary>
        /// ユーザー : '存在（他組織）
        /// </summary>
        [Fact]
        public void Case06()
        {

            GeneralUser user2 = new GeneralUser
            {
                AccountName = "User02",
                Domain = _domain21,
                Name = "user02",
                AuthenticateWhenUnlockingScreen = true
            };
            UserGroupEndUser userGroupEndUser = new UserGroupEndUser()
            {
                UserGroup = _userGroup2,
                EndUser = user2
            };
            _context.AddRange(user2);
            _context.AddRange(userGroupEndUser);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain"); // ユーザー管理者
            var obj = new
            {
                AuthenticateWhenUnlockingScreen = false
            };
            var result = Utils.Put(_client, $"{Url}/{user2.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]["Role"]);
        }

        /// <summary>
        /// ユーザー : '存在（自組織）
        /// </summary>
        [Fact]
        public void Case07()
        {
            GeneralUser user = new GeneralUser
            {
                AccountName = "User03",
                Domain = _domain,
                Name = "User03",
                AuthenticateWhenUnlockingScreen = true,
                AvailablePeriods = new HashSet<AvailablePeriod> { new AvailablePeriod { StartDate = DateTime.Parse("2020-02-03"), EndDate = DateTime.Parse("2021-01-03") } },
                UserGroupEndUsers = new HashSet<UserGroupEndUser> { new UserGroupEndUser { UserGroup = _userGroup1 } },
            };
            _context.Add(user);
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain"); // ユーザー管理者
            var obj = new
            {
                AuthenticateWhenUnlockingScreen = false
            };
            var result = Utils.Put(_client, $"{Url}/{user.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(json["traceId"]);
            Assert.Equal(user.AccountName, json["accountName"]);
            Assert.Equal(user.Name, json["name"]);
            Assert.Equal(obj.AuthenticateWhenUnlockingScreen.ToString(), json["authenticateWhenUnlockingScreen"].ToString());
            Assert.Equal(user.Id, json["id"]);
            Assert.Equal(user.Domain.Id, json["domain"]?["id"]);
            Assert.Equal(user.UserGroupEndUsers?.FirstOrDefault()?.UserGroup?.Name, json["userGroups"]?[0]?["name"].ToString());
            Assert.Equal(user.Domain.Name, json["domain"]?["name"].ToString());
            Assert.Equal(user.Domain.Organization.Name, json["domain"]?["organization"]?["name"].ToString());
            Assert.Equal(user.AvailablePeriods.First().StartDate, json["startDate"]);
            Assert.Equal(user.AvailablePeriods.First().EndDate, json["endDate"]);
        }
    }
}
