using CsvHelper;
using CsvHelper.Configuration.Attributes;
using JinCreek.Server.Admin.Controllers;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.MultiFactorTests
{
    /// <summary>
    /// 認証要素組合せエクスポート
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class CsvExportTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/multi-factors/csv";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly SimGroup _simGroup1;
        private readonly Device _device1;
        private readonly Device _device2;
        private readonly Sim _sim1;
        private readonly Sim _sim2;
        private readonly SimAndDevice _simDevice1;
        private readonly SimAndDevice _simDevice2;
        private readonly SimAndDevice _simDevice3;

        private readonly User _user0;
        private readonly User _user1;
        private readonly User _user2;
        private readonly User _user2a;
        private readonly User _user3;


        public CsvExportTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
        {
            _client = factory.CreateClient();
            _context = factory.Services.GetService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetService<MainDbContext>();

            Utils.RemoveAllEntities(_context);

            _context.Add(_org1 = Utils.CreateOrganization(code: 1, name: "org1"));
            _context.Add(_org2 = Utils.CreateOrganization(code: 2, name: "org2"));
            _context.Add(_domain1 = new Domain { Id = Guid.NewGuid(), Name = "domain01", Organization = _org1 });
            _context.Add(_domain2 = new Domain { Id = Guid.NewGuid(), Name = "domain03", Organization = _org2 });
            _context.Add(_userGroup1 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup1", Domain = _domain1 });
            _context.Add(_userGroup2 = new UserGroup { Id = Guid.NewGuid(), Name = "userGroup2", Domain = _domain2 });
            _context.Add(_user0 = new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(_user1 = new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), DomainId = _domain1.Id }); // ユーザー管理者
            _context.Add(_user2 = new UserAdmin { AccountName = "user2", Password = Utils.HashPassword("user2"), DomainId = _domain1.Id }); // ユーザー管理者
            _context.Add(_user2a = new UserAdmin { AccountName = "user2a", Password = Utils.HashPassword("user2a"), DomainId = _domain1.Id }); // ユーザー管理者
            _context.Add(_user3 = new GeneralUser() { AccountName = "user2", DomainId = _domain2.Id });
            _context.Add(_simGroup1 = new SimGroup()
            {
                Id = Guid.NewGuid(),
                Name = "simGroup1",
                Organization = _org1,
                Apn = "apn",
                AuthenticationServerIp = "AuthenticationServerIp",
                IsolatedNw1IpPool = "IsolatedNw1IpPool",
                IsolatedNw1SecondaryDns = "IsolatedNw1SecondaryDns",
                IsolatedNw1IpRange = "IsolatedNw1IpRange",
                IsolatedNw1PrimaryDns = "IsolatedNw1PrimaryDns",
                NasIp = "NasIp",
                PrimaryDns = "PrimaryDns",
                SecondaryDns = "SecondaryDns",
                UserNameSuffix = ""
            });
            _context.Add(_sim1 = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn01",
                Imsi = "imsi01",
                IccId = "iccid01",
                UserName = "sim01",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_sim2 = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn02",
                Imsi = "imsi02",
                IccId = "iccid02",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1
            });
            _context.Add(_device1 = new Device()
            {
                LteModule = null,
                Domain = _domain1,
                Name = "device01",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
            _context.Add(_device2 = new Device()
            {
                LteModule = null,
                Domain = _domain1,
                Name = "device02",
                UseTpm = true,
                ManagedNumber = "001",
                WindowsSignInListCacheDays = 1,
            });
            _context.Add(_simDevice1 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim1,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            });
            _context.Add(_simDevice2 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device1,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            });
            _context.Add(_simDevice3 = new SimAndDevice() // 組織 : '自組織
            {
                Sim = _sim2,
                Device = _device2,
                IsolatedNw2Ip = "127.0.0.1",
                AuthenticationDuration = 1,
                StartDate = DateTime.Parse("2020-02-07"),
                EndDate = DateTime.Now.AddHours(6.00)
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者 
        /// </summary>
        [Fact]
        public void Case01()
        {
            var (response, body, _) = Utils.Get(_client, $"{Url}/?organizationCode=1", "user1", "user1", 1, _domain1.Name);// ユーザー管理者
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// 組織コード:'不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("The organizationCode field is required.", json["errors"]?["organizationCode"].First);
        }
        /// <summary>
        /// 組織コード:'数字以外の文字含む
        /// </summary>
        [Fact]
        public void Case03()
        {
            var (response, _, json) = Utils.Get(_client, $"{Url}/?organizationCode=afasf", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(json["traceId"]);
            Assert.EndsWith("is not valid.", json["errors"]?["organizationCode"].First.ToString());
        }
        /// <summary>
        /// DBに組織不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var (response, body, _) = Utils.Get(_client, $"{Url}/?organizationCode=2333", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stringReader = new StringReader(body);
            var csvReader = new CsvReader(stringReader, CultureInfo.InvariantCulture);
            csvReader.Configuration.RegisterClassMap<MultiFactorsController.MultiFactorMap>();
            var multiFactors = csvReader.GetRecords<MultiFactor>().ToList();
            Assert.Empty(multiFactors);
        }

        public sealed class CsvRecord
        {
            [Name("is Delete")]
            public string Delete { get; set; }
            [Name("End Date")]
            public DateTime EndDate { get; set; }
            [Name("Start Date")]
            public DateTime StartDate { get; set; }
            [Name("Closed NW IP")]
            public string ClosedNwIp { get; set; }
            [Name("Sim And Device ID")]
            public Guid SimAndDeviceId { get; set; }
            [Name("User ID")]
            public Guid UserId { get; set; }
            [Name("User Name")]
            public string AccountName { get; set; }
            [Name("MSISDN")]
            public string Msisdn { get; set; }
            [Name("Domain")]
            public string DomainName { get; set; }
            [Name("Device")]
            public string DeviceName { get; set; }
        }

        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            _context.Add(new MultiFactor { SimAndDeviceId = _simDevice1.Id, EndUserId = _user3.Id }); //User他組織
            _context.Add(new MultiFactor { SimAndDevice = _simDevice1, EndUserId = _user2.Id, ClosedNwIp = "127.0.0.1", StartDate = DateTime.Now, EndDate = DateTime.Now });
            _context.Add(new MultiFactor { SimAndDevice = _simDevice2, EndUserId = _user2.Id, ClosedNwIp = "127.0.0.1", StartDate = DateTime.Now, EndDate = DateTime.Now });
            _context.Add(new MultiFactor { SimAndDevice = _simDevice3, EndUserId = _user2.Id, ClosedNwIp = "127.0.0.1", StartDate = DateTime.Now, EndDate = DateTime.Now });
            _context.Add(new MultiFactor { SimAndDevice = _simDevice3, EndUserId = _user2a.Id, ClosedNwIp = "127.0.0.1", StartDate = DateTime.Now, EndDate = DateTime.Now });
            _context.SaveChanges();
            var (response, body, _) = Utils.Get(_client, $"{Url}/?organizationCode=1", "user0", "user0"); // スーパー管理者
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stringReader = new StringReader(body);
            var csvReader = new CsvReader(stringReader, CultureInfo.InvariantCulture);
            csvReader.Configuration.RegisterClassMap<MultiFactorsController.MultiFactorMap>();
            var multiFactors = csvReader.GetRecords<CsvRecord>().ToList();
            Assert.Equal(4, multiFactors.Count);

            Assert.Equal("msisdn01", multiFactors[0].Msisdn);
            Assert.Equal("msisdn02", multiFactors[1].Msisdn); //SIMソート:昇順
            Assert.Equal("device01", multiFactors[1].DeviceName);
            Assert.Equal("device02", multiFactors[2].DeviceName); //端末ソート:昇順
            Assert.Equal("user2", multiFactors[2].AccountName);
            Assert.Equal("user2a", multiFactors[3].AccountName); // ユーザーソート:'昇順
        }
    }
}
