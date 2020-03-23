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
    /// 端末照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public class InquiryTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/devices";

        private readonly Organization _org1;
        private readonly Domain _domain1;
        private readonly DeviceGroup _deviceGroup1;
        private readonly UserGroup _userGroup1;
        private readonly LteModule _lte1;
        private readonly Device _device1;
        private readonly SuperAdmin _user0;
        private readonly EndUser _user1;
        private readonly ClientOs _clientOs1;
        private readonly ClientApp _clientApp1;
        private readonly OrganizationClientApp _organizationClientApp1;

        public InquiryTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();


            _context.Add(_clientOs1 = new ClientOs { Id = Guid.NewGuid(), Name = "Windows 10" });
            _context.Add(_clientApp1 = new ClientApp { Id = Guid.NewGuid(), ClientOs = _clientOs1, Version = "1903" });
            _context.Add(_organizationClientApp1 = new OrganizationClientApp { Id = Guid.NewGuid(), Organization = _org1, ClientApp = _clientApp1 });
            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = _org1 });
            _context.Add(_deviceGroup1 = new DeviceGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "deviceGroup1" });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup1" });
            _context.Add(_lte1 = new LteModule { Id = Guid.NewGuid(), Name = "lte1" });
            _context.Add(_device1 = new Device
            {
                Id = Guid.NewGuid(),
                Name = "deviceName1",
                Domain = _domain1,
                ManagedNumber = "0001",
                SerialNumber = "0001",
                ProductName = "Test1",
                UseTpm = true, 
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse("2021-03-01"),
                LteModule = _lte1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
                WindowsSignInListCacheDays = 1,
                OrganizationClientApp = _organizationClientApp1,
            });
            _context.Add(_user0 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者;
            _context.Add(_user1 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者1
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain1"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/", token);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Empty(body);
        }

        /// <summary>
        /// ID：UUID以外
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/5", token);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// DBに端末不在
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}/{Guid.NewGuid()}", token);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"][nameof(Device)]);
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response,_,json) = Utils.Get(_client, $"{Url}/{_device1.Id}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_device1.Id, json["id"]);
            Assert.Equal(_device1.Name, json["name"].ToString());
            Assert.Equal(_device1.StartDate.ToString("yyyy-MM-dd"), (string)json["startDate"]);
            Assert.Equal(_device1.EndDate?.ToString("yyyy-MM-dd"), (string)json["endDate"]);
            Assert.Equal(_device1.ManagedNumber, json["managedNumber"]);
            Assert.Equal(_device1.SerialNumber, json["serialNumber"]);
            Assert.Equal(_device1.ProductName, json["productName"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.ClientOs.Name, json["organizationClientApp"]?["clientApp"]?["clientOs"]?["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.Version, json["organizationClientApp"]?["clientApp"]?["version"]);
            Assert.Equal(_device1.LteModule.Id, json["lteModule"]?["id"]);
            Assert.Equal(_device1.LteModule.Name, json["lteModule"]?["name"]);
            Assert.Equal(_device1.UseTpm, json["useTpm"]);
            Assert.Equal(_device1.WindowsSignInListCacheDays, json["windowsSignInListCacheDays"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroupId.ToString(), json["deviceGroups"]?[0]?["id"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroup.Name, json["deviceGroups"]?[0]?["name"]);
            Assert.Equal(_device1.Domain.Id, json["domain"]?["id"]);
            Assert.Equal(_device1.Domain.Name, json["domain"]?["name"].ToString());
            Assert.Equal(_device1.Domain.Organization.Code, json["domain"]?["organization"]?["code"]);
            Assert.Equal(_device1.Domain.Organization.Name, json["domain"]?["organization"]?["name"].ToString());
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.ClientOs.Name, json["organizationClientApp"]["clientApp"]["clientOs"]["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.Version, json["organizationClientApp"]["clientApp"]["version"]);
        }
    }
}
