using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimAndDevicesTests
{
    /// <summary>
    /// 自分SIM端末一覧照会 
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

        private const string Url = "/api/sim-and-devices/mine";
        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroupEndUser _userGroupEndUser;
        private readonly AvailablePeriod _availablePeriod;
        private readonly SuperAdmin _user1;
        private readonly EndUser _user2;
        private readonly DeviceGroup _deviceGroup1;
        private readonly LteModule _lte1;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup2;
        private readonly Device _device1;
        private readonly Device _device2;
        private readonly Sim _sim1;
        private readonly Sim _sim1a;
        private readonly Sim _sim2;

        public ListingMineTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain02", Organization = _org2 });
            _context.Add(_user1 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user2 = new UserAdmin() { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
            _context.Add(_deviceGroup1 = new DeviceGroup() { Id = Guid.NewGuid(), Name = "_deviceGroup1", Domain = _domain1 });
            _context.Add(_lte1 = new LteModule() { Name = "lte1", UseSoftwareRadioState = true, NwAdapterName = "abc" });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Domain = _domain1, Name = "userGroup1" });
            _context.Add(_userGroupEndUser = new UserGroupEndUser() { EndUser = _user2, UserGroup = _userGroup1 });
            _context.Add(_availablePeriod = new AvailablePeriod()
            { EndUser = _user2, EndDate = null, StartDate = DateTime.Today });
            _context.Add(_simGroup1 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup1",
                Organization = _org1,
                Apn = "apn",
                AuthenticationServerIp = "AuthServerIpAddress",
                IsolatedNw1IpPool = "Nw1IpAddressPool",
                IsolatedNw1SecondaryDns = "Nw1SecondaryDns",
                IsolatedNw1IpRange = "Nw1IpAddressRange",
                IsolatedNw1PrimaryDns = "Nw1PrimaryDns",
                NasIp = "NasIpAddress",
                UserNameSuffix = "UserNameSuffix",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns"
            });
            _context.Add(_simGroup2 = new SimGroup
                { Id = Guid.NewGuid(), Name = "simGroup2", Organization = _org2, IsolatedNw1IpPool = "" });
            _context.Add(_sim1 = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_sim1a = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01a",
                Imsi = "imsi01a",
                IccId = "iccid01a",
                UserName = "sim01a",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_sim2 = new Sim() // 組織 : '他組織
            {
                Msisdn = "msisdn02",
                Imsi = "imsi02",
                IccId = "iccid02",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup2
            });
            _context.Add(_device1 = new Device()
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
            _context.Add(_device2 = new Device()
            {
                LteModule = _lte1,
                Domain = _domain1,
                Name = "device02",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// 全部空 
        /// </summary>
        [Fact]
        public void Case01()
        {
            var simDevice1 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1a,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            };
            var simDevice3 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1a,
                Device = _device2,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            };
            var simDevice5 = new SimAndDevice() // 組織 : '他組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            };
            _context.AddRange(simDevice5, simDevice1, simDevice2, simDevice3);
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = (JArray)json["results"];
            Assert.Equal(3, list.Count);
            Assert.Equal(3, (int)json["count"]);
            Assert.Equal("msisdn01", list[0]["sim"]["msisdn"]);
            Assert.Equal("msisdn01a", list[1]["sim"]["msisdn"]); //SIM(MSISDN)ソート:'昇順
            Assert.Equal("device01", list[1]["device"]["name"]);
            Assert.Equal("device02", list[2]["device"]["name"]); // 端末ソート:'昇順
        }

        /// <summary>
        /// ページ数 : 存在（あり得ないページ）
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=0", "user1", "user1", 1, "domain01"); // ユーザー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.NotNull(json["errors"]?["Page"]);
        }

        /// <summary>
        /// ページ数 : '存在（最終ページ超過）
        /// </summary>
        [Fact]
        public void Case03()
        {
            var simDevice1 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            };
            _context.Add(simDevice1);
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=30", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(list);
        }

        /// <summary>
        /// SIM(MSISDN)フィルター : '存在 ページ数:'存在（初期ページ）;SIM(MSISDN)ソート:'昇順
        /// </summary>
        [Fact]
        public void Case04()
        {
            var simDevice1 = new SimAndDevice() // 組織 : '自組織 SIM(MSISDN):'フィルターに合致
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            };
            var simDevice4 = new SimAndDevice() // 組織 : '自組織 SIM(MSISDN):'フィルターに合致
            {
                Sim = _sim1a,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            };
            var simDevice2 = new SimAndDevice() // 組織 : '自組織 SIM(MSISDN):'フィルターに合致せず
            {
                Sim = new Sim() // 組織 : '自組織
                {
                    Msisdn = "abcdfe",
                    Imsi = "12",
                    IccId = "11111",
                    UserName = "111111",
                    Password = "password",
                    SimGroup = _simGroup1
                },
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            };
            var simDevice3 = new SimAndDevice() // 組織 : '他組織 SIM(MSISDN):'フィルターに合致
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            };
            _context.AddRange(simDevice3, simDevice1, simDevice2, simDevice4);
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=1&Msisdn=msisdn&sortBy=Msisdn&orderBy=asc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(2, (int)json["count"]);
            Assert.Equal("msisdn01", list[0]["sim"]["msisdn"]); // SIM(MSISDN)ソート:'昇順
            Assert.Equal("msisdn01a", list[1]["sim"]["msisdn"]);
        }

        /// <summary>
        /// 端末フィルター : '存在; ページ数:''存在（中間ページ）;1ページ当たり表示件数:'存在（デフォルト値以外）SIM(MSISDN)ソート:'降順
        /// </summary>
        [Fact]
        public void Case05()
        {
            for (int i = 1; i <= 6; i++)
            {
                _context.Add(new SimAndDevice() // 組織 : '自組織 端末:'フィルターに合致
                {
                    Sim = new Sim() // 組織 : '自組織
                    {
                        Msisdn = $"msisdn{i:00}",
                        Imsi = $"imsi{i:00}",
                        IccId = $"iccid{i:00}",
                        UserName = $"sim{i:00}",
                        Password = "password",
                        SimGroup = _simGroup1
                    },
                    Device = _device1,
                    IsolatedNw2Ip = "127.0.0.1",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-02-07"),
                });
            }
            _context.Add(new SimAndDevice() // 組織 : '自組織 端末:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = new Device()
                {
                    LteModule = _lte1,
                    Domain = _domain1,
                    Name = "aaaaaa",
                    UseTpm = true,
                    ManagedNumber = "001",
                    WindowsSignInListCacheDays = 1,
                },
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            });
            _context.Add(new SimAndDevice() // 組織 : '他組織 端末:'フィルターに合致
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=2&pageSize=2&DeviceName=device&sortBy=Msisdn&orderBy=desc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(6, (int)json["count"]);
            Assert.Equal("msisdn04", list[0]["sim"]["msisdn"]); // SIM(MSISDN)ソート:'降順
            Assert.Equal("msisdn03", list[1]["sim"]["msisdn"]);
        }

        /// <summary>
        /// 利用開始日範囲開始フィルター : '存在; ページ数:''存在（最終ページ）;1ページ当たり表示件数:'存在（デフォルト値以外）;端末ソート:'昇順
        /// </summary>
        [Fact]
        public void Case06()
        {
            for (int i = 1; i <= 8; i++)
            {
                _context.Add(new SimAndDevice() // 組織 : '自組織 利用開始日範囲開始:'フィルターに合致
                {
                    Sim = _sim1,
                    Device = new Device()
                    {
                        LteModule = _lte1,
                        Domain = _domain1,
                        Name = $"device{i:00}",
                        UseTpm = true,
                        ManagedNumber = "001",
                        WindowsSignInListCacheDays = 1,
                    },
                    IsolatedNw2Ip = "127.0.0.1",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-02-07"),
                });
            }
            _context.Add(new SimAndDevice() // 組織 : '自組織 利用開始日範囲開始:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-05"),
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=3&pageSize=3&startDateFrom=2020-02-06&sortBy=DeviceName&orderBy=asc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(3 > list.Count); //'指定件数未満
            Assert.Equal(8, (int)json["count"]);
            Assert.Equal("device07", list[0]["device"]["name"]);
            Assert.Equal("device08", list[1]["device"]["name"]); // 端末ソート: '昇順
        }

        /// <summary>
        /// 利用開始日範囲終了フィルター : '存在; ページ数:''存在（中間ページ）;端末ソート:'降順
        /// </summary>
        [Fact]
        public void Case07()
        {
            for (int i = 1; i <= 42; i++)
            {
                _context.Add(new SimAndDevice() // 組織 : '自組織 利用開始日範囲終了:'フィルターに合致
                {
                    Sim = _sim1,
                    Device = new Device()
                    {
                        LteModule = _lte1,
                        Domain = _domain1,
                        Name = $"device{i:00}",
                        UseTpm = true,
                        ManagedNumber = "001",
                        WindowsSignInListCacheDays = 1,
                    },
                    IsolatedNw2Ip = "127.0.0.1",
                    AuthenticationDuration = 1,
                    StartDate = DateTime.Parse("2020-02-07"),
                });
            }
            _context.Add(new SimAndDevice() // 組織 : '自組織 利用開始日範囲終了:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-09"),
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=2&startDateTo=2020-02-08&sortBy=DeviceName&orderBy=desc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(20, list.Count);
            Assert.Equal(42, (int)json["count"]);
            Assert.Equal("device22", list[0]["device"]["name"]);
            Assert.Equal("device21", list[1]["device"]["name"]); // 名前ソート:'降順
        }

        /// <summary>
        /// 利用終了日範囲開始フィルター : '存在; ページ数:''存在（初期ページ）;検疫NW2 IPアドレスソート:'昇順
        /// </summary>
        [Fact]
        public void Case08()
        {
            for (int i = 1; i <= 10; i++)
            {
                _context.Add(new SimAndDevice() // 組織 : '自組織 利用終了日範囲開始:'フィルターに合致
                {
                    Sim = _sim1,
                    Device = _device1,
                    IsolatedNw2Ip = $"127.0.0.{i:00}",
                    AuthenticationDuration = i,
                    EndDate = DateTime.Parse("2020-02-07")
                });
            }
            _context.Add(new SimAndDevice() // 組織 : '自組織 利用終了日範囲開始:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                EndDate = DateTime.Parse("2020-02-05"),
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=1&endDateFrom=2020-02-06&sortBy=IsolatedNw2Ip&orderBy=asc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(20 > list.Count); //'指定件数未満
            Assert.Equal(10, (int)json["count"]);
            Assert.Equal("127.0.0.01", list[0]["isolatedNw2Ip"]);
            Assert.Equal("127.0.0.02", list[1]["isolatedNw2Ip"]); // 検疫NW2 IPアドレスソート:'昇順
        }

        /// <summary>
        /// 利用終了日範囲終了フィルター : '存在; ページ数:''存在（中間ページ）;1ページ当たり表示件数：'存在（デフォルト値以外）；検疫NW2 IPアドレスソート:'降順
        /// </summary>
        [Fact]
        public void Case09()
        {
            for (int i = 1; i <= 6; i++)
            {
                _context.Add(new SimAndDevice() // 組織 : '自組織 利用終了日範囲終了:'フィルターに合致
                {
                    Sim = _sim1,
                    Device = _device1,
                    IsolatedNw2Ip = $"127.0.0.{i:00}",
                    AuthenticationDuration = i,
                    EndDate = DateTime.Parse("2020-02-07")
                });
            }
            _context.Add(new SimAndDevice() // 組織 : '自組織 利用終了日範囲終了:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                EndDate = DateTime.Parse("2020-02-09"),
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=2&pageSize=2&endDateTo=2020-02-08&sortBy=IsolatedNw2Ip&orderBy=desc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, list.Count);
            Assert.Equal(6, (int)json["count"]);
            Assert.Equal("127.0.0.04", list[0]["isolatedNw2Ip"]);
            Assert.Equal("127.0.0.03", list[1]["isolatedNw2Ip"]); // 検疫NW2 IPアドレスソート:'降順
        }

        /// <summary>
        /// 認証済みフィルター : ''済; ページ数:''存在（最終ページ）;利用開始日ソート:'昇順
        /// </summary>
        [Fact]
        public void Case10()
        {
            for (int j = 1; j <= 2; j++)
            {
                for (int i = 1; i <= 25; i++)
                {
                    var simDeviceAuthenticationStateDone = new SimAndDeviceAuthenticated()
                    {
                        Expiration = DateTime.Now.AddHours(6.00)
                    };
                    var simDevice = new SimAndDevice() // 組織 : '自組織 認証済み:'フィルターに合致
                    {
                        Sim = _sim1,
                        Device = _device1,
                        IsolatedNw2Ip = $"127.0.0.{i:00}",
                        AuthenticationDuration = i,
                        StartDate = DateTime.Parse($"2020-{j:00}-{i:00}"),
                        SimAndDeviceAuthenticated = simDeviceAuthenticationStateDone
                    };
                    _context.AddRange(simDevice, simDeviceAuthenticationStateDone);
                }
            }
            _context.Add(new SimAndDevice() // 組織 : '自組織 認証済み:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-01")
            });
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=3&IsAuthenticationDone=True&sortBy=startDate&orderBy=asc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.InRange(list.Count, 1, 20);
            Assert.Equal(50, (int)json["count"]);
            Assert.Equal("2020-02-16", list[0]["startDate"]);
            Assert.Equal("2020-02-17", list[1]["startDate"]); // 利用開始日ソート:'昇順
        }

        /// <summary>
        /// 全部 '存在; ページ数:''存在（中間ページ）;1ページ当たり表示件数:'存在（デフォルト値以外）; 利用開始日ソート:'降順
        /// </summary>
        [Fact]
        public void Case11()
        {
            SetUpData();
            for (int j = 1; j <= 2; j++)
            {
                for (int i = 1; i <= 25; i++)
                {
                    var simDeviceAuthenticationStateDone = new SimAndDeviceAuthenticated()
                    {
                        Expiration = DateTime.Now.AddHours(6.00)
                    };
                    var simDevice = new SimAndDevice() // 正常データ
                    {
                        Sim = _sim1,
                        Device = _device1,
                        IsolatedNw2Ip = $"127.0.0.{i:00}",
                        AuthenticationDuration = i,
                        StartDate = DateTime.Parse($"2020-{j:00}-{i:00}"),
                        EndDate = DateTime.Parse($"2021-{j:00}-{i:00}"),
                        SimAndDeviceAuthenticated = simDeviceAuthenticationStateDone
                    };
                    _context.AddRange(simDevice, simDeviceAuthenticationStateDone);
                }
            }
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=12&pageSize=2&Msisdn=msisdn&DeviceName=device&startDateFrom=2019-12-29&startDateTo=2020-02-27&endDateFrom=2020-12-29&endDateTo=2021-02-27&IsAuthenticationDone=True&sortBy=startDate&orderBy=desc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, list.Count);
            Assert.Equal(50, (int)json["count"]);
            Assert.Equal("2020-02-03", list[0]["startDate"]);
            Assert.Equal("2020-02-02", list[1]["startDate"]);// 利用開始日ソート:'降順
        }

        public void SetUpData()
        {
            _context.Add(new SimAndDevice() // 組織 : '自組織 SIM(MSISDN):'フィルターに合致せず
            {
                Sim = new Sim() // 組織 : '自組織
                {
                    Msisdn = "abcdfe",
                    Imsi = "12",
                    IccId = "11111",
                    UserName = "111111",
                    Password = "password",
                    SimGroup = _simGroup1
                },
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
            });
            _context.Add(new SimAndDevice() // 組織 : '自組織 端末:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device2,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
            });
            _context.Add(new SimAndDevice() // 組織 : '自組織 利用開始日範囲開始:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2019-12-05")
            });
            _context.Add(new SimAndDevice() // 組織 : '自組織 利用開始日範囲終了:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-03-09")
            });
            _context.Add(new SimAndDevice() // 組織 : '自組織 利用終了日範囲開始:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                EndDate = DateTime.Parse("2020-12-05")
            });
            _context.Add(new SimAndDevice() // 組織 : '自組織 利用終了日範囲終了:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                EndDate = DateTime.Parse("2021-03-09")
            });
            _context.Add(new SimAndDevice() // 組織 : '自組織 認証済み:'フィルターに合致せず
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07")
            });
        }

        /// <summary>
        /// 全部 '存在; ページ数:''存在（初期ページ）;1ページ当たり表示件数；'存在（デフォルト値以外）利用終了日ソート:'昇順
        /// </summary>
        [Fact]
        public void Case12()
        {
            SetUpData();
            for (int j = 1; j <= 2; j++)
            {
                for (int i = 1; i <= 25; i++)
                {
                    var simDeviceAuthenticationStateDone = new SimAndDeviceAuthenticated()
                    {
                        Expiration = DateTime.Now.AddHours(6.00)
                    };
                    var simDevice = new SimAndDevice() // 正常データ
                    {
                        Sim = _sim1,
                        Device = _device1,
                        IsolatedNw2Ip = $"127.0.0.{i:00}",
                        AuthenticationDuration = i,
                        StartDate = DateTime.Parse($"2020-{j:00}-{i:00}"),
                        EndDate = DateTime.Parse($"2021-{j:00}-{i:00}"),
                        SimAndDeviceAuthenticated = simDeviceAuthenticationStateDone
                    };
                    _context.AddRange(simDevice, simDeviceAuthenticationStateDone);
                }
            }
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=1&pageSize=2&Msisdn=msisdn&DeviceName=device&startDateFrom=2019-12-29&startDateTo=2020-02-27&endDateFrom=2020-12-29&endDateTo=2021-02-27&IsAuthenticationDone=True&sortBy=endDate&orderBy=asc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, list.Count);
            Assert.Equal(50, (int)json["count"]);
            Assert.Equal("2021-01-01", list[0]["endDate"]);
            Assert.Equal("2021-01-02", list[1]["endDate"]);// 利用終了日ソート:'昇順
        }

        /// <summary>
        /// 全部 '存在; 認証済みフィルター:'未 ページ数:''存在（中間ページ）;利用終了日ソート:'降順
        /// </summary>
        [Fact]
        public void Case13()
        {
            SetUpData();
            for (int j = 1; j <= 2; j++)
            {
                for (int i = 1; i <= 25; i++)
                {
                    var simDevice = new SimAndDevice() // 正常データ
                    {
                        Sim = _sim1,
                        Device = _device1,
                        IsolatedNw2Ip = $"127.0.0.{i:00}",
                        AuthenticationDuration = i,
                        StartDate = DateTime.Parse($"2020-{j:00}-{i:00}"),
                        EndDate = DateTime.Parse($"2021-{j:00}-{i:00}"),
                    };
                    _context.AddRange(simDevice);
                }
            }
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=2&Msisdn=msisdn&DeviceName=device&startDateFrom=2019-12-29&startDateTo=2020-02-27&endDateFrom=2020-12-29&endDateTo=2021-02-27&IsAuthenticationDone=False&sortBy=endDate&orderBy=desc", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(20, list.Count);
            Assert.Equal(50, (int)json["count"]);
            Assert.Equal("2021-02-05", list[0]["endDate"]);
            Assert.Equal("2021-02-04", list[1]["endDate"]); // 利用終了日ソート:'降順
        }

        /// <summary>
        /// 全部 '存在;認証済みフィルター:'済; ページ数:''存在（初期ページ）;認証済みソート:'降順
        /// </summary>
        [Fact]
        public void Case14()
        {
            SetUpData();
            _context.SaveChanges();
            var (response, _, json) = Utils.Get(_client, $"{Url}/?page=1&Msisdn=msisdn&DeviceName=device&startDateFrom=2020-02-06&startDateTo=2020-02-08&endDateFrom=2020-02-08&endDateTo=2020-02-10&IsAuthenticationDone=True", "user1", "user1", 1, "domain01"); // ユーザー管理者
            var list = (JArray)json["results"];
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(list);
        }

    }
}
