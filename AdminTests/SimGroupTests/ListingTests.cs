using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimGroupTests
{
    /// <summary>
    /// SIMグループ一覧照会
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class ListingTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/sim-groups";
        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;

        public ListingTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();
            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Name = "", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user1", Name = "", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.SaveChanges();
        }

        /// <summary>
        /// ロール :  ユーザー管理者
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, body, _) = Utils.Get(_client, $"{Url}", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }
        /// <summary>
        /// 組織コード：不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["OrganizationCode"]);
            Assert.Equal("The OrganizationCode field is required.", json["errors"]?["OrganizationCode"].FirstOrDefault());
        }

        /// <summary>
        /// 組織コード：'数字以外の文字含む
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode=abcde1234", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["OrganizationCode"]);
            Assert.EndsWith("is not valid for OrganizationCode.", json["errors"]?["OrganizationCode"].First().ToString());
        }
        /// <summary>
        /// 組織：不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode=13465", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            var list = (JArray)json["results"];
            Assert.Empty(list);
        }

        /// <summary>
        /// 全部空 
        /// </summary>
        [Fact]
        public void Case05()
        {
            var simGroup1 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup01",
                Organization = _org1,
                Apn = "apn01",
                AuthenticationServerIp = "AuthServerIpAddress01",
                NasIp = "NasIpAddress01",
            };
            var simGroup2 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup02",
                Organization = _org1,
                Apn = "apn02",
                AuthenticationServerIp = "AuthServerIpAddress02",
                NasIp = "NasIpAddress02",
            };
            var simGroup3 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup03",
                Organization = _org1,
                Apn = "apn03",
                AuthenticationServerIp = "AuthServerIpAddress03",
                NasIp = "NasIpAddress03",
            };
            var simGroup4 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup04",
                Organization = _org2,
                Apn = "apn04",
                AuthenticationServerIp = "AuthServerIpAddress04",
                NasIp = "NasIpAddress04",
            };
            _context.AddRange(simGroup1, simGroup2, simGroup3, simGroup4);
            _context.SaveChanges();

            var (response, body, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, (int)json["count"]); // トータル件数
            Assert.NotEmpty(body);
            var list = (JArray)json["results"];
            Assert.Equal(3, list.Count);
            Assert.Equal("simGroup01", list[0]["name"]);　//SIMグループ名ソート:'昇順
            Assert.Equal("simGroup02", list[1]["name"]);　//SIMグループ名ソート:'昇順
            Assert.Equal("apn02", list[1]["apn"]);　//APNソート:'昇順
            Assert.Equal("apn03", list[2]["apn"]);　//APNソート:'昇順
            Assert.Null(list[0]["traceId"]);
        }

        /// <summary>
        /// ページ数 : 存在（あり得ないページ）
        /// </summary>
        [Fact]
        public void Case06()
        {
            var (response, body, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=0", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(body);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// ページ数 : '存在（最終ページ超過）
        /// </summary>
        [Fact]
        public void Case07()
        {
            var simGroup1 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup01",
                Organization = _org1,
                Apn = "apn01",
                AuthenticationServerIp = "AuthServerIpAddress01",
                NasIp = "NasIpAddress01",
            };
            _context.Add(simGroup1);
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=30", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, (int)json["count"]); // トータル件数
            Assert.Empty(list);
        }

        /// <summary>
        /// SIMグループフィルター: '存在 ページ数:'存在（初期ページ）1ページ当たり表示件数:'存在（デフォルト値以外）;SIMグループ名ソート:'昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            _context.Add(new SimGroup() // 組織 : '自組織 SIMグループ名:'フィルターに部分一致
            {
                Name = "simGroup01",
                Organization = _org1,
                Apn = "apn01",
                AuthenticationServerIp = "AuthServerIpAddress01",
                NasIp = "NasIpAddress01",
            });
            _context.Add(new SimGroup() // 組織 : '自組織 SIMグループ名:'フィルターに部分一致
            {
                Name = "simGroup02",
                Organization = _org1,
                Apn = "apn02",
                AuthenticationServerIp = "AuthServerIpAddress02",
                NasIp = "NasIpAddress02",
            });
            _context.Add(new SimGroup() // 組織 : '自組織 SIMグループ名:'フィルターに部分一致せず
            {
                Name = "aaaaaaa",
                Organization = _org1,
                Apn = "apn03",
                AuthenticationServerIp = "AuthServerIpAddress03",
                NasIp = "NasIpAddress03",
            });
            _context.Add(new SimGroup() // 組織 : '他組織 SIMグループ名:'フィルターに部分一致
            {
                Name = "simGroup04",
                Organization = _org2,
                Apn = "apn04",
                AuthenticationServerIp = "AuthServerIpAddress04",
                NasIp = "NasIpAddress04",
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=1&pageSize=3&Name=simGroup&sortBy=Name&orderBy=asc", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(list);
            Assert.Equal(2, (int)json["count"]); // トータル件数
            Assert.True(3 > list.Count); //'指定件数未満
            Assert.Equal("simGroup01", list[0]["name"]); //SIMグループ名ソート:'昇順
            Assert.Equal("simGroup02", list[1]["name"]);
        }

        /// <summary>
        /// APNフィルター: '存在; ページ数:''存在（中間ページ）;SIMグループ名ソート:'降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            for (int i = 1; i <= 47; i++)
            {
                _context.Add(new SimGroup() // 組織 : '自組織 ;APN:'フィルターに部分一致
                {
                    Name = $"simGroup{i:00}",
                    Organization = _org1,
                    Apn = $"apn{i:00}",
                    AuthenticationServerIp = $"AuthenticationServerIp{i:00}",
                    NasIp = $"NasIp{i:00}",
                });
            }
            _context.Add(new SimGroup() // 組織 : '自組織;APN:'フィルターに部分一致せず
            {
                Name = "simGroup002",
                Organization = _org1,
                Apn = "aaaaaaaaaaaa",
                AuthenticationServerIp = "AuthServerIpAddress002",
                NasIp = "NasIpAddress002",
            });
            _context.Add(new SimGroup() // 組織 : '他組織;APN:'フィルターに部分一致
            {
                Name = "simGroup002",
                Organization = _org2,
                Apn = "apn001",
                AuthenticationServerIp = "AuthServerIpAddress002",
                NasIp = "NasIpAddress002",
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=2&Apn=apn&sortBy=Name&orderBy=desc", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(47, (int)json["count"]); // トータル件数
            Assert.NotEmpty(list);
            Assert.Equal(20, list.Count);
            Assert.Equal("simGroup27", list[0]["name"]);
            Assert.Equal("simGroup26", list[1]["name"]); // SIMグループソート:'降順
        }

        public void SetData()
        {
            _context.Add(new SimGroup() // 組織 : '自組織;SIMグループ名:'フィルターに部分一致せず
            {
                Name = "aaaaaaaaa",
                Organization = _org1,
                Apn = "apn001",
                AuthenticationServerIp = "AuthServerIpAddress002",
                NasIp = "NasIpAddress002",
            });
            _context.Add(new SimGroup() // 組織 : '自組織;apn:'フィルターに部分一致せず
            {
                Name = "simGroup002",
                Organization = _org1,
                Apn = "aaaaaaaaaaaa",
                AuthenticationServerIp = "AuthServerIpAddress002",
                NasIp = "NasIpAddress002",
            });
            _context.Add(new SimGroup() // 組織 : '他組織;
            {
                Name = "simGroup002",
                Organization = _org2,
                Apn = "apn001",
                AuthenticationServerIp = "AuthServerIpAddress002",
                NasIp = "NasIpAddress002",
            });
        }

        /// <summary>
        /// 全部存在; ページ数:''存在（最終ページ）;APNソート:'昇順
        /// </summary>
        [Fact]
        public void Case10()
        {
            SetData();
            for (int i = 1; i <= 47; i++)
            {
                _context.Add(new SimGroup() //　全部正常
                {
                    Name = $"simGroup{i:00}",
                    Organization = _org1,
                    Apn = $"apn{i:00}",
                    AuthenticationServerIp = $"AuthenticationServerIp{i:00}",
                    NasIp = $"NasIp{i:00}",
                });
            }
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=3&Apn=apn&Name=simGroup&sortBy=Apn&orderBy=asc", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(47, (int)json["count"]); // トータル件数
            Assert.InRange(list.Count, 1, 20);
            Assert.Equal("apn41", list[0]["apn"]);
            Assert.Equal("apn42", list[1]["apn"]); // APNソート: '昇順
        }

        /// <summary>
        /// 全部不在; ページ数:''存在（中間ページ）;1ページ当たり表示件数:'存在（デフォルト値以外）;APNソート:'降順
        /// </summary>
        [Fact]
        public void Case11()
        {
            for (int i = 1; i <= 6; i++)
            {
                _context.Add(new SimGroup() //　全部正常
                {
                    Name = $"simGroup{i:00}",
                    Organization = _org1,
                    Apn = $"apn{i:00}",
                    AuthenticationServerIp = $"AuthenticationServerIp{i:00}",
                    NasIp = $"NasIp{i:00}",
                });
            }
            _context.Add(new SimGroup() // 組織 : '他組織;
            {
                Name = "simGroup002",
                Organization = _org2,
                Apn = "apn001",
                AuthenticationServerIp = "AuthServerIpAddress002",
                NasIp = "NasIpAddress002",
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=2&pageSize=2&sortBy=Apn&orderBy=desc", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(6, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count);
            Assert.Equal("apn04", list[0]["apn"]);
            Assert.Equal("apn03", list[1]["apn"]); // APNソート: '昇順
        }

        /// <summary>
        /// 全部存在; ページ数:存在（初期ページ））;NAS IPアドレスソート:'昇順
        /// </summary>
        [Fact]
        public void Case12()
        {
            SetData();
            for (int i = 1; i <= 47; i++)
            {
                _context.Add(new SimGroup() //　全部正常
                {
                    Name = $"simGroup{i:00}",
                    Organization = _org1,
                    Apn = $"apn{i:00}",
                    AuthenticationServerIp = $"AuthenticationServerIp{i:00}",
                    NasIp = $"NasIp{i:00}",
                });
            }
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=1&Apn=apn&Name=simGroup&sortBy=NasIp&orderBy=asc", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(47, (int)json["count"]); // トータル件数
            Assert.Equal(20, list.Count);
            Assert.Equal("NasIp01", list[0]["nasIp"]);
            Assert.Equal("NasIp02", list[1]["nasIp"]); // NAS IPアドレスソート:'昇順
        }

        /// <summary>
        /// 全部存在; ページ数:存在（中間ページ））;NAS IPアドレスソート:'降順
        /// </summary>
        [Fact]
        public void Case13()
        {
            SetData();
            for (int i = 1; i <= 47; i++)
            {
                _context.Add(new SimGroup() //　全部正常
                {
                    Name = $"simGroup{i:00}",
                    Organization = _org1,
                    Apn = $"apn{i:00}",
                    AuthenticationServerIp = $"AuthenticationServerIp{i:00}",
                    NasIp = $"NasIp{i:00}",
                });
            }
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=2&Apn=apn&Name=simGroup&sortBy=NasIp&orderBy=desc", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(47, (int)json["count"]); // トータル件数
            Assert.Equal(20, list.Count);
            Assert.Equal("NasIp27", list[0]["nasIp"]);
            Assert.Equal("NasIp26", list[1]["nasIp"]);// NAS IPアドレスソート:'降順
        }

        /// <summary>
        /// 全部存在; ページ数:存在（最終ページ）；1ページ当たり表示件数:'存在（デフォルト値以外）;認証サーバー IPアドレスソート：'昇順
        /// </summary>
        [Fact]
        public void Case14()
        {
            for (int i = 1; i <= 8; i++)
            {
                _context.Add(new SimGroup() //　全部正常
                {
                    Name = $"simGroup{i:00}",
                    Organization = _org1,
                    Apn = $"apn{i:00}",
                    AuthenticationServerIp = $"AuthenticationServerIp{i:00}",
                    NasIp = $"NasIp{i:00}",
                });
            }
            SetData();
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=3&pageSize=3&Apn=apn&Name=simGroup&sortBy=AuthenticationServerIp&orderBy=asc", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(8, (int)json["count"]); // トータル件数
            Assert.True(3 > list.Count); //'指定件数未満
            Assert.Equal("AuthenticationServerIp07", list[0]["authenticationServerIp"]);
            Assert.Equal("AuthenticationServerIp08", list[1]["authenticationServerIp"]);// 認証サーバー IPアドレスソート：'昇順
        }

        /// <summary>
        /// 全部存在; ページ数:存在（中間ページ）；1ページ当たり表示件数:'存在（デフォルト値以外）;認証サーバー IPアドレスソート：'降順
        /// </summary>
        [Fact]
        public void Case15()
        {
            for (int i = 1; i <= 6; i++)
            {
                _context.Add(new SimGroup() //　全部正常
                {
                    Name = $"simGroup{i:00}",
                    Organization = _org1,
                    Apn = $"apn{i:00}",
                    AuthenticationServerIp = $"AuthenticationServerIp{i:00}",
                    NasIp = $"NasIp{i:00}",
                });
            }
            SetData();
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=2&pageSize=2&Apn=apn&Name=simGroup&sortBy=AuthenticationServerIp&orderBy=desc", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(6, (int)json["count"]); // トータル件数
            Assert.Equal(2, list.Count);
            Assert.Equal("AuthenticationServerIp04", list[0]["authenticationServerIp"]);
            Assert.Equal("AuthenticationServerIp03", list[1]["authenticationServerIp"]); // 認証サーバー IPアドレスソート：'降順
        }

        /// <summary>
        /// 全部存在; ページ数:存在（初期ページ）;
        /// </summary>
        [Fact]
        public void Case16()
        {
            SetData(); // 異常データ
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?OrganizationCode={_org1.Code}&page=1&Apn=apn&Name=simGroup", "user0", "user0"); // スーパー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, (int)json["count"]); // トータル件数
            Assert.Empty(list);
        }

    }
}
