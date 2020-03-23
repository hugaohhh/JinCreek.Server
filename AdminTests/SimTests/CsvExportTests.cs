using CsvHelper;
using JinCreek.Server.Admin.Controllers;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace JinCreek.Server.AdminTests.SimTests
{
    /// <summary>
    /// sim一覧エクスポート
    /// </summary>
    [Collection("Sequential")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class CsvExportTests : IClassFixture<CustomWebApplicationFactory<JinCreek.Server.Admin.Startup>>
    {
        private readonly HttpClient _client;
        private readonly MainDbContext _context;

        private const string Url = "/api/sims/csv";

        private readonly Organization _org1;
        private readonly Organization _org2;
        private readonly Domain _domain1;
        private readonly Domain _domain1a;
        private readonly Domain _domain2;
        private readonly UserGroup _userGroup1;
        private readonly UserGroup _userGroup2;
        private readonly SimGroup _simGroup1;
        private readonly SimGroup _simGroup1a;
        private readonly Sim _sim1;
        private readonly Sim _sim1a;
        private readonly Sim _sim2;

        public CsvExportTests(CustomWebApplicationFactory<JinCreek.Server.Admin.Startup> factory)
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
            _context.Add(new SuperAdmin { AccountName = "user0", Password = Utils.HashPassword("user0") }); // スーパー管理者
            _context.Add(new UserAdmin { AccountName = "user1", Password = Utils.HashPassword("user1"), Domain = _domain1 }); // ユーザー管理者
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
                SecondaryDns = "SecondaryDns"
            });
            _context.Add(_simGroup1a = new SimGroup
            { Id = Guid.NewGuid(), Name = "simGroup1a", Organization = _org1, IsolatedNw1IpPool = "" });
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
                Msisdn = "msisdn02",
                Imsi = "imsi02",
                IccId = "iccid02",
                UserName = "sim02",
                Password = "password",
                SimGroup = _simGroup1a
            });
            _context.Add(_sim2 = new Sim() // 組織 : '自組織
            {
                Msisdn = "msisdn03",
                Imsi = "imsi03",
                IccId = "iccid03",
                UserName = "sim03",
                Password = "password",
                SimGroup = _simGroup1a
            });
            _context.SaveChanges();
        }

        /// <summary>
        /// ユーザー管理者 
        /// </summary>
        [Fact]
        public void Case01()
        {
            var token = Utils.GetAccessToken(_client, "user1", "user1", 1, "domain01"); // ユーザー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
            Assert.Empty(body);
        }

        /// <summary>
        /// 組織コード:'不在
        /// </summary>
        [Fact]
        public void Case02()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotEmpty(body);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.Equal("The organizationCode field is required.", json["errors"]?["organizationCode"].First);
        }
        /// <summary>
        /// 組織コード:'数字以外の文字含む
        /// </summary>
        [Fact]
        public void Case03()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=abcd", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.NotEmpty(body);
            var json = JObject.Parse(body);
            Assert.NotNull(json["traceId"]);
            Assert.EndsWith("is not valid.", json["errors"]?["organizationCode"].First.ToString());
        }
        /// <summary>
        /// DBに組織不在
        /// </summary>
        [Fact]
        public void Case04()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=2333", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var stringReader = new StringReader(body);
            var csvReader = new CsvReader(stringReader, CultureInfo.InvariantCulture);
            csvReader.Configuration.RegisterClassMap<SimsController.SimMap>();
            var sims = csvReader.GetRecords<Sim>().ToList();
            Assert.Empty(sims);
        }
        /// <summary>
        /// 正常
        /// </summary>
        [Fact]
        public void Case05()
        {
            var token = Utils.GetAccessToken(_client, "user0", "user0"); // スーパー管理者
            var result = Utils.Get(_client, $"{Url}?organizationCode=1", token);
            var body = result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var stringReader = new StringReader(body);
            var csvReader = new CsvReader(stringReader, CultureInfo.InvariantCulture);
            csvReader.Configuration.RegisterClassMap<SimsController.SimMap>();
            var sims = csvReader.GetRecords<Sim>().ToList();

            Assert.Equal("simGroup1", sims[0].SimGroup.Name);
            Assert.Equal("simGroup1a", sims[1].SimGroup.Name); // SIMグループソート:昇順
            Assert.Equal("msisdn02", sims[1].Msisdn);
            Assert.Equal("msisdn03", sims[2].Msisdn); // MSISDNソート:昇順
        }
    }
}
