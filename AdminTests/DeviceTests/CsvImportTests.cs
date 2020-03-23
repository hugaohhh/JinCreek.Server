using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace JinCreek.Server.AdminTests.DeviceTests
{
    /// <summary>
    /// 端末インポート
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class CsvImportTests : IClassFixture<CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/devices/csv";
        private const string CsvHeader =
            "ID,Domain,Device Group,Name,Managed Number,Serial Number,Product Name,LTE Module ID,LTE Module,JinCreek App ID,OS,JinCreek App Version,Use Tpm,OS SignIn List Cache Days,Start Date,End Date";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly DeviceGroup _deviceGroup1;
        private readonly DeviceGroup _deviceGroup2;
        private readonly LteModule _lte1;
        private readonly ClientOs _clientOs1;
        private readonly ClientApp _clientApp1;
        private readonly OrganizationClientApp _organizationClientApp1;

        public CsvImportTests(CustomWebApplicationFactoryWithMariaDb<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain1", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain2", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup1", Domain = _domain1 });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup2", Domain = _domain2 });
            _context.Add(_deviceGroup1 = new DeviceGroup { Id = Guid.NewGuid(), Name = "deviceGroup1", Domain = _domain1 });
            _context.Add(_deviceGroup2 = new DeviceGroup { Id = Guid.NewGuid(), Name = "deviceGroup2", Domain = _domain2 });
            _context.Add(_lte1 = new LteModule { Id = Guid.NewGuid(), Name = "lte1", NwAdapterName = "" });
            _context.Add(_lte1 = new LteModule { Id = Guid.NewGuid(), Name = "lte2", NwAdapterName = "" });
            _context.Add(new SuperAdmin { Name = "", AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { Name = "", AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_clientOs1 = new ClientOs { Id = Guid.NewGuid(), Name = "Windows 10" });
            _context.Add(_clientApp1 = new ClientApp { Id = Guid.NewGuid(), ClientOs = _clientOs1, Version = "1903" });
            _context.Add(_organizationClientApp1 = new OrganizationClientApp { Id = Guid.NewGuid(), Organization = _org1, ClientApp = _clientApp1 });
            _context.SaveChanges();
        }

        /// <summary>
        /// ロール : 不在
        /// </summary>
        [Fact]
        public void Case01()
        {
            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($",domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,False,0,0001-01-01,");

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user1", "user1", 1, "domain1"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }


        /// <summary>
        /// 端末ID：不在　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case02()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.Add(device1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($",domain1,deviceGroup1,device3,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Id"]);
        }

        /// <summary>
        /// 端末ID：UUIDではない　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case03()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.Add(device1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"abcdd,domain1,deviceGroup1,device3,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["ID"]);
        }

        /// <summary>
        /// 社内管理番号：不在　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case04()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device2 = new Device
            {
                Name = "device2",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device1, device2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"{device2.Id},domain1,deviceGroup1,device3,,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
        }

        /// <summary>
        /// 製造番号：不在　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case05()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device2 = new Device
            {
                Name = "device2",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device1, device2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"{device2.Id},domain1,deviceGroup1,device3,11,,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
        }

        /// <summary>
        /// 機種名：不在　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case06()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device2 = new Device
            {
                Name = "device2",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device1, device2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"{device2.Id},domain1,deviceGroup1,device3,11,1001,,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(json["traceId"]); // 不在
            Assert.Null(json["errors"]); // 不在
        }

        /// <summary>
        /// LTEモジュールID：不在　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case07()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device2 = new Device
            {
                Name = "device2",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device1, device2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"{device2.Id},domain1,deviceGroup1,device2,11,1001,type01,,,{_organizationClientApp1.Id},,,False,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["LteModuleId"]);
        }

        /// <summary>
        /// LTEモジュールID：UUIDではない　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case08()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device2 = new Device
            {
                Name = "device2",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device1, device2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"{device2.Id},domain1,deviceGroup1,device2,11,1001,type01,a,lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["LTE Module ID"]);
        }

        /// <summary>
        /// TPM利用：不在　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case09()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.Add(device1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"c2546192-b84c-481f-8c2b-ca0c38ab4eed,domain1,deviceGroup1,device3,11,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["UseTpm"]);
        }

        /// <summary>
        /// TPM利用：'bool型以外　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case10()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.Add(device1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"c2546192-b84c-481f-8c2b-ca0c38ab4eed,domain1,deviceGroup1,device3,11,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,abcddd,0,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Use Tpm"]);
        }

        /// <summary>
        /// Windowsサインイン許可リストキャッシュ日数：不在　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case11()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.Add(device1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"c2546192-b84c-481f-8c2b-ca0c38ab4eed,domain1,deviceGroup1,device3,11,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["WindowsSignInListCacheDays"]);
        }

        /// <summary>
        /// Windowsサインイン許可リストキャッシュ日数：'0以上の整数以外　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case12()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.Add(device1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"c2546192-b84c-481f-8c2b-ca0c38ab4eed,domain1,deviceGroup1,device3,11,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,-120,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["WindowsSignInListCacheDays"]);
        }

        /// <summary>
        /// 全部正常（端末レコード不在）　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case13()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.Add(device1);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"c2546192-b84c-481f-8c2b-ca0c38ab4eed,domain1,deviceGroup1,device3,11,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,1,0001-01-01,");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("not found", (string)json["errors"]["Device"][0]);
        }

        ///// <summary>
        ///// 社内管理番号:'他レコードと重複して存在（端末レコード:'存在（自組織））　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        ///// </summary>
        //[Fact]
        //public void Case14()
        //{
        //    var device1 = new Device
        //    {
        //        Name = "device1",
        //        ManagedNumber = "1",
        //        SerialNumber = "",
        //        ProductName = "",
        //        WindowsSignInListCacheDays = 0,
        //        Domain = _domain1,
        //        DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
        //    };
        //    var device2 = new Device
        //    {
        //        Name = "device2",
        //        ManagedNumber = "2",
        //        SerialNumber = "",
        //        ProductName = "",
        //        WindowsSignInListCacheDays = 0,
        //        Domain = _domain1,
        //        DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
        //    };
        //    _context.AddRange(device1, device2);
        //    _context.SaveChanges();

        //    var obj = new StringBuilder();
        //    obj.AppendLine(CsvHeader);
        //    obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,2,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,1,0001-01-01,");
        //    obj.AppendLine($"{device2.Id},domain1,deviceGroup1,device2,3,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

        //    var (response, _, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
        //    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        //    Assert.NotNull(json["traceId"]);
        //    Assert.Equal("duplicate manage number", json["errors"]?["managedNumber"]);
        //    Assert.Equal("1", device1.ManagedNumber);
        //    Assert.Equal("2", device2.ManagedNumber);
        //}

        /// <summary>
        /// 端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0以上　端末レコード：'存在（自組織）Windowsサインイン許可リストキャッシュ日数:0
        /// </summary>
        [Fact]
        public void Case15()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device2 = new Device
            {
                Name = "device2",
                ManagedNumber = "2",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device1, device2);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,1,0001-01-01,");
            obj.AppendLine($"{device2.Id},domain1,deviceGroup1,device2,2,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,0,0001-01-01,");

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JArray.Parse(body);
            Assert.Equal("1", list[0]["windowsSignInListCacheDays"]);
            Assert.Equal("0", list[1]["windowsSignInListCacheDays"]);
        }

        /// <summary>
        /// 組織端末アプリIDが不在のケース
        /// </summary>
        [Fact]
        public void Case16()
        {
            var device15 = new Device
            {
                Name = "device15",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device17 = new Device
            {
                Name = "device17",
                ManagedNumber = "2",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device15, device17);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"{device15.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,1,0001-01-01,");
            obj.AppendLine($"{device17.Id},domain1,deviceGroup1,device2,2,1001,type01,{_lte1.Id},lte1,,,,False,0,0001-01-01,"); // 組織端末アプリIDが不在

            var (response, body, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("(Row:3) The OrganizationClientAppId field is required.", (string)json["errors"]["OrganizationClientAppId"][0]);
        }

        /// <summary>
        /// 組織端末アプリIDがUUIDでないケース
        /// </summary>
        [Fact]
        public void Case17()
        {
            var device15 = new Device
            {
                Name = "device15",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device18 = new Device
            {
                Name = "device18",
                ManagedNumber = "2",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device15, device18);
            _context.SaveChanges();

            var obj = new StringBuilder();
            obj.AppendLine(CsvHeader);
            obj.AppendLine($"{device15.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,False,1,0001-01-01,");
            obj.AppendLine($"{device18.Id},domain1,deviceGroup1,device2,2,1001,type01,{_lte1.Id},lte1,a,,,False,0,0001-01-01,"); // 組織端末アプリIDがUUIDでない

            var (response, body, json) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Contains("(Row:3) Guid should contain 32 digits with 4 dashes", (string)json["errors"]["JinCreek App ID"][0]);
        }

        [Fact]
        public void CaseEx()
        {
            var device1 = new Device
            {
                Name = "device1",
                ManagedNumber = "1",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            var device2 = new Device
            {
                Name = "device2",
                ManagedNumber = "2",
                SerialNumber = "",
                ProductName = "",
                WindowsSignInListCacheDays = 0,
                Domain = _domain1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } },
            };
            _context.AddRange(device1, device2);
            _context.SaveChanges();

            // Use TpmがなくてもOK
            var obj = new StringBuilder();
            obj.AppendLine("ID,Domain,Device Group,Name,Managed Number,Serial Number,Product Name,LTE Module ID,LTE Module,JinCreek App ID,OS,JinCreek App Version,OS SignIn List Cache Days,Start Date,End Date");
            obj.AppendLine($"{device1.Id},domain1,deviceGroup1,device1,1,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,1,0001-01-01,");
            obj.AppendLine($"{device2.Id},domain1,deviceGroup1,device2,2,1001,type01,{_lte1.Id},lte1,{_organizationClientApp1.Id},,,0,0001-01-01,");

            var (response, body, _) = Utils.Post(_client, $"{Url}", Utils.CreateFormContent(obj.ToString(), "csv"), "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = JArray.Parse(body);
            Assert.Equal("1", list[0]["windowsSignInListCacheDays"]);
            Assert.Equal("0", list[1]["windowsSignInListCacheDays"]);
        }
    }
}
