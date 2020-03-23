using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.DeviceTests
{
    /// <summary>
    /// 端末一覧照会
    /// </summary>
    [Collection("Sequential")]
    public class ListingMineTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/devices/mine";
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly Organization _org1;
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
        private readonly Organization _org2;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly Domain _domain1;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private readonly Domain _domain1a;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")] private readonly Domain _domain2;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")] private readonly SuperAdmin _user1;
        [SuppressMessage("ReSharper", "NotAccessedField.Local")] private readonly EndUser _user2;
        private readonly DeviceGroup _deviceGroup1;
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly DeviceGroup _deviceGroup1a;
        private readonly DeviceGroup _deviceGroup2;
        private readonly LteModule _lte1;
        private readonly LteModule _lte2;

        public ListingMineTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain1a = new Domain { Id = Guid.NewGuid(), Name = "domain01a", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_deviceGroup1 = new DeviceGroup() { Id = Guid.NewGuid(), Name = "_deviceGroup1", Domain = _domain1 });
            _context.Add(_deviceGroup2 = new DeviceGroup
            { Id = Guid.NewGuid(), Name = "DeviceGroup2", Domain = _domain2 });
            _context.Add(_deviceGroup1a = new DeviceGroup
            { Id = Guid.NewGuid(), Name = "_deviceGroup1a", Domain = _domain1a });
            _context.Add(_lte1 = new LteModule { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.Add(_lte2 = new LteModule { Name = "lte2", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.SaveChanges();
        }

        /// <summary>
        /// 全部空 
        /// </summary>
        [Fact]
        public void Case01()
        {
            _context.Add(new Device() // 組織 : '自組織
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device01",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 組織 : '自組織
            {
                LteModule = _lte1,
                Domain = _domain1a,
                Name = "device03",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 組織 : '他組織
            {
                LteModule = _lte1,
                Domain = _domain2,
                Name = "device02",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup2 } }
            });
            _context.SaveChanges();

            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotEmpty(body);
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(2, list.Count);
            Assert.Equal(2, (int)json["count"]);
            Assert.Equal("domain01", list[0]["domain"]["name"]);
            Assert.Equal("domain01a", list[1]["domain"]["name"]); //ドメイン名ソート:'昇順
            Assert.Null(list[0]["traceId"]);
        }

        /// <summary>
        /// ページ数 : 存在（あり得ないページ）
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=0", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotEmpty(body);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// ページ数 : '存在（最終ページ超過）
        /// </summary>
        [Fact]
        public void Case03()
        {
            _context.Add(new Device() // 組織 : '自組織
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device01",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=30", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Empty(list);
            Assert.Equal(1, (int)json["count"]);
        }

        /// <summary>
        /// ドメインフィルター : '存在 ページ数:'存在（初期ページ）;ドメイン名ソート:'昇順
        /// </summary>
        [Fact]
        public void Case04()
        {
            _context.Add(new Device() // 組織 : '自組織 ;ドメイン:'フィルターに合致
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device01",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 組織 : '自組織 ;ドメイン:'フィルターに合致
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device03",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 組織 : '自組織;ドメイン:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1a,
                Name = "device02",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=1&domainId={_domain1.Id}&sortBy=Name&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(2, (int)json["count"]);
            Assert.Equal("domain01", list[0]["domain"]["name"]); // ドメイン名ソート:'昇順
            Assert.Equal("domain01", list[1]["domain"]["name"]);
        }

        /// <summary>
        /// 端末グループフィルター : '存在; ページ数:''存在（中間ページ）;1ページ当たり表示件数:'存在（デフォルト値以外）ドメイン名ソート:'降順
        /// </summary>
        [Fact]
        public void Case05()
        {
            for (int i = 1; i <= 6; i++)
            {
                _context.Add(new Device() // 全部正常
                {
                    LteModule = _lte1,
                    Domain = new Domain()
                    {
                        Name = $"domain{i:00}",
                        Organization = _org1
                    },
                    Name = $"device{i:00}",
                    ProductName = "type01",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
                });
            }
            _context.Add(new Device() // 組織 : '自組織;端末グループ:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "abcd",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup2 } }
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=2&pageSize=2&DeviceGroupId={_deviceGroup1.Id}&sortBy=domainName&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(6, (int)json["count"]);
            Assert.Equal("domain04", list[0]["domain"]["name"]);
            Assert.Equal("domain03", list[1]["domain"]["name"]); // ドメイン名ソート:'降順
        }

        /// <summary>
        /// 名前フィルター : '存在; ページ数:''存在（最終ページ）;1ページ当たり表示件数:'存在（デフォルト値以外）;名前ソート:'昇順
        /// </summary>
        [Fact]
        public void Case06()
        {
            for (int i = 1; i <= 8; i++)
            {
                _context.Add(new Device() // 組織 : '自組織 ;名前フィルター:'フィルターに部分一致
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = $"device{i:00}",
                    ProductName = "type01",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
                });
            }

            _context.Add(new Device() // 組織 : '自組織;名前フィルター:'フィルターに部分一致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "abc",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=3&pageSize=3&name=device&sortBy=Name&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(3 > list.Count); // '指定件数未満
            Assert.Equal(8, (int)json["count"]);
            Assert.Equal("device07", list[0]["name"]);
            Assert.Equal("device08", list[1]["name"]); // 名前ソート: '昇順
        }

        /// <summary>
        /// 機種名フィルター : '存在; ページ数:''存在（中間ページ）;名前ソート:'降順
        /// </summary>
        [Fact]
        public void Case07()
        {
            for (int i = 1; i <= 42; i++)
            {
                _context.Add(new Device() // 組織 : '自組織 ;機種名フィルター:'フィルターに部分一致
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = $"device{i:00}",
                    ProductName = "type01",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
                });
            }
            _context.Add(new Device() // 組織 : '自組織;機種名フィルター:フィルターに部分一致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "abcd",
                ProductName = "abcd",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=2&ProductName=type&sortBy=Name&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(20, list.Count);
            Assert.Equal(42, (int)json["count"]);
            Assert.Equal("device22", list[0]["name"]);
            Assert.Equal("device21", list[1]["name"]); // 名前ソート:'降順
            foreach (var device in list)
            {
                Assert.NotEqual("abcd", device["name"]);
            }
        }

        /// <summary>
        /// LTEモジュールフィルター : '存在; ページ数:''存在（最終ページ）;機種名ソート:'昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            for (int i = 1; i <= 42; i++)
            {
                _context.Add(new Device() // 組織 : '自組織 ;LTEモジュールフィルター:'フィルターに合致
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = $"device{i:00}",
                    ProductName = $"type{i:00}",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
                });
            }
            _context.Add(new Device() // 組織 : '自組織;LTEモジュールフィルター:'フィルターに合致せず
            {
                LteModule = _lte2,
                Domain = _domain1,
                Name = "device43",
                ProductName = "abc",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=3&lteModuleId={_lte1.Id}&sortBy=ProductName&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.InRange(list.Count, 1, 20);
            Assert.Equal(42, (int)json["count"]);
            Assert.Equal("type41", list[0]["productName"]);
            Assert.Equal("type42", list[1]["productName"]); // 機種名ソート:'昇順
            foreach (var device in list)
            {
                Assert.NotEqual("abcd", device["name"]);
            }
        }

        public void SetUpData()
        {
            _context.Add(new Device() // 13: 組織 : '自組織;ドメイン:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1a,
                Name = "device01",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 14: 組織 : '自組織;端末グループ:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device02",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup2 } }
            });
            _context.Add(new Device() // 15: 組織 : '自組織;名前:''フィルターに部分一致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "abcd",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 16: 組織 : '自組織;機種名:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device04",
                ProductName = "abcd",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 利用開始日範囲開始フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = $"device06",
                ProductName = $"type",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-05"),
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 利用開始日範囲終了:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device07",
                ProductName = "type02",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-09"),
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.Add(new Device() // 利用終了日範囲開始:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = $"device08",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse($"2020-02-01"),
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>
                    {new DeviceGroupDevice {DeviceGroup = _deviceGroup1}}
            });
            _context.Add(new Device() // 利用終了日範囲終了:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = $"device09",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse($"2020-02-07"),
                EndDate = DateTime.Parse($"2020-03-01"),
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>
                    {new DeviceGroupDevice {DeviceGroup = _deviceGroup1}}
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// 利用開始日範囲開始フィルター; ページ数:''存在（最終ページ）;機種名ソート:'降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            for (int i = 1; i <= 42; i++)
            {
                _context.Add(new Device() // '利用開始日範囲開始フィルターに合致
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = $"device{i:00}",
                    ProductName = $"type{i:00}",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    StartDate = DateTime.Parse("2020-02-07"),
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
                });
            }
            _context.Add(new Device() // 利用開始日範囲開始フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = $"device",
                ProductName = $"type",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-05"),
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=3&startDateFrom=2020-02-06&sortBy=ProductName&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.InRange(list.Count, 1, 20);
            Assert.Equal(42, (int)json["count"]);
            Assert.Equal("type02", list[0]["productName"]);
            Assert.Equal("type01", list[1]["productName"]); // 機種名ソート:'降順
        }

        /// <summary>
        /// 利用開始日範囲終了フィルター:存在; ページ数:存在（初期ページ））;1ページ当たり表示件数:'存在（デフォルト値以外）;利用開始日ソート:'昇順
        /// </summary>
        [Fact]
        public void Case10()
        {
            for (int i = 1; i <= 6; i++)
            {
                _context.Add(new Device() //利用開始日範囲終了:'フィルターに合致
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = "device",
                    ProductName = "type01",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    StartDate = DateTime.Parse($"2020-02-{i:00}"),
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice>
                        {new DeviceGroupDevice {DeviceGroup = _deviceGroup1}}
                });
            }
            _context.Add(new Device() // 利用開始日範囲終了:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device06",
                ProductName = "type02",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-09"),
                DeviceGroupDevices = new HashSet<DeviceGroupDevice> { new DeviceGroupDevice { DeviceGroup = _deviceGroup1 } }
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=1&pageSize=2&startDateTo=2020-02-08&sortBy=StartDate&orderBy=asc", "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, list.Count);
            Assert.Equal(6, (int)json["count"]);
            Assert.Equal("2020-02-01", (string)list[0]["startDate"]);
            Assert.Equal("2020-02-02", (string)list[1]["startDate"]); // 利用開始日ソート:'昇順
        }

        /// <summary>
        /// 利用終了日範囲開始フィルター 存在; ページ数:'存在（中間ページ））;1ページ当たり表示件数:'存在（デフォルト値以外）;利用開始日ソート:'降順
        /// </summary>
        [Fact]
        public void Case11()
        {
            for (int i = 7; i <= 13; i++)
            {
                _context.Add(new Device() // 利用終了日範囲開始:'フィルターに合致
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = $"device{i:00}",
                    ProductName = "type01",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    StartDate = DateTime.Parse($"2020-02-{i:00}"),
                    EndDate = DateTime.Parse($"2020-02-20"),
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice>
                        {new DeviceGroupDevice {DeviceGroup = _deviceGroup1}}
                });
            }
            _context.Add(new Device() // 利用終了日範囲開始:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = $"device",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-01"),
                EndDate = DateTime.Parse($"2020-02-01"),
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>
                    {new DeviceGroupDevice {DeviceGroup = _deviceGroup1}}
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=2&pageSize=2&endDateFrom=2020-02-06&sortBy=StartDate&orderBy=desc", "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, list.Count);
            Assert.Equal(7, (int)json["count"]);
            Assert.Equal("2020-02-11", (string)list[0]["startDate"]);
            Assert.Equal("2020-02-10", (string)list[1]["startDate"]); // 利用開始日ソート:'降順
        }

        /// <summary>
        /// 利用終了日範囲終了フィルター 存在; ページ数:'存在（最終ページ） 利用終了日ソート:'昇順
        /// </summary>
        [Fact]
        public void Case12()
        {
            for (int i = 1; i <= 42; i++)
            {
                _context.Add(new Device() // 利用終了日範囲終了:'フィルターに合致
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = $"device{i:00}",
                    ProductName = "type01",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    StartDate = DateTime.Parse("2020-02-07"),
                    EndDate = DateTime.Parse("2021-01-01").AddDays(i), // 2021-01-02～2021-02-12
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice>
                        {new DeviceGroupDevice {DeviceGroup = _deviceGroup1}}
                });
            }
            _context.Add(new Device() // 利用終了日範囲終了:'フィルターに合致せず
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = $"device",
                ProductName = "type01",
                UseTpm = true,
                SerialNumber = "000111222",
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Parse("2021-02-13"),
                DeviceGroupDevices = new HashSet<DeviceGroupDevice>
                    {new DeviceGroupDevice {DeviceGroup = _deviceGroup1}}
            });
            _context.SaveChanges();

            var query = new
            {
                organizationCode = _org1.Code,
                page = 1,
                endDateTo = "2021-02-12",
                sortBy = "EndDate",
                orderBy = "asc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, _domain1.Name); //   ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(20, list.Count);
            Assert.Equal(42, (int)json["count"]);
            Assert.Equal("2021-01-02", (string)list[0]["endDate"]);
            Assert.Equal("2021-01-03", (string)list[1]["endDate"]); //利用終了日ソート:'昇順
        }

        /// <summary>
        /// 全部 存在; ページ数:'存在（中間ページ）） 利用終了日ソート:'降順
        /// </summary>
        [Fact]
        public void Case13()
        {
            SetUpData();
            for (int i = 10; i <= 52; i++)
            {
                _context.Add(new Device() // 正常
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = $"device{i:00}",
                    ProductName = "type01",
                    UseTpm = true,
                    SerialNumber = "000111222",
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                    StartDate = DateTime.Parse("2020-02-07"),
                    EndDate = DateTime.Parse("2021-01-01").AddDays(i), // 2021-01-11～2021-02-22
                    DeviceGroupDevices = new HashSet<DeviceGroupDevice>
                        {new DeviceGroupDevice {DeviceGroup = _deviceGroup1}}
                });
            }
            _context.SaveChanges();

            var query = new
            {
                organizationCode = _org1.Code,
                page = 2,
                productName = "type",
                name = "device",
                lteModuleId = _lte1.Id,
                deviceGroupId = _deviceGroup1.Id,
                domainId = _domain1.Id,
                startDateFrom = "2020-02-06",
                startDateTo = "2020-02-08",
                endDateFrom = "2021-01-11",
                endDateTo = "2021-02-22",
                sortBy = "EndDate",
                orderBy = "desc",
            };
            var (response, _, json) = Utils.Get(_client, Utils.AddQueryString(Url, query), "user1", "user1", 1, _domain1.Name); //   ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(20, list.Count);
            Assert.Equal(43, (int)json["count"]);
            Assert.Equal("2021-02-02", (string)list[0]["endDate"]);
            Assert.Equal("2021-02-01", (string)list[1]["endDate"]); //利用終了日ソート:'降順
        }

        /// <summary>
        /// 全部 存在;  利用終了日ソート:'降順
        /// </summary>
        [Fact]
        public void Case14()
        {
            SetUpData();
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=1&ProductName=type&Name=device&lteModuleId={_lte1.Id}&DeviceGroupId={_deviceGroup1.Id}&DomainId={_domain1.Id}&startDateFrom=2020-02-06&startDateTo=2020-02-08&endDateFrom=2020-02-06&endDateTo=2020-02-28&sortBy=EndDate&orderBy=desc", "user1", "user1",1,_domain1.Name); //   ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(list);
        }
    }
}
