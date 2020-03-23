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
    /// 自分端末更新
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class UpdateMineTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/devices/mine";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly DeviceGroup _deviceGroup1;
        private readonly DeviceGroup _deviceGroup2;
        private readonly Device _device1;
        private readonly Device _device1B;
        private readonly Device _device2;
        private readonly LteModule _lte1;
        private readonly ClientOs _clientOs1;
        private readonly ClientApp _clientApp1;
        private readonly OrganizationClientApp _organizationClientApp1;

        public UpdateMineTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_clientOs1 = new ClientOs { Id = Guid.NewGuid(), Name = "Windows 10" });
            _context.Add(_clientApp1 = new ClientApp { Id = Guid.NewGuid(), ClientOs = _clientOs1, Version = "1903" });
            _context.Add(_organizationClientApp1 = new OrganizationClientApp { Id = Guid.NewGuid(), Organization = _org1, ClientApp = _clientApp1 });
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup1", Domain = _domain1 });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup2", Domain = _domain2 });
            _context.Add(_deviceGroup1 = new DeviceGroup { Id = Guid.NewGuid(), Name = "deviceGroup1", Domain = _domain1 });
            _context.Add(_deviceGroup2 = new DeviceGroup { Id = Guid.NewGuid(), Name = "deviceGroup2", Domain = _domain2 });
            _context.Add(_device1 = new Device { OrganizationClientApp = _organizationClientApp1, Name = "device1", ManagedNumber = "1", Domain = _domain1, DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1} } });
            _context.Add(_device1B = new Device { OrganizationClientApp = _organizationClientApp1, Name = "device1b", ManagedNumber = "2", Domain = _domain1, DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1} } });
            _context.Add(_device2 = new Device { OrganizationClientApp = _organizationClientApp1, Name = "device2", ManagedNumber = "1", Domain = _domain2, DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup2} } });
            _context.Add(_lte1 = new LteModule { Id = Guid.NewGuid(), Name = "lte1", NwAdapterName = "", UseSoftwareRadioState = true });
            _context.Add(new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.SaveChanges();
        }

        /// <summary>
        /// URLにIDが不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new { };
            var result = Utils.Put(_client, $"{Url}/", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.Empty(body); // 不在
        }

        /// <summary>
        /// IDがUUIDでない
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = _lte1.Id,
                UseTpm = "true",
                WindowsSignInListCacheDays = 1,
            };
            var result = Utils.Put(_client, $"{Url}/a", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["id"]);
        }

        /// <summary>
        /// 入力が不在
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new { };
            var result = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            //Assert.NotNull(json["errors"]?["ManagedNumber"]); // nullable対応
            //Assert.NotNull(json["errors"]?["SerialNumber"]); // nullable対応
            //Assert.NotNull(json["errors"]?["ProductName"]); // nullable対応
            //Assert.NotNull(json["errors"]?["LteModuleId"]); // nullable対応
            Assert.NotNull(json["errors"]?["WindowsSignInListCacheDays"]);
        }

        /// <summary>
        /// 型変換エラー
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = "a", // UUID以外
                UseTpm = "b", // bool型以外
                WindowsSignInListCacheDays = "c", // 0以上の整数以外
            };
            var result = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]?["LteModuleId"]);
            Assert.NotNull(json["errors"]?["UseTpm"]);
            Assert.NotNull(json["errors"]?["WindowsSignInListCacheDays"]);
        }

        /// <summary>
        /// DBに端末が不在
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = _lte1.Id,
                UseTpm = "true",
                WindowsSignInListCacheDays = 1,
            };
            var result = Utils.Put(_client, $"{Url}/{Guid.NewGuid()}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"][nameof(Device)]);
        }

        /// <summary>
        /// 端末が他組織
        /// </summary>
        [Fact]
        public void Case06()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = _lte1.Id,
                UseTpm = "true",
                WindowsSignInListCacheDays = 1,
            };
            var result = Utils.Put(_client, $"{Url}/{_device2.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.NotNull(json["errors"]["Role"]);
        }

        ///// <summary>
        ///// 社内管理番号が組織内で重複
        ///// </summary>
        //[Fact]
        //public void Case07()
        //{
        //    var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
        //    var obj = new
        //    {
        //        ManagedNumber = _device1B.ManagedNumber, // 重複
        //        SerialNumber = "1",
        //        ProductName = "1",
        //        LteModuleId = _lte1.Id.ToString(),
        //        UseTpm = "true",
        //        WindowsSignInListCacheDays = 1,
        //    };
        //    var result = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), token);
        //    var body = result.Content.ReadAsStringAsync().Result;
        //    var json = JObject.Parse(body);
        //    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        //    Assert.NotNull(json["traceId"]); // 存在
        //    Assert.NotNull(json["errors"]?["managedNumber"]);
        //}

        /// <summary>
        /// Windowsサインイン許可リストキャッシュ日数が0
        /// </summary>
        [Fact]
        public void Case08()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = _lte1.Id,
                OrganizationClientAppId = _device1.OrganizationClientApp.Id,
                UseTpm = true,
                WindowsSignInListCacheDays = 0,
            };
            var result = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_device1.Id, json["id"]);
            Assert.Equal(_device1.Name, json["name"]);
            Assert.Equal(obj.ManagedNumber, json["managedNumber"]);
            Assert.Equal(obj.SerialNumber, json["serialNumber"]);
            Assert.Equal(obj.ProductName, json["productName"]);
            Assert.Equal(obj.LteModuleId, json["lteModule"]?["id"]);
            Assert.Equal(_lte1.Name, json["lteModule"]?["name"]);
            Assert.Equal(obj.UseTpm, json["useTpm"]);
            Assert.Equal(obj.WindowsSignInListCacheDays, json["windowsSignInListCacheDays"]);
            Assert.Equal(_device1.DomainId, json["domainId"]);
            Assert.Equal(_device1.Domain.Name, json["domain"]?["name"]);
            Assert.Equal(_device1.Domain.Organization.Code, json["domain"]?["organization"]?["code"]);
            Assert.Equal(_device1.Domain.Organization.Name, json["domain"]?["organization"]?["name"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroupId.ToString(), json["deviceGroups"]?[0]?["id"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroup.Name, json["deviceGroups"]?[0]?["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.ClientOs.Name, json["organizationClientApp"]?["clientApp"]?["clientOs"]?["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.Version, json["organizationClientApp"]?["clientApp"]?["version"]);
        }

        /// <summary>
        /// Windowsサインイン許可リストキャッシュ日数が1
        /// </summary>
        [Fact]
        public void Case09()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = _lte1.Id,
                OrganizationClientAppId = _device1.OrganizationClientApp.Id,
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
            };
            var result = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_device1.Id, json["id"]);
            Assert.Equal(_device1.Name, json["name"]);
            Assert.Equal(obj.ManagedNumber, json["managedNumber"]);
            Assert.Equal(obj.SerialNumber, json["serialNumber"]);
            Assert.Equal(obj.ProductName, json["productName"]);
            Assert.Equal(obj.LteModuleId, json["lteModule"]?["id"]);
            Assert.Equal(_lte1.Name, json["lteModule"]?["name"]);
            Assert.Equal(obj.UseTpm, json["useTpm"]);
            Assert.Equal(obj.WindowsSignInListCacheDays, json["windowsSignInListCacheDays"]);
            Assert.Equal(_device1.DomainId, json["domainId"]);
            Assert.Equal(_device1.Domain.Name, json["domain"]?["name"]);
            Assert.Equal(_device1.Domain.Organization.Code, json["domain"]?["organization"]?["code"]);
            Assert.Equal(_device1.Domain.Organization.Name, json["domain"]?["organization"]?["name"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroupId.ToString(), json["deviceGroups"]?[0]?["id"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroup.Name, json["deviceGroups"]?[0]?["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.ClientOs.Name, json["organizationClientApp"]?["clientApp"]?["clientOs"]?["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.Version, json["organizationClientApp"]?["clientApp"]?["version"]);
        }

        /// <summary>
        /// 入力の社内管理番号、製造番号、機種名が不在のケース
        /// </summary>
        [Fact]
        public void Case10()
        {
            var obj = new
            {
                //ManagedNumber = "1", // 不在
                //SerialNumber = "1", // 不在
                //ProductName = "1", // 不在
                LteModuleId = _lte1.Id,
                OrganizationClientAppId = _device1.OrganizationClientApp.Id,
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_device1.Id, json["id"]);
            Assert.Equal(_device1.Name, json["name"]);
            Assert.Null((string)json["managedNumber"]);
            Assert.Null((string)json["serialNumber"]);
            Assert.Null((string)json["productName"]);
            Assert.Equal(obj.LteModuleId, json["lteModule"]?["id"]);
            Assert.Equal(_lte1.Name, json["lteModule"]?["name"]);
            Assert.Equal(obj.UseTpm, json["useTpm"]);
            Assert.Equal(obj.WindowsSignInListCacheDays, json["windowsSignInListCacheDays"]);
            Assert.Equal(_device1.DomainId, json["domainId"]);
            Assert.Equal(_device1.Domain.Name, json["domain"]?["name"]);
            Assert.Equal(_device1.Domain.Organization.Code, json["domain"]?["organization"]?["code"]);
            Assert.Equal(_device1.Domain.Organization.Name, json["domain"]?["organization"]?["name"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroupId.ToString(), json["deviceGroups"]?[0]?["id"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroup.Name, json["deviceGroups"]?[0]?["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.ClientOs.Name, json["organizationClientApp"]?["clientApp"]?["clientOs"]?["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.Version, json["organizationClientApp"]?["clientApp"]?["version"]);
        }

        /// <summary>
        /// 入力の組織端末アプリIDが不在のケース
        /// </summary>
        [Fact]
        public void Case11()
        {
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = _lte1.Id,
                //OrganizationClientAppId = _device1.OrganizationClientApp.Id, // 不在
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
            };
            var (response, body, json) = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_device1.Id, json["id"]);
            Assert.Equal(_device1.Name, json["name"]);
            Assert.Equal(obj.ManagedNumber, (string)json["managedNumber"]);
            Assert.Equal(obj.SerialNumber, (string)json["serialNumber"]);
            Assert.Equal(obj.ProductName, (string)json["productName"]);
            Assert.Equal(obj.LteModuleId, json["lteModule"]["id"]);
            Assert.Equal(_lte1.Name, json["lteModule"]["name"]);
            Assert.Equal(obj.UseTpm, json["useTpm"]);
            Assert.Equal(obj.WindowsSignInListCacheDays, json["windowsSignInListCacheDays"]);
            Assert.Equal(_device1.DomainId, json["domainId"]);
            Assert.Equal(_device1.Domain.Name, json["domain"]["name"]);
            Assert.Equal(_device1.Domain.Organization.Code, json["domain"]["organization"]["code"]);
            Assert.Equal(_device1.Domain.Organization.Name, json["domain"]["organization"]["name"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroupId.ToString(), json["deviceGroups"][0]["id"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroup.Name, json["deviceGroups"][0]["name"]);
            Assert.Null((string)json["organizationClientApp"]);
        }

        /// <summary>
        /// 入力の組織端末アプリIDがUUID以外のケース
        /// </summary>
        [Fact]
        public void Case12()
        {
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = _lte1.Id,
                OrganizationClientAppId = "a", // UUIDでない
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Contains("Error converting value", (string)json["errors"]["OrganizationClientAppId"][0]);
        }

        /// <summary>
        /// 入力のLTEモジュールIDが不在のケース
        /// </summary>
        [Fact]
        public void Case13()
        {
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                //LteModuleId = _lte1.Id, // 不在
                OrganizationClientAppId = _device1.OrganizationClientApp.Id,
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
            };
            var (response, body, json) = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
            Assert.Equal(_device1.Id, json["id"]);
            Assert.Equal(_device1.Name, json["name"]);
            Assert.Equal(obj.ManagedNumber, (string)json["managedNumber"]);
            Assert.Equal(obj.SerialNumber, (string)json["serialNumber"]);
            Assert.Equal(obj.ProductName, (string)json["productName"]);
            Assert.Null((string)json["lteModule"]);
            Assert.Equal(obj.UseTpm, json["useTpm"]);
            Assert.Equal(obj.WindowsSignInListCacheDays, json["windowsSignInListCacheDays"]);
            Assert.Equal(_device1.DomainId, json["domainId"]);
            Assert.Equal(_device1.Domain.Name, json["domain"]["name"]);
            Assert.Equal(_device1.Domain.Organization.Code, json["domain"]["organization"]["code"]);
            Assert.Equal(_device1.Domain.Organization.Name, json["domain"]["organization"]["name"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroupId.ToString(), json["deviceGroups"][0]["id"]);
            Assert.Equal(_device1.DeviceGroupDevices.First().DeviceGroup.Name, json["deviceGroups"][0]["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.ClientOs.Name, json["organizationClientApp"]?["clientApp"]["clientOs"]?["name"]);
            Assert.Equal(_device1.OrganizationClientApp.ClientApp.Version, json["organizationClientApp"]?["clientApp"]["version"]);
        }

        /// <summary>
        /// 入力のLTEモジュールIDがUUID以外のケース
        /// </summary>
        [Fact]
        public void Case14()
        {
            var obj = new
            {
                ManagedNumber = "1",
                SerialNumber = "1",
                ProductName = "1",
                LteModuleId = "a", // UUIDでない
                OrganizationClientAppId = _device1.OrganizationClientApp.Id,
                UseTpm = true,
                WindowsSignInListCacheDays = 1,
            };
            var (response, _, json) = Utils.Put(_client, $"{Url}/{_device1.Id}", Utils.CreateJsonContent(obj), "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]); // 存在
            Assert.Contains("Error converting value", (string)json["errors"]["LteModuleId"][0]);
        }
    }
}
