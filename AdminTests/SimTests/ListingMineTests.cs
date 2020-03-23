using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimTests
{
    /// <summary>
    /// 自分SIM一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class ListingMineTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/sims/mine";
        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup1a;
        private readonly SimGroup _simGroup2;


        public ListingMineTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_simGroup1 = new SimGroup() { Id = Guid.NewGuid(), Name = "simGroup1", Organization = _org1 });
            _context.Add(_simGroup2 = new SimGroup
            { Id = Guid.NewGuid(), Name = "simGroup2", Organization = _org2, IsolatedNw1IpPool = "" });
            _context.Add(_simGroup1a = new SimGroup
            { Id = Guid.NewGuid(), Name = "simGroup1a", Organization = _org1 });
            _context.SaveChanges();
        }

        /// <summary>
        /// 全部空 
        /// </summary>
        [Fact]
        public void Case01()
        {
            _context.Add(new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn02",
                Imsi = "imsi02",
                IccId = "iccid02",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1a
            });
            _context.Add(new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn03",
                Imsi = "imsi03",
                IccId = "iccid03",
                UserName = "sim03",
                Password = "password",
                SimGroup = _simGroup1a
            });
            _context.Add(new Sim() // 組織 : '他組織
            {
                Msisdn = "msisdn04",
                Imsi = "imsi04",
                IccId = "iccid04",
                UserName = "sim04",
                Password = "password",
                SimGroup = _simGroup2
            });
            _context.SaveChanges();

            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotEmpty(body);
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(3, list.Count);
            Assert.Equal(3, (int)json["count"]);
            Assert.Equal("simGroup1", list[0]["simGroup"]["name"]);　//SIMグループソート:'昇順
            Assert.Equal("simGroup1a", list[1]["simGroup"]["name"]);
            Assert.Equal("msisdn02", list[1]["msisdn"]); //MSISDNソート:'昇順
            Assert.Equal("msisdn03", list[2]["msisdn"]);
            Assert.Null(list[0]["traceId"]);
        }

        /// <summary>
        /// ページ数 : 存在（あり得ないページ）
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?page=0", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotEmpty(body);
            var list = JObject.Parse(body);
            Assert.NotNull(list["traceId"]);
            Assert.NotNull(list["errors"]?["Page"]);
        }

        /// <summary>
        /// ページ数 : '存在（最終ページ超過）
        /// </summary>
        [Fact]
        public void Case03()
        {
            _context.Add(new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?page=30", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(1, (int)json["count"]);
            Assert.Empty(list);
        }

        /// <summary>
        /// SIMグループフィルター: '存在 ページ数:'存在（初期ページ）;SIMグループソート:'昇順
        /// </summary>
        [Fact]
        public void Case04()
        {
            _context.Add(new Sim() // 組織 : '自組織 SIMグループ:'フィルターに合致
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(new Sim() // 組織 : '自組織 SIMグループ:'フィルターに合致
            {
                Msisdn = "msisdn04",
                Imsi = "imsi04",
                IccId = "iccid04",
                UserName = "sim04",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(new Sim() // 組織 : '自組織 SIMグループ:'フィルターに合致せず
            {
                Msisdn = "msisdn02",
                Imsi = "imsi02",
                IccId = "iccid02",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1a
            });
            _context.Add(new Sim() // 組織 : '他組織 SIMグループ:'フィルターに合致
            {
                Msisdn = "msisdn03",
                Imsi = "imsi03",
                IccId = "iccid03",
                UserName = "sim03",
                Password = "password",
                SimGroup = _simGroup2
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?page=1&SimGroupId={_simGroup1.Id}&sortBy=SimGroupName&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(2, (int)json["count"]);
            Assert.Equal("simGroup1", list[0]["simGroup"]["name"]); // SIMグループソート:'昇順
            Assert.Equal("simGroup1", list[1]["simGroup"]["name"]);
        }

        /// <summary>
        /// MSISDNフィルター : '存在; ページ数:''存在（中間ページ）;1ページ当たり表示件数:'存在（デフォルト値以外）SIMグループソート:'降順
        /// </summary>
        [Fact]
        public void Case05()
        {
            for (int i = 1; i <= 6; i++)
            {
                _context.Add(new Sim() // 組織 : '自組織 ;MSISDN:'フィルターに部分一致
                {
                    Msisdn = $"msisdn{i:00}",
                    Imsi = $"imsi{i:00}",
                    IccId = $"iccid{i:00}",
                    UserName = $"sim{i:00}",
                    Password = "password",
                    SimGroup = _simGroup1
                });
            }
            _context.Add(new Sim() // 組織 : '自組織;MSISDN:'フィルターに部分一致せず
            {
                Msisdn = "123456",
                Imsi = "imsi07",
                IccId = "iccid07",
                UserName = "sim07",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(new Sim() // 組織 : '他組織;MSISDN:'フィルターに部分一致
            {
                Msisdn = "msisdn08",
                Imsi = "imsi08",
                IccId = "iccid08",
                UserName = "sim08",
                Password = "password",
                SimGroup = _simGroup2
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?page=2&pageSize=2&Msisdn=msisdn&sortBy=SimGroupName&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(6, (int)json["count"]);
            Assert.Equal("simGroup1", list[0]["simGroup"]["name"]);
            Assert.Equal("simGroup1", list[1]["simGroup"]["name"]); // SIMグループソート:'降順
        }

        /// <summary>
        /// ユーザー名フィルター : '存在; ページ数:''存在（最終ページ）;1ページ当たり表示件数:'存在（デフォルト値以外）;MSISDNソート:'昇順
        /// </summary>
        [Fact]
        public void Case06()
        {
            for (int i = 1; i <= 8; i++)
            {
                _context.Add(new Sim() // 組織 : '自組織 ;ユーザー名:'フィルターに部分一致
                {
                    Msisdn = $"msisdn{i:00}",
                    Imsi = $"imsi{i:00}",
                    IccId = $"iccid{i:00}",
                    UserName = $"sim{i:00}",
                    Password = "password",
                    SimGroup = _simGroup1
                });
            }
            _context.Add(new Sim() // 組織 : '自組織;ユーザー名:'フィルターに部分一致せず
            {
                Msisdn = "msisdn07",
                Imsi = "imsi07",
                IccId = "iccid07",
                UserName = "abcd",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(new Sim() // 組織 : '他組織;ユーザー名:'フィルターに部分一致
            {
                Msisdn = "msisdn08",
                Imsi = "imsi08",
                IccId = "iccid08",
                UserName = "sim08",
                Password = "password",
                SimGroup = _simGroup2
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?page=3&pageSize=3&UserName=sim&sortBy=Msisdn&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(3 > list.Count); // '指定件数未満
            Assert.Equal(8, (int)json["count"]);
            Assert.Equal("msisdn07", list[0]["msisdn"]);
            Assert.Equal("msisdn08", list[1]["msisdn"]); // MSISDNソート: '昇順
        }


        public void SetUpData()
        {
            _context.Add(new Sim() // 組織 : '自組織;SIMグループ:'フィルターに合致せず
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup2
            });
            _context.Add(new Sim() // 組織 : '自組織;MSISDN:''フィルターに部分一致せず
            {
                Msisdn = "abcd",
                Imsi = "imsi02",
                IccId = "iccid02",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(new Sim() // 組織 : '自組織;ユーザー名:''フィルターに部分一致せず
            {
                Msisdn = "msisdn03",
                Imsi = "imsi03",
                IccId = "iccid03",
                UserName = "abcd",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// 全部存在; ページ数:''存在（中間ページ）;MSISDNソート:'降順
        /// </summary>
        [Fact]
        public void Case07()
        {
            SetUpData();
            for (int i = 4; i < 67; i++)
            {
                _context.Add(new Sim() // 全部正常
                {
                    Msisdn = $"msisdn{i:00}",
                    Imsi = $"imsi{i:00}",
                    IccId = $"iccid{i:00}",
                    UserName = $"sim{i:00}",
                    Password = "password",
                    SimGroup = _simGroup1
                });
            }
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?page=2&Msisdn=msisdn&UserName=sim&SimGroupId={_simGroup1.Id}&sortBy=Msisdn&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(20, list.Count);
            Assert.Equal(63, (int)json["count"]);
            Assert.Equal("msisdn46", list[0]["msisdn"]);
            Assert.Equal("msisdn45", list[1]["msisdn"]); // MSISDNソート:'降順

        }

        /// <summary>
        /// 全部存在; ページ数:存在（初期ページ））;1ページ当たり表示件数:'存在（デフォルト値以外）;ユーザー名ソート:'昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            SetUpData();
            _context.Add(new Sim() // 全部正常
            {
                Msisdn = "msisdn04",
                Imsi = "imsi04",
                IccId = "iccid04",
                UserName = "sim04",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(new Sim() // 全部正常
            {
                Msisdn = "msisdn05",
                Imsi = "imsi05",
                IccId = "iccid05",
                UserName = "sim05",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?page=1&pageSize=3&Msisdn=msisdn&UserName=sim&SimGroupId={_simGroup1.Id}&sortBy=UserName&orderBy=asc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(3 > list.Count);// '指定件数未満
            Assert.Equal(2, (int)json["count"]);
            Assert.Equal("sim04", list[0]["userName"]);
            Assert.Equal("sim05", list[1]["userName"]); // ユーザー名ソート:'昇順

        }

        /// <summary>
        /// 全部存在; ページ数:'存在（最終ページ））;1ページ当たり表示件数:'存在（デフォルト値以外）;ユーザー名ソート:'降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            SetUpData();
            for (int i = 4; i <= 9; i++)
            {
                _context.Add(new Sim() // 全部正常
                {
                    Msisdn = $"msisdn{i:00}",
                    Imsi = $"imsi{i:00}",
                    IccId = $"iccid{i:00}",
                    UserName = $"sim{i:00}",
                    Password = "password",
                    SimGroup = _simGroup1
                });
            }
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=3&PageSize=2&Msisdn=msisdn&UserName=sim&SimGroupId={_simGroup1.Id}&sortBy=UserName&orderBy=desc", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(2, list.Count);
            Assert.Equal(6, (int)json["count"]);
            Assert.Equal("sim05", list[0]["userName"]);
            Assert.Equal("sim04", list[1]["userName"]); // ユーザー名ソート:'降順

        }

        /// <summary>
        /// 全部存在; ページ数:'存在（初期ページ）
        /// </summary>
        [Fact]
        public void Case10()
        {
            SetUpData();
            _context.SaveChanges();
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}/?page=1&Msisdn=msisdn&UserName=sim&SimGroupId={_simGroup1.Id}", token);
            var body = result.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(0, (int)json["count"]);
            Assert.Empty(list);
        }

    }
}
