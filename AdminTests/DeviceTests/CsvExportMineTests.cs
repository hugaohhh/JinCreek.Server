using CsvHelper;
using JinCreek.Server.Admin.Controllers;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.DeviceTests
{
    /// <summary>
    /// 自分端末一覧エクスポート
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class CsvExportMineTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/devices/mine/csv";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain1a;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly DeviceGroup _deviceGroup1;
        private readonly DeviceGroup _deviceGroup2;
        private readonly Device _device1;
        private readonly Device _device1B;
        private readonly Device _device1C;
        private readonly Device _device2;
        private readonly LteModule _lte1;

        public CsvExportMineTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain1a = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain03", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup1", Domain = _domain1 });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup2", Domain = _domain2 });
            _context.Add(_deviceGroup1 = new DeviceGroup { Id = Guid.NewGuid(), Name = "deviceGroup1", Domain = _domain1 });
            _context.Add(_deviceGroup2 = new DeviceGroup { Id = Guid.NewGuid(), Name = "deviceGroup2", Domain = _domain2 });
            _context.Add(_device1 = new Device { Id = Guid.NewGuid(), Name = "device1", ManagedNumber = "1", Domain = _domain1, DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1} } });
            _context.Add(_device1B = new Device { Id = Guid.NewGuid(), Name = "device1b", ManagedNumber = "2", Domain = _domain1, DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1} } });
            _context.Add(_device1C = new Device { Id = Guid.NewGuid(), Name = "device1c", ManagedNumber = "2", Domain = _domain1a, DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1} } });
            _context.Add(_device2 = new Device { Id = Guid.NewGuid(), Name = "device2", ManagedNumber = "1", Domain = _domain2, DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup2} } });
            _context.Add(_lte1 = new LteModule { Id = Guid.NewGuid(), Name = "lte1" });
            _context.Add(new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.SaveChanges();
        }

        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var stringReader = new StringReader(body);
            var csvReader = new CsvReader(stringReader, CultureInfo.InvariantCulture);
            csvReader.Configuration.RegisterClassMap<DevicesController.CsvRecordMap>();
            var devices = csvReader.GetRecords<DevicesController.CsvRecord>().ToList();

            Assert.Equal("device1", devices[0].Name);
            Assert.Equal("device1b", devices[1].Name); // 名前ソート:昇順
            Assert.Equal("domain01", devices[1].DomainName);
            Assert.Equal("domain02", devices[2].DomainName); // ドメイン名ソート:昇順
        }

    }
}
