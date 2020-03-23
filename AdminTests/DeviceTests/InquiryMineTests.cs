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

namespace JinCreek.Server.AdminTests.DeviceTests
{
    /// <summary>
    /// 自分端末照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class InquiryMineTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/devices/mine";

        private static readonly Organization Org1 = Utils.CreateOrganization(code: 1, name: "org1");
        private static readonly Organization Org2 = Utils.CreateOrganization(code: 2, name: "org2");
        private static readonly Domain Domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = Org1 };
        private static readonly Domain Domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = Org2 };
        private static readonly DeviceGroup DeviceGroup1 = new DeviceGroup { Id = Guid.NewGuid(), Domain = Domain1, Name = "deviceGroup1" };
        private static readonly DeviceGroup DeviceGroup2 = new DeviceGroup { Id = Guid.NewGuid(), Domain = Domain2, Name = "deviceGroup2" };
        private static readonly UserGroup UserGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = Domain1, Name = "userGroup1" };
        private static readonly UserGroup UserGroup2 = new UserGroup { Id = Guid.NewGuid(), Domain = Domain2, Name = "userGroup2" };
        private static readonly LteModule lte1 = new LteModule { Name = "lte1", Id = Guid.NewGuid() };
        private static readonly LteModule lte2 = new LteModule { Name = "lte2", Id = Guid.NewGuid() };
        private static readonly Device Device1 = new Device
        {
            Id = Guid.NewGuid(),
            Name = "deviceName1",
            Domain = Domain1,
            ManagedNumber = "0001",
            SerialNumber = "0001",
            ProductName = "Test1",
            UseTpm = true,
            LteModule = lte1,
            StartDate = DateTime.Parse("2020-02-01"),
            EndDate = DateTime.Parse("2021-03-01"),
            DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = DeviceGroup1} },
            OrganizationClientApp = new OrganizationClientApp()
            {
                Organization = Org1,
                ClientApp = new ClientApp()
                {
                    ClientOs = new ClientOs()
                    {
                        Name = "window"
                    },
                    Version = "1.1"
                },
            },
            WindowsSignInListCacheDays = 1
        };
        private static readonly Device Device2 = new Device
        {
            Id = Guid.NewGuid(),
            Name = "deviceName2",
            Domain = Domain2,
            ManagedNumber = "0002",
            SerialNumber = "0002",
            ProductName = "Test2",
            UseTpm = true,
            LteModule = lte2,
            DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = DeviceGroup2} },
            WindowsSignInListCacheDays = 1
        };
        private static readonly SuperAdmin User0 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }; // スーパー管理者;
        private static readonly EndUser User1 = new UserAdmin { Name = "user1", AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = Domain1 }; // ユーザー管理者1
        private static readonly EndUser User2 = new UserAdmin { Name = "user2", AccountName = "user2", Password = Utils.HashPassword("user2"), Domain = Domain2 }; // ユーザー管理者2



        public InquiryMineTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            //_context.Organization.RemoveRange(_context.Organization);
            //_context.Domain.RemoveRange(_context.Domain);
            //_context.UserGroup.RemoveRange(_context.UserGroup);
            //_context.DeviceGroup.RemoveRange(_context.DeviceGroup);
            //_context.Device.RemoveRange(_context.Device);
            //_context.LteModule.RemoveRange(_context.LteModule);
            //_context.User.RemoveRange(_context.User);
            //_context.SaveChanges();

            _context.AddRange(Org1, Device1, Domain1, UserGroup1, DeviceGroup1, lte1);
            _context.AddRange(Org2, Device2, Domain2, UserGroup2, DeviceGroup2, lte2);
            _context.AddRange(User0, User1, User2);
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/a", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// DBに端末不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/{Guid.NewGuid()}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Device)]);
        }

        /// <summary>
        /// 他組織
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/{Device2.Id}", token);
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

            var (response, _, json) = Utils.Get(_client, $"{Url}/{Device1.Id}", "user1", "user1",1,Domain1.Name); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(Device1.Id, json["id"]);
            Assert.Equal(Device1.Name, json["name"].ToString());
            Assert.Equal(Device1.StartDate.ToString("yyyy-MM-dd"), (string)json["startDate"]);
            Assert.Equal(Device1.EndDate?.ToString("yyyy-MM-dd"), (string)json["endDate"]);
            Assert.Equal(Device1.ManagedNumber, json["managedNumber"]);
            Assert.Equal(Device1.SerialNumber, json["serialNumber"]);
            Assert.Equal(Device1.ProductName, json["productName"]);
            Assert.Equal(Device1.OrganizationClientApp.ClientApp.ClientOs.Name, json["organizationClientApp"]?["clientApp"]?["clientOs"]?["name"]);
            Assert.Equal(Device1.OrganizationClientApp.ClientApp.Version, json["organizationClientApp"]?["clientApp"]?["version"]);
            Assert.Equal(Device1.LteModule.Id, json["lteModule"]?["id"]);
            Assert.Equal(Device1.LteModule.Name, json["lteModule"]?["name"]);
            Assert.Equal(Device1.UseTpm, json["useTpm"]);
            Assert.Equal(Device1.WindowsSignInListCacheDays, json["windowsSignInListCacheDays"]);
            Assert.Equal(Device1.DeviceGroupDevices.First().DeviceGroupId.ToString(), json["deviceGroups"]?[0]?["id"]);
            Assert.Equal(Device1.DeviceGroupDevices.First().DeviceGroup.Name, json["deviceGroups"]?[0]?["name"]);
            Assert.Equal(Device1.Domain.Id, json["domain"]?["id"]);
            Assert.Equal(Device1.Domain.Name, json["domain"]?["name"].ToString());
            Assert.Equal(Device1.Domain.Organization.Code, json["domain"]?["organization"]?["code"]);
            Assert.Equal(Device1.Domain.Organization.Name, json["domain"]?["organization"]?["name"].ToString());
        }
    }
}
