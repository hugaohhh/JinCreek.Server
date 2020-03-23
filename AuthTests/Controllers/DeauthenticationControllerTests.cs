using JinCreek.Server.Auth;
using JinCreek.Server.AuthTests.Repositories;
using JinCreek.Server.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.AuthTests.Controllers
{
    [Collection("Sequential")]
    public class DeauthenticationControllerTests : WebApplicationBase
    {
        private readonly AuthControllerTestRepository _repository;
        private readonly DeauthenticationControllerTestSetupRepository _setupRepository;

        private readonly AuthHttpClientWrapper _client;
        public DeauthenticationControllerTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
            _setupRepository = new DeauthenticationControllerTestSetupRepository(MainDbContext, RadiusDbContext);
            _repository = new AuthControllerTestRepository(MainDbContext, RadiusDbContext);
            _client = new AuthHttpClientWrapper(Client);
        }
        class TestDataClass : IEnumerable<object[]>
        {
            List<object[]> _testData = new List<object[]>();

            public TestDataClass()
            {
                var certificationBase64 = "Q2VydGlmaWNhdGU6CiAgICBEYXRhOgogICAgICAgIFZlcnNpb246IDMgKDB4MikKICAgICAgICBTZXJpYWwgTnVtYmVyOiA0ICgweDQpCiAgICAgICAgU2lnbmF0dXJlIEFsZ29yaXRobTogc2hhMjU2V2l0aFJTQUVuY3J5cHRpb24KICAgICAgICBJc3N1ZXI6IEM9SlAsIFNUPVRva3lvLCBMPUNodW8ta3UsIE89SmluQ3JlZWssIENOPUppbkNyZWVrIENBCiAgICAgICAgVmFsaWRpdHkKICAgICAgICAgICAgTm90IEJlZm9yZTogSmFuIDI5IDEyOjIxOjM4IDIwMjAgR01UCiAgICAgICAgICAgIE5vdCBBZnRlciA6IEphbiAyOCAxMjoyMTozOCAyMDIxIEdNVAogICAgICAgIFN1YmplY3Q6IEM9SlAsIFNUPVRva3lvLCBPPUppbkNyZWVrLCBDTj1KSU5DUkVFSy1QQwogICAgICAgIFN1YmplY3QgUHVibGljIEtleSBJbmZvOgogICAgICAgICAgICBQdWJsaWMgS2V5IEFsZ29yaXRobTogcnNhRW5jcnlwdGlvbgogICAgICAgICAgICAgICAgUlNBIFB1YmxpYy1LZXk6ICgyMDQ4IGJpdCkKICAgICAgICAgICAgICAgIE1vZHVsdXM6CiAgICAgICAgICAgICAgICAgICAgMDA6Yjg6ZGY6Zjc6YTY6Y2E6MDQ6Nzk6ZWM6MDQ6YzQ6MDM6ZDQ6NjY6ZTk6CiAgICAgICAgICAgICAgICAgICAgNTM6Mzg6YzQ6YjA6Nzg6YmE6ZTQ6ZTY6NjA6OWM6ZTA6ZDE6Mzk6MmY6MjE6CiAgICAgICAgICAgICAgICAgICAgYTc6ZTk6YWU6NTU6OGU6NjA6NzY6NTA6Yzc6Mzk6MDM6OTI6MDc6Njc6ZDU6CiAgICAgICAgICAgICAgICAgICAgNTU6Zjg6MmU6Njc6MjU6ZGM6NzM6ODk6MjE6NmI6ODc6OTQ6MGU6YWM6NGU6CiAgICAgICAgICAgICAgICAgICAgYzA6YTQ6Mzk6Y2U6Yzg6NDA6Zjk6ODc6ZGU6ZjE6MzE6NGQ6ZGU6ZmE6ODc6CiAgICAgICAgICAgICAgICAgICAgMmU6OTQ6ZTA6MmM6MWI6NmQ6YWQ6NWU6MTI6OTU6Zjc6Mzg6OGE6OWM6ZDA6CiAgICAgICAgICAgICAgICAgICAgY2M6YzY6ODk6ZTc6ZTU6MDY6ODI6YjY6N2M6YTY6MjY6MTU6MDc6Njg6Mzk6CiAgICAgICAgICAgICAgICAgICAgMDU6MWI6MWU6MDA6NDk6MzA6ZDA6YzY6Y2M6MDY6OTY6NDE6OTA6MzA6NzU6CiAgICAgICAgICAgICAgICAgICAgNzg6NTk6MzI6Njk6N2U6YzM6Y2Y6MzE6MzU6ZTQ6NDk6ZGU6NDI6YzA6MDc6CiAgICAgICAgICAgICAgICAgICAgMzI6ZjM6MTk6ZWU6Njc6NjE6YTI6YTA6Zjg6YjQ6NDA6OTY6ODY6NTY6ZTc6CiAgICAgICAgICAgICAgICAgICAgMmY6YTk6MzA6YWI6OTk6NWQ6NDY6YzY6ZTU6Yjk6OTc6ZjY6NmE6YTM6Y2U6CiAgICAgICAgICAgICAgICAgICAgNGI6MWU6Njc6NDI6N2M6ODY6Zjk6NDI6OGQ6N2Q6Yzg6MWU6ZWI6MDA6NmE6CiAgICAgICAgICAgICAgICAgICAgMTk6OTk6NjI6NDE6M2I6ZDg6NjE6ZTQ6NzU6MmI6M2E6OGI6NjU6NDc6NTc6CiAgICAgICAgICAgICAgICAgICAgOWM6ODE6NDk6NDI6ODk6MmI6OTM6MjE6ZjQ6YmU6MDU6MmE6MDc6M2Y6MjI6CiAgICAgICAgICAgICAgICAgICAgZmY6MjE6Y2E6Nzg6NmI6MmE6YTM6Yzk6ZDY6MDc6NjY6OWI6MmQ6NTQ6Y2Y6CiAgICAgICAgICAgICAgICAgICAgZTY6ZmE6MDg6MmU6N2U6OTA6NTg6ZjA6N2Q6MjQ6MmU6NTk6NWQ6NzQ6YTE6CiAgICAgICAgICAgICAgICAgICAgMTY6YWY6NzU6MTk6N2E6OGY6YjY6NDU6NjU6N2E6Nzk6Y2E6NWU6MzU6YTQ6CiAgICAgICAgICAgICAgICAgICAgMDI6YjcKICAgICAgICAgICAgICAgIEV4cG9uZW50OiA2NTUzNyAoMHgxMDAwMSkKICAgICAgICBYNTA5djMgZXh0ZW5zaW9uczoKICAgICAgICAgICAgWDUwOXYzIEJhc2ljIENvbnN0cmFpbnRzOiAKICAgICAgICAgICAgICAgIENBOkZBTFNFCiAgICAgICAgICAgIE5ldHNjYXBlIENlcnQgVHlwZTogCiAgICAgICAgICAgICAgICBTU0wgQ2xpZW50LCBTL01JTUUsIE9iamVjdCBTaWduaW5nCiAgICAgICAgICAgIE5ldHNjYXBlIENvbW1lbnQ6IAogICAgICAgICAgICAgICAgT3BlblNTTCBHZW5lcmF0ZWQgQ2VydGlmaWNhdGUKICAgICAgICAgICAgWDUwOXYzIFN1YmplY3QgS2V5IElkZW50aWZpZXI6IAogICAgICAgICAgICAgICAgMEE6REI6RTY6NTM6QUQ6Rjc6M0Q6MjM6MEY6RjU6OTU6ODQ6Mzk6MDE6NEI6Q0E6QUU6N0Y6RjY6NDkKICAgICAgICAgICAgWDUwOXYzIEF1dGhvcml0eSBLZXkgSWRlbnRpZmllcjogCiAgICAgICAgICAgICAgICBrZXlpZDpFQzpDRjpGMDo4NzoxRToyQTo0NzpEODo3MjoyQzo1RTo1Qjo5Qjo4Qjo5Njo5QjoxQzpBQTo1ODo0MQoKICAgIFNpZ25hdHVyZSBBbGdvcml0aG06IHNoYTI1NldpdGhSU0FFbmNyeXB0aW9uCiAgICAgICAgIDg3OmY4OmMxOjI5OmFmOmEwOjhjOjY4OjU2OjgwOjNlOjM2OjBhOjU2OmM3OjdmOjY1OmNiOgogICAgICAgICAzYzpjMzoxNjoxZDpmMjo3ZTozMjplZDplMzoyOTo4NjphMTo5Yzo3MjpmYzo3Yjo5YzpmZToKICAgICAgICAgMGY6ZGE6ZTI6Mzc6NDQ6ZjM6NWM6NTk6ZTk6NDQ6MTY6MDE6NWY6MTE6ZTA6M2E6NzU6NjQ6CiAgICAgICAgIGZmOjY0OmJiOmI2OjEwOmVjOjZlOjc2OmU4OjY1OjUzOjcxOjUwOjgzOmEzOjVlOjAxOmNlOgogICAgICAgICA0ZjpiZTplNTo1MDo2ODo2Zjo1NTpjMTphYTpmNDpjODpjNTpiZjpkNzoyODoyZjo1ZDowNjoKICAgICAgICAgNzM6MmY6ZTU6MjU6NDY6ZWM6OTE6Y2M6MDA6N2U6YjA6YzY6NDY6OWQ6NTQ6NDU6Y2Q6NmY6CiAgICAgICAgIGUyOjYzOmI3OjZhOjBmOjc5OmFjOmUyOjk2OmVlOjEzOjNjOmUxOjg1OjQ0OjQ1OjAwOjBhOgogICAgICAgICA4MjpkZjo4Mjo1Yjo2MTphNDo3ZDphNjo2YTo3NToxZToxOTo2ZDplOTplZTo1Mzo2ODo4MjoKICAgICAgICAgZTY6NTY6ZmU6NzQ6NjI6MDc6NjM6N2E6Mzk6NjI6OTE6MTQ6ZTk6NmU6YTE6NzU6Y2Y6MjY6CiAgICAgICAgIDg4OmEyOjdkOmZiOjE5OmMzOjgyOjRjOjViOmZkOjkxOjU2OmMxOjQ0OjM0OmVlOjZiOjM0OgogICAgICAgICAxZjozOTplZDoxZjo2Njo4YjpmYjpiNDoyMTpjYjo5YTphNjplNzpiZDowMjo1NTplYzplZDoKICAgICAgICAgMjM6MGU6MTY6NWY6ZDc6YjU6YzY6NzA6Njg6ZWM6NjI6YmM6YmM6MWM6NzQ6NzE6OWU6MDY6CiAgICAgICAgIDAzOmMyOjNkOjhhOjk4OmUyOjBkOjljOmFlOmU3OjBhOjRjOjBlOjRkOmY0OmE3OjJiOmQ0OgogICAgICAgICBhNTo3ZTpjMTphZjo4Mjo1MjpmYzoxMjo0ZjoxYjo4NDphOTo1OTo5ZDozMDo5ZDpmYjpjZjoKICAgICAgICAgYzY6ZWU6NmY6MDYKLS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSURxVENDQXBHZ0F3SUJBZ0lCQkRBTkJna3Foa2lHOXcwQkFRc0ZBREJZTVFzd0NRWURWUVFHRXdKS1VERU8KTUF3R0ExVUVDQXdGVkc5cmVXOHhFREFPQmdOVkJBY01CME5vZFc4dGEzVXhFVEFQQmdOVkJBb01DRXBwYmtOeQpaV1ZyTVJRd0VnWURWUVFEREF0S2FXNURjbVZsYXlCRFFUQWVGdzB5TURBeE1qa3hNakl4TXpoYUZ3MHlNVEF4Ck1qZ3hNakl4TXpoYU1FWXhDekFKQmdOVkJBWVRBa3BRTVE0d0RBWURWUVFJREFWVWIydDViekVSTUE4R0ExVUUKQ2d3SVNtbHVRM0psWldzeEZEQVNCZ05WQkFNTUMwcEpUa05TUlVWTExWQkRNSUlCSWpBTkJna3Foa2lHOXcwQgpBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUF1Ti8zcHNvRWVld0V4QVBVWnVsVE9NU3dlTHJrNW1DYzRORTVMeUduCjZhNVZqbUIyVU1jNUE1SUhaOVZWK0M1bkpkeHppU0ZyaDVRT3JFN0FwRG5PeUVENWg5N3hNVTNlK29jdWxPQXMKRzIydFhoS1Y5emlLbk5ETXhvbm41UWFDdG55bUpoVUhhRGtGR3g0QVNURFF4c3dHbGtHUU1IVjRXVEpwZnNQUApNVFhrU2Q1Q3dBY3k4eG51WjJHaW9QaTBRSmFHVnVjdnFUQ3JtVjFHeHVXNWwvWnFvODVMSG1kQ2ZJYjVRbzE5CnlCN3JBR29abVdKQk85aGg1SFVyT290bFIxZWNnVWxDaVN1VElmUytCU29IUHlML0ljcDRheXFqeWRZSFpwc3QKVk0vbStnZ3VmcEJZOEgwa0xsbGRkS0VXcjNVWmVvKzJSV1Y2ZWNwZU5hUUN0d0lEQVFBQm80R1BNSUdNTUFrRwpBMVVkRXdRQ01BQXdFUVlKWUlaSUFZYjRRZ0VCQkFRREFnU3dNQ3dHQ1dDR1NBR0crRUlCRFFRZkZoMVBjR1Z1ClUxTk1JRWRsYm1WeVlYUmxaQ0JEWlhKMGFXWnBZMkYwWlRBZEJnTlZIUTRFRmdRVUN0dm1VNjMzUFNNUDlaV0UKT1FGTHlxNS85a2t3SHdZRFZSMGpCQmd3Rm9BVTdNL3doeDRxUjloeUxGNWJtNHVXbXh5cVdFRXdEUVlKS29aSQpodmNOQVFFTEJRQURnZ0VCQUlmNHdTbXZvSXhvVm9BK05ncFd4MzlseXp6REZoM3lmakx0NHltR29aeHkvSHVjCi9nL2E0amRFODF4WjZVUVdBVjhSNERwMVpQOWt1N1lRN0c1MjZHVlRjVkNEbzE0QnprKys1VkJvYjFYQnF2VEkKeGIvWEtDOWRCbk12NVNWRzdKSE1BSDZ3eGthZFZFWE5iK0pqdDJvUGVhemlsdTRUUE9HRlJFVUFDb0xmZ2x0aApwSDJtYW5VZUdXM3A3bE5vZ3VaVy9uUmlCMk42T1dLUkZPbHVvWFhQSm9paWZmc1p3NEpNVy8yUlZzRkVOTzVyCk5CODU3UjltaS91MEljdWFwdWU5QWxYczdTTU9GbC9YdGNad2FPeGl2THdjZEhHZUJnUENQWXFZNGcyY3J1Y0sKVEE1TjlLY3IxS1Yrd2ErQ1V2d1NUeHVFcVZtZE1KMzd6OGJ1YndZPQotLS0tLUVORCBDRVJUSUZJQ0FURS0tLS0tCg==";
                var certificationBase64InvalidBase64 = "ABCDEFG";
                var certificationBase64InvalidCert = "QUJDREVGRwo=";

                _testData.Add(new object[] { new Case01(new DeauthenticationRequest() { SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case02(new DeauthenticationRequest() { Account = "AccountUser1", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                ////Case03はGuidオブジェクトが作成できないので別途記載
                //_testData.Add(new object[] { new Case04(new DeauthenticationRequest() { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776a"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                //_testData.Add(new object[] { new Case05(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized), });
                // SIM&端末認証部分について何もしないこととする
                //_testData.Add(new object[] { new Case06(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized) });
                //_testData.Add(new object[] { new Case07(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized) });
                //_testData.Add(new object[] { new Case08(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case09(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });

                _testData.Add(new object[] { new Case10(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = "", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case11(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64InvalidBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case12(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64InvalidCert, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case13(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case14(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "abcdef123456", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case15(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "89811000038174800001111111111111", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case16(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case17(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "abcdef123456", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case18(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "44010321310000011111111111", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case19(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981080005819480000", SimImsi = "440103213100000", SimMsisdn = "" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case20(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "abcdef123456" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case21(new DeauthenticationRequest() { Account = "AccountUser1", ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000111111111111111111111" }, HttpStatusCode.BadRequest) });
            }

            public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        //[Fact]
        //public void TestCase03()
        //{
        //    _setupRepository.SetUpInsertBaseDataForMainDb();
        //    _setupRepository.SetUpInsertBaseDataForRadiusDb();
        //    _setupRepository.SetUpInsertDataForCase03();

        //    string request = "{accountName:'aaa', authId:'aaaabbbbbbbbcccccccc'}";
        //    var acualRessult = _client.PostDeauthenticationCase03(request);
        //    DeauthenticationUtils.Assert01And02And10To21(_repository, acualRessult);
        //    var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);

        //    Assert.StartsWith("Error converting value \"aaaabbbbbbbbcccccccc\" to type 'System.Nullable`1[System.Guid]'", result["errors"]["authId"].First().ToString());
        //}


        [Theory(DisplayName = "認証解除APIテスト")]
        [ClassData(typeof(TestDataClass))]
        public void DeauthenticationValidation(DeauthenticationTestCase deauthenticationTestCase)
        {
            TestOutputHelper.WriteLine($"{deauthenticationTestCase}");
            deauthenticationTestCase.Test(TestOutputHelper, _repository, _setupRepository, _client);
        }

        private static class DeauthenticationUtils
        {
            public static void Assert01And02And10To21(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.BadRequest, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.NotNull(result["traceId"]);
                Assert.Equal("One or more validation errors occurred.", result["title"]);
                Assert.Equal("400", result["status"]);
                var deauthentication = authControllerTestRepository.GetDeauthentication();
                Assert.Empty(deauthentication);

                var simDeviceAuthenticationStateDones = authControllerTestRepository.GetAllSimAndDeviceAuthenticated();
                Assert.Empty(simDeviceAuthenticationStateDones);
                var multiFactorAuthenticationStateDones = authControllerTestRepository.GetMultiFatorAuthenticationDone();
                Assert.Empty(multiFactorAuthenticationStateDones);
                var radreply = authControllerTestRepository.GetRadreplys("user1@jincreek2");
                Assert.Single(radreply);
                Assert.Equal("NwAddress", radreply.First().Value);
            }
            //public static void Assert04(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            //{
            //    Assert.Equal(HttpStatusCode.Unauthorized, acualRessult.StatusCode);
            //    var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
            //    Assert.Equal("1001", result["ErrorCode"]);
            //    Assert.Equal("Not Match SimAndDevice Info", result["ErrorMessage"]);
            //    var deauthentication = authControllerTestRepository.GetDeauthentication();
            //    Assert.Empty(deauthentication);
            //    var simDeviceAuthenticationStateDones = authControllerTestRepository.GetSimAndDeviceAuthenticated();
            //    Assert.Empty(simDeviceAuthenticationStateDones);
            //    var multiFactorAuthenticationStateDones = authControllerTestRepository.GetMultiFatorAuthenticationDone();
            //    Assert.Empty(multiFactorAuthenticationStateDones);
            //    var radreply = authControllerTestRepository.GetRadreplys("user1@jincreek2");
            //    Assert.Single(radreply);
            //    Assert.Equal("NwAddress", radreply.First().Value);
            //}

            //public static void Assert05(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            //{
            //    Assert.Equal(HttpStatusCode.Unauthorized, acualRessult.StatusCode);
            //    var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
            //    Assert.Equal("1001", result["ErrorCode"]);
            //    Assert.Equal("Not Match SimAndDevice Info", result["ErrorMessage"]);
            //    var deauthentication = authControllerTestRepository.GetDeauthentication();
            //    Assert.Empty(deauthentication);
            //    var simDeviceAuthenticationStateDones = authControllerTestRepository.GetSimAndDeviceAuthenticated();
            //    // 関連ないSimDeviceAuthenticationStateDoneを入れているのでcount=1を確認
            //    Assert.Equal(1, simDeviceAuthenticationStateDones.Count);
            //    var multiFactorAuthenticationStateDones = authControllerTestRepository.GetMultiFatorAuthenticationDone();
            //    Assert.Empty(multiFactorAuthenticationStateDones);
            //    var radreply = authControllerTestRepository.GetRadreplys("user1@jincreek2");
            //    Assert.Single(radreply);
            //    Assert.Equal("NwAddress", radreply.First().Value);
            //}

            public static void Assert06To07(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
                Assert.Equal("", acualRessult.Content.ReadAsStringAsync().Result);
                //var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                //Assert.Equal("1002", result["ErrorCode"]);
                //Assert.Equal("Not Match MultiFactor Info", result["ErrorMessage"]);
                var deauthentication = authControllerTestRepository.GetDeauthentication();
                Assert.Empty(deauthentication);
                var simDeviceAuthenticationStateDones = authControllerTestRepository.GetAllSimAndDeviceAuthenticated();
                Assert.Empty(simDeviceAuthenticationStateDones);
                var multiFactorAuthenticationStateDones = authControllerTestRepository.GetMultiFatorAuthenticationDone();
                Assert.Empty(multiFactorAuthenticationStateDones);
                var radreply = authControllerTestRepository.GetRadreplys("user1@jincreek2");
                Assert.Single(radreply);
                Assert.Equal("Nw2Address", radreply.First().Value);
            }

            public static void Assert08(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult, DeauthenticationRequest request)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
                Assert.Equal("", acualRessult.Content.ReadAsStringAsync().Result);

                //TODO
                //Debug.Assert(request.AuthId != null, "request.AuthId != null");
                //認証成功でSimDeviceAuthenticationStateDoneがなくなっているのでAuIdで探すのは無理
                var simDeviceList = authControllerTestRepository.GetSimDeviceList();
                Assert.Equal(3, simDeviceList.Count);

                var factorCombinationList = authControllerTestRepository.GetFactorCombinationList();
                Assert.Equal(2, factorCombinationList.Count);

                var multiFactorAuthenticationStateDones = authControllerTestRepository.GetMultiFatorAuthenticationDone();
                Assert.Equal(0, multiFactorAuthenticationStateDones.Count);

                var radreply = authControllerTestRepository.GetRadreply("user1@jincreek2");
                Assert.Equal("Nw2Address", radreply.Value);
            }

            public static void Assert09(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult, DeauthenticationRequest request)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
                Assert.Equal("", acualRessult.Content.ReadAsStringAsync().Result);

                //TODO
                //Debug.Assert(request.AuthId != null, "request.AuthId != null");
                //認証成功でSimDeviceAuthenticationStateDoneがなくなっているのでAuIdで探すのは無理
                var simDeviceList = authControllerTestRepository.GetSimDeviceList();
                Assert.Equal(3, simDeviceList.Count);

                var factorCombinationList = authControllerTestRepository.GetFactorCombinationList();
                Assert.Equal(2, factorCombinationList.Count);

                var multiFactorAuthenticationStateDones = authControllerTestRepository.GetMultiFatorAuthenticationDone();
                Assert.Equal(1, multiFactorAuthenticationStateDones.Count);

                var radreply = authControllerTestRepository.GetRadreply("user1@jincreek2");
                Assert.Equal("Nw2Address", radreply.Value);
            }
        }
        public abstract class DeauthenticationTestCase
        {
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            protected DeauthenticationRequest _deauthenticationRequest;

            // setterのため
            [SuppressMessage("ReSharper", "NotAccessedField.Local")]
            private HttpStatusCode _httpStatusCode;

            // setterのため
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            protected ITestOutputHelper _testOutputHelper;

            protected DeauthenticationTestCase(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode)
            {
                _deauthenticationRequest = deauthenticationRequest;
                _httpStatusCode = httpStatusCode;
            }

            public virtual void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                setupRepository.SetUpInsertBaseDataForMainDb();
                setupRepository.SetUpInsertBaseDataForRadiusDb();
            }

            public abstract void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage);

            public void Test(ITestOutputHelper testOutputHelper, AuthControllerTestRepository authControllerTestRepository, DeauthenticationControllerTestSetupRepository setupRepository,
                AuthHttpClientWrapper httpClientWrapper)
            {
                _testOutputHelper = testOutputHelper;
                Test(authControllerTestRepository, setupRepository, httpClientWrapper);
            }

            public void Test(AuthControllerTestRepository authControllerTestRepository, DeauthenticationControllerTestSetupRepository setupRepository, AuthHttpClientWrapper httpClientWrapper)
            {
                SetUp(setupRepository);
                var response = httpClientWrapper.PostDeauthentication(_deauthenticationRequest);
                AssertThat(authControllerTestRepository, response);
            }
        }


        private class Case01 : DeauthenticationTestCase
        {
            public Case01(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("01 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase01();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("01 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("certification_required", result["errors"]["ClientCertificationBase64"].First().ToString());
            }
        }

        private class Case02 : DeauthenticationTestCase
        {
            public Case02(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("02 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase02();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("02 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("certification_required", result["errors"]["ClientCertificationBase64"].First().ToString());
            }
        }

        //private class Case04 : DeauthenticationTestCase
        //{
        //    public Case04(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
        //    {
        //    }

        //    public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("04 Setup");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase04();
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("04 AssertThat");
        //        DeauthenticationUtils.Assert04(authControllerTestRepository, acualRessult);
        //    }
        //}

        //private class Case05 : DeauthenticationTestCase
        //{
        //    public Case05(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
        //    {
        //    }

        //    public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("05 Setup");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase05();
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("05 AssertThat");
        //        DeauthenticationUtils.Assert05(authControllerTestRepository, acualRessult);
        //    }
        //}

        private class Case06 : DeauthenticationTestCase
        {
            public Case06(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("06 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase06();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("06 AssertThat");
                DeauthenticationUtils.Assert06To07(authControllerTestRepository, acualRessult);
            }
        }

        private class Case07 : DeauthenticationTestCase
        {
            public Case07(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("07 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase07();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("07 AssertThat");
                DeauthenticationUtils.Assert06To07(authControllerTestRepository, acualRessult);
            }
        }

        private class Case08 : DeauthenticationTestCase
        {
            public Case08(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("08 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase08();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("08 AssertThat");
                DeauthenticationUtils.Assert08(authControllerTestRepository, acualRessult, _deauthenticationRequest);
            }
        }

        private class Case09 : DeauthenticationTestCase
        {
            public Case09(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("09 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase09();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("09 AssertThat");
                DeauthenticationUtils.Assert09(authControllerTestRepository, acualRessult, _deauthenticationRequest);
            }
        }

        private class Case10 : DeauthenticationTestCase
        {
            public Case10(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("10 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase10();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("10 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("certification_required", result["errors"]["ClientCertificationBase64"].First().ToString());
            }
        }

        private class Case11 : DeauthenticationTestCase
        {
            public Case11(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("11 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase11();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("11 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("certification_invalid", result["errors"]["ClientCertificationBase64"].First().ToString());
            }
        }
        private class Case12 : DeauthenticationTestCase
        {
            public Case12(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("12 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("12 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("certification_invalid", result["errors"]["ClientCertificationBase64"].First().ToString());
            }
        }
        private class Case13 : DeauthenticationTestCase
        {
            public Case13(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("13 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase13();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("13 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("iccid_required", result["errors"]["SimIccId"].First().ToString());
            }
        }
        private class Case14 : DeauthenticationTestCase
        {
            public Case14(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("14 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase14();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("14 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("iccid_is_only_number", result["errors"]["SimIccId"].First().ToString());
            }
        }
        private class Case15 : DeauthenticationTestCase
        {
            public Case15(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("15 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase15();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("15 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("iccid_invalid_length", result["errors"]["SimIccId"].First().ToString());
            }
        }
        private class Case16 : DeauthenticationTestCase
        {
            public Case16(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("16 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase16();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("16 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("imsi_required", result["errors"]["SimImsi"].First().ToString());
            }
        }
        private class Case17 : DeauthenticationTestCase
        {
            public Case17(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("17 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase17();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("17 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("imsi_is_only_number", result["errors"]["SimImsi"].First().ToString());
            }
        }
        private class Case18 : DeauthenticationTestCase
        {
            public Case18(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase18();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("imsi_invalid_length", result["errors"]["SimImsi"].First().ToString());
            }
        }
        private class Case19 : DeauthenticationTestCase
        {
            public Case19(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase19();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("msisdn_required", result["errors"]["SimMsisdn"].First().ToString());
            }
        }
        private class Case20 : DeauthenticationTestCase
        {
            public Case20(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("20 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase20();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("20 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("msisdn_is_only_number", result["errors"]["SimMsisdn"].First().ToString());
            }
        }
        private class Case21 : DeauthenticationTestCase
        {
            public Case21(DeauthenticationRequest deauthenticationRequest, HttpStatusCode httpStatusCode) : base(deauthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(DeauthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("21 Setup");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase21();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("21 AssertThat");
                DeauthenticationUtils.Assert01And02And10To21(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("msisdn_invalid_length", result["errors"]["SimMsisdn"].First().ToString());
            }
        }
    }
}
