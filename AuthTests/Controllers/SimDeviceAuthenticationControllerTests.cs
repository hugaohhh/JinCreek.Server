using JinCreek.Server.Auth;
using JinCreek.Server.AuthTests.Repositories;
using JinCreek.Server.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.AuthTests.Controllers
{
    [Collection("Sequential")]
    public class SimDeviceAuthenticationControllerTests : WebApplicationBase
    {
        private readonly AuthControllerTestRepository _repository;
        private readonly SimDeviceAuthenticationControllerTestSetupRepository _setupRepository;

        private readonly AuthHttpClientWrapper _client;

        public SimDeviceAuthenticationControllerTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
            _setupRepository = new SimDeviceAuthenticationControllerTestSetupRepository(MainDbContext, RadiusDbContext);
            _repository = new AuthControllerTestRepository(MainDbContext, RadiusDbContext);
            _client = new AuthHttpClientWrapper(Client);
        }

        class TestDataClass : IEnumerable<object[]>
        {
            readonly List<object[]> _testData = new List<object[]>();

            public TestDataClass()
            {
                var certificationBase64 = "Q2VydGlmaWNhdGU6CiAgICBEYXRhOgogICAgICAgIFZlcnNpb246IDMgKDB4MikKICAgICAgICBTZXJpYWwgTnVtYmVyOiA0ICgweDQpCiAgICAgICAgU2lnbmF0dXJlIEFsZ29yaXRobTogc2hhMjU2V2l0aFJTQUVuY3J5cHRpb24KICAgICAgICBJc3N1ZXI6IEM9SlAsIFNUPVRva3lvLCBMPUNodW8ta3UsIE89SmluQ3JlZWssIENOPUppbkNyZWVrIENBCiAgICAgICAgVmFsaWRpdHkKICAgICAgICAgICAgTm90IEJlZm9yZTogSmFuIDI5IDEyOjIxOjM4IDIwMjAgR01UCiAgICAgICAgICAgIE5vdCBBZnRlciA6IEphbiAyOCAxMjoyMTozOCAyMDIxIEdNVAogICAgICAgIFN1YmplY3Q6IEM9SlAsIFNUPVRva3lvLCBPPUppbkNyZWVrLCBDTj1KSU5DUkVFSy1QQwogICAgICAgIFN1YmplY3QgUHVibGljIEtleSBJbmZvOgogICAgICAgICAgICBQdWJsaWMgS2V5IEFsZ29yaXRobTogcnNhRW5jcnlwdGlvbgogICAgICAgICAgICAgICAgUlNBIFB1YmxpYy1LZXk6ICgyMDQ4IGJpdCkKICAgICAgICAgICAgICAgIE1vZHVsdXM6CiAgICAgICAgICAgICAgICAgICAgMDA6Yjg6ZGY6Zjc6YTY6Y2E6MDQ6Nzk6ZWM6MDQ6YzQ6MDM6ZDQ6NjY6ZTk6CiAgICAgICAgICAgICAgICAgICAgNTM6Mzg6YzQ6YjA6Nzg6YmE6ZTQ6ZTY6NjA6OWM6ZTA6ZDE6Mzk6MmY6MjE6CiAgICAgICAgICAgICAgICAgICAgYTc6ZTk6YWU6NTU6OGU6NjA6NzY6NTA6Yzc6Mzk6MDM6OTI6MDc6Njc6ZDU6CiAgICAgICAgICAgICAgICAgICAgNTU6Zjg6MmU6Njc6MjU6ZGM6NzM6ODk6MjE6NmI6ODc6OTQ6MGU6YWM6NGU6CiAgICAgICAgICAgICAgICAgICAgYzA6YTQ6Mzk6Y2U6Yzg6NDA6Zjk6ODc6ZGU6ZjE6MzE6NGQ6ZGU6ZmE6ODc6CiAgICAgICAgICAgICAgICAgICAgMmU6OTQ6ZTA6MmM6MWI6NmQ6YWQ6NWU6MTI6OTU6Zjc6Mzg6OGE6OWM6ZDA6CiAgICAgICAgICAgICAgICAgICAgY2M6YzY6ODk6ZTc6ZTU6MDY6ODI6YjY6N2M6YTY6MjY6MTU6MDc6Njg6Mzk6CiAgICAgICAgICAgICAgICAgICAgMDU6MWI6MWU6MDA6NDk6MzA6ZDA6YzY6Y2M6MDY6OTY6NDE6OTA6MzA6NzU6CiAgICAgICAgICAgICAgICAgICAgNzg6NTk6MzI6Njk6N2U6YzM6Y2Y6MzE6MzU6ZTQ6NDk6ZGU6NDI6YzA6MDc6CiAgICAgICAgICAgICAgICAgICAgMzI6ZjM6MTk6ZWU6Njc6NjE6YTI6YTA6Zjg6YjQ6NDA6OTY6ODY6NTY6ZTc6CiAgICAgICAgICAgICAgICAgICAgMmY6YTk6MzA6YWI6OTk6NWQ6NDY6YzY6ZTU6Yjk6OTc6ZjY6NmE6YTM6Y2U6CiAgICAgICAgICAgICAgICAgICAgNGI6MWU6Njc6NDI6N2M6ODY6Zjk6NDI6OGQ6N2Q6Yzg6MWU6ZWI6MDA6NmE6CiAgICAgICAgICAgICAgICAgICAgMTk6OTk6NjI6NDE6M2I6ZDg6NjE6ZTQ6NzU6MmI6M2E6OGI6NjU6NDc6NTc6CiAgICAgICAgICAgICAgICAgICAgOWM6ODE6NDk6NDI6ODk6MmI6OTM6MjE6ZjQ6YmU6MDU6MmE6MDc6M2Y6MjI6CiAgICAgICAgICAgICAgICAgICAgZmY6MjE6Y2E6Nzg6NmI6MmE6YTM6Yzk6ZDY6MDc6NjY6OWI6MmQ6NTQ6Y2Y6CiAgICAgICAgICAgICAgICAgICAgZTY6ZmE6MDg6MmU6N2U6OTA6NTg6ZjA6N2Q6MjQ6MmU6NTk6NWQ6NzQ6YTE6CiAgICAgICAgICAgICAgICAgICAgMTY6YWY6NzU6MTk6N2E6OGY6YjY6NDU6NjU6N2E6Nzk6Y2E6NWU6MzU6YTQ6CiAgICAgICAgICAgICAgICAgICAgMDI6YjcKICAgICAgICAgICAgICAgIEV4cG9uZW50OiA2NTUzNyAoMHgxMDAwMSkKICAgICAgICBYNTA5djMgZXh0ZW5zaW9uczoKICAgICAgICAgICAgWDUwOXYzIEJhc2ljIENvbnN0cmFpbnRzOiAKICAgICAgICAgICAgICAgIENBOkZBTFNFCiAgICAgICAgICAgIE5ldHNjYXBlIENlcnQgVHlwZTogCiAgICAgICAgICAgICAgICBTU0wgQ2xpZW50LCBTL01JTUUsIE9iamVjdCBTaWduaW5nCiAgICAgICAgICAgIE5ldHNjYXBlIENvbW1lbnQ6IAogICAgICAgICAgICAgICAgT3BlblNTTCBHZW5lcmF0ZWQgQ2VydGlmaWNhdGUKICAgICAgICAgICAgWDUwOXYzIFN1YmplY3QgS2V5IElkZW50aWZpZXI6IAogICAgICAgICAgICAgICAgMEE6REI6RTY6NTM6QUQ6Rjc6M0Q6MjM6MEY6RjU6OTU6ODQ6Mzk6MDE6NEI6Q0E6QUU6N0Y6RjY6NDkKICAgICAgICAgICAgWDUwOXYzIEF1dGhvcml0eSBLZXkgSWRlbnRpZmllcjogCiAgICAgICAgICAgICAgICBrZXlpZDpFQzpDRjpGMDo4NzoxRToyQTo0NzpEODo3MjoyQzo1RTo1Qjo5Qjo4Qjo5Njo5QjoxQzpBQTo1ODo0MQoKICAgIFNpZ25hdHVyZSBBbGdvcml0aG06IHNoYTI1NldpdGhSU0FFbmNyeXB0aW9uCiAgICAgICAgIDg3OmY4OmMxOjI5OmFmOmEwOjhjOjY4OjU2OjgwOjNlOjM2OjBhOjU2OmM3OjdmOjY1OmNiOgogICAgICAgICAzYzpjMzoxNjoxZDpmMjo3ZTozMjplZDplMzoyOTo4NjphMTo5Yzo3MjpmYzo3Yjo5YzpmZToKICAgICAgICAgMGY6ZGE6ZTI6Mzc6NDQ6ZjM6NWM6NTk6ZTk6NDQ6MTY6MDE6NWY6MTE6ZTA6M2E6NzU6NjQ6CiAgICAgICAgIGZmOjY0OmJiOmI2OjEwOmVjOjZlOjc2OmU4OjY1OjUzOjcxOjUwOjgzOmEzOjVlOjAxOmNlOgogICAgICAgICA0ZjpiZTplNTo1MDo2ODo2Zjo1NTpjMTphYTpmNDpjODpjNTpiZjpkNzoyODoyZjo1ZDowNjoKICAgICAgICAgNzM6MmY6ZTU6MjU6NDY6ZWM6OTE6Y2M6MDA6N2U6YjA6YzY6NDY6OWQ6NTQ6NDU6Y2Q6NmY6CiAgICAgICAgIGUyOjYzOmI3OjZhOjBmOjc5OmFjOmUyOjk2OmVlOjEzOjNjOmUxOjg1OjQ0OjQ1OjAwOjBhOgogICAgICAgICA4MjpkZjo4Mjo1Yjo2MTphNDo3ZDphNjo2YTo3NToxZToxOTo2ZDplOTplZTo1Mzo2ODo4MjoKICAgICAgICAgZTY6NTY6ZmU6NzQ6NjI6MDc6NjM6N2E6Mzk6NjI6OTE6MTQ6ZTk6NmU6YTE6NzU6Y2Y6MjY6CiAgICAgICAgIDg4OmEyOjdkOmZiOjE5OmMzOjgyOjRjOjViOmZkOjkxOjU2OmMxOjQ0OjM0OmVlOjZiOjM0OgogICAgICAgICAxZjozOTplZDoxZjo2Njo4YjpmYjpiNDoyMTpjYjo5YTphNjplNzpiZDowMjo1NTplYzplZDoKICAgICAgICAgMjM6MGU6MTY6NWY6ZDc6YjU6YzY6NzA6Njg6ZWM6NjI6YmM6YmM6MWM6NzQ6NzE6OWU6MDY6CiAgICAgICAgIDAzOmMyOjNkOjhhOjk4OmUyOjBkOjljOmFlOmU3OjBhOjRjOjBlOjRkOmY0OmE3OjJiOmQ0OgogICAgICAgICBhNTo3ZTpjMTphZjo4Mjo1MjpmYzoxMjo0ZjoxYjo4NDphOTo1OTo5ZDozMDo5ZDpmYjpjZjoKICAgICAgICAgYzY6ZWU6NmY6MDYKLS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSURxVENDQXBHZ0F3SUJBZ0lCQkRBTkJna3Foa2lHOXcwQkFRc0ZBREJZTVFzd0NRWURWUVFHRXdKS1VERU8KTUF3R0ExVUVDQXdGVkc5cmVXOHhFREFPQmdOVkJBY01CME5vZFc4dGEzVXhFVEFQQmdOVkJBb01DRXBwYmtOeQpaV1ZyTVJRd0VnWURWUVFEREF0S2FXNURjbVZsYXlCRFFUQWVGdzB5TURBeE1qa3hNakl4TXpoYUZ3MHlNVEF4Ck1qZ3hNakl4TXpoYU1FWXhDekFKQmdOVkJBWVRBa3BRTVE0d0RBWURWUVFJREFWVWIydDViekVSTUE4R0ExVUUKQ2d3SVNtbHVRM0psWldzeEZEQVNCZ05WQkFNTUMwcEpUa05TUlVWTExWQkRNSUlCSWpBTkJna3Foa2lHOXcwQgpBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUF1Ti8zcHNvRWVld0V4QVBVWnVsVE9NU3dlTHJrNW1DYzRORTVMeUduCjZhNVZqbUIyVU1jNUE1SUhaOVZWK0M1bkpkeHppU0ZyaDVRT3JFN0FwRG5PeUVENWg5N3hNVTNlK29jdWxPQXMKRzIydFhoS1Y5emlLbk5ETXhvbm41UWFDdG55bUpoVUhhRGtGR3g0QVNURFF4c3dHbGtHUU1IVjRXVEpwZnNQUApNVFhrU2Q1Q3dBY3k4eG51WjJHaW9QaTBRSmFHVnVjdnFUQ3JtVjFHeHVXNWwvWnFvODVMSG1kQ2ZJYjVRbzE5CnlCN3JBR29abVdKQk85aGg1SFVyT290bFIxZWNnVWxDaVN1VElmUytCU29IUHlML0ljcDRheXFqeWRZSFpwc3QKVk0vbStnZ3VmcEJZOEgwa0xsbGRkS0VXcjNVWmVvKzJSV1Y2ZWNwZU5hUUN0d0lEQVFBQm80R1BNSUdNTUFrRwpBMVVkRXdRQ01BQXdFUVlKWUlaSUFZYjRRZ0VCQkFRREFnU3dNQ3dHQ1dDR1NBR0crRUlCRFFRZkZoMVBjR1Z1ClUxTk1JRWRsYm1WeVlYUmxaQ0JEWlhKMGFXWnBZMkYwWlRBZEJnTlZIUTRFRmdRVUN0dm1VNjMzUFNNUDlaV0UKT1FGTHlxNS85a2t3SHdZRFZSMGpCQmd3Rm9BVTdNL3doeDRxUjloeUxGNWJtNHVXbXh5cVdFRXdEUVlKS29aSQpodmNOQVFFTEJRQURnZ0VCQUlmNHdTbXZvSXhvVm9BK05ncFd4MzlseXp6REZoM3lmakx0NHltR29aeHkvSHVjCi9nL2E0amRFODF4WjZVUVdBVjhSNERwMVpQOWt1N1lRN0c1MjZHVlRjVkNEbzE0QnprKys1VkJvYjFYQnF2VEkKeGIvWEtDOWRCbk12NVNWRzdKSE1BSDZ3eGthZFZFWE5iK0pqdDJvUGVhemlsdTRUUE9HRlJFVUFDb0xmZ2x0aApwSDJtYW5VZUdXM3A3bE5vZ3VaVy9uUmlCMk42T1dLUkZPbHVvWFhQSm9paWZmc1p3NEpNVy8yUlZzRkVOTzVyCk5CODU3UjltaS91MEljdWFwdWU5QWxYczdTTU9GbC9YdGNad2FPeGl2THdjZEhHZUJnUENQWXFZNGcyY3J1Y0sKVEE1TjlLY3IxS1Yrd2ErQ1V2d1NUeHVFcVZtZE1KMzd6OGJ1YndZPQotLS0tLUVORCBDRVJUSUZJQ0FURS0tLS0tCg==";
                var certificationBase64InvalidBase64 = "ABCDEFG";
                var certificationBase64InvalidCert = "QUJDREVGRwo=";

                _testData.Add(new object[] { new Case01(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = "", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case02(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64InvalidBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case03(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64InvalidCert, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case04(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case05(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "abcdef123456", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case06(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "89811000038174800001111111111111", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case07(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case08(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "abcdef123456", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case09(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "44010321310000011111111111", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case10(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981080005819480000", SimImsi = "440103213100000", SimMsisdn = "" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case11(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "abcdef123456" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case12(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000111111111111111111111" }, HttpStatusCode.BadRequest) });

                // #13,#14はリクエストからのDeviceIpAddrの廃止により削除
                //_testData.Add(new object[] { new Case1301(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1302(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1303(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1304(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1305(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1306(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1307(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1308(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1309(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1310(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1311(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1312(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1313(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1314(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1315(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1316(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1317(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case1318(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case14(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest) });

                _testData.Add(new object[] { new Case15(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case16(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case17(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized) });

                _testData.Add(new object[] { new Case1801(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1802(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1803(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1804(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1805(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1806(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1807(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1808(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1809(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1810(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1811(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1812(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1813(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1814(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1815(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1816(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1817(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1818(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });

                _testData.Add(new object[] { new Case1901(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1902(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1903(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1904(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1905(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1906(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1907(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1908(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1909(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1910(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1911(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1912(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1913(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1914(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1915(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1916(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1917(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case1918(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });

                _testData.Add(new object[] { new Case20(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case21(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case2201(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2202(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2203(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2204(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2205(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2206(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2207(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2208(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2209(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2210(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2211(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2212(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2213(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2214(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2215(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2216(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2217(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case2218(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case23(new SimDeviceAuthenticationRequest() { ClientCertificationBase64 = certificationBase64, SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.Unauthorized) });
            }

            public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory(DisplayName = "SIM&端末認証APIテスト")]
        [ClassData(typeof(TestDataClass))]
        public void TestWitDb(SimDeviceTestCase simDeviceTestCase)
        {
            TestOutputHelper.WriteLine($"{simDeviceTestCase}");
            simDeviceTestCase.Test(TestOutputHelper, _repository, _setupRepository, _client);
        }

        private static class Utils
        {
            private static void AssertExistsLogonUsers(List<String> expectedList, JObject result)
            {
                var list = result["CanLogonUsers"].ToImmutableList();
                Assert.NotEmpty(list);
                var resultList = list.Select(l => l.Value<string>()).ToList();

                expectedList.Sort();
                resultList.Sort();

                Assert.True(expectedList.SequenceEqual(resultList));
            }

            private static void AssertNotExistsLogonUsers(JObject result)
            {
                var list = result["CanLogonUsers"].ToImmutableList();
                Assert.Empty(list);
            }

            private static void AssertOkResponse(AuthControllerTestRepository authControllerTestRepository, JObject result)
            {
                Assert.Null(result["ErrorCode"]);
                Assert.Null(result["ErrorMessage"]);
                Assert.NotNull(result["AuthId"]);
                Assert.NotNull(result["AssignDeviceIpAddress"]);
                Assert.NotNull(result["AuthenticationDuration"]);
                var simAndAuthenticated = authControllerTestRepository.GetSimAndDeviceAuthenticated(Guid.Parse(result["AuthId"].ToString()));
                Assert.NotNull(simAndAuthenticated);
                Assert.NotNull(simAndAuthenticated.SimAndDevice);

                var simAndDeviceAuthenticationSuccessLog = authControllerTestRepository.GetSimAndDeviceAuthenticationSuccessLog(simAndAuthenticated.SimAndDevice.Id);
                Assert.NotNull(simAndDeviceAuthenticationSuccessLog);

                var simAndDeviceAuthenticationFailureLog = authControllerTestRepository.GetSimAndDeviceAuthenticationFailureLog(simAndAuthenticated.SimAndDevice.Sim.Id);
                Assert.Null(simAndDeviceAuthenticationFailureLog);

                var testRadreply = authControllerTestRepository.GetRadreply(simAndAuthenticated.SimAndDevice.Sim.UserName + "@" + simAndAuthenticated.SimAndDevice.Sim.SimGroup.UserNameSuffix);

                Assert.Equal(simAndAuthenticated.SimAndDevice.IsolatedNw2Ip, testRadreply.Value);
            }

            private static void AssertCaseXxWithEndUser(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acutualRessult)
            {
                Assert.Equal(HttpStatusCode.OK, acutualRessult.StatusCode);
                var result = JObject.Parse(acutualRessult.Content.ReadAsStringAsync().Result);
                AssertOkResponse(authControllerTestRepository, result);
                AssertExistsLogonUsers(new List<string> { "AdminAccount", "GeneralAccount", "AdminAccount_OtherSimSameDevice", "GeneralAccount_OtherSimSameDevice" }, result);
            }

            private static void AssertCaseXxWithEndUserOthersOnly(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acutualRessult)
            {
                Assert.Equal(HttpStatusCode.OK, acutualRessult.StatusCode);
                var result = JObject.Parse(acutualRessult.Content.ReadAsStringAsync().Result);
                AssertOkResponse(authControllerTestRepository, result);
                AssertExistsLogonUsers(new List<string> { "AdminAccount_OtherSimSameDevice", "GeneralAccount_OtherSimSameDevice" }, result);
            }

            private static void AssertCaseXxWithoutEndUser(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acutualRessult)
            {
                Assert.Equal(HttpStatusCode.OK, acutualRessult.StatusCode);
                var result = JObject.Parse(acutualRessult.Content.ReadAsStringAsync().Result);
                AssertOkResponse(authControllerTestRepository, result);
                AssertNotExistsLogonUsers(result);
            }

            public static void AssertCase13_18_19_22WithEndUser(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acutualRessult)
            {
                AssertCaseXxWithEndUser(authControllerTestRepository, acutualRessult);
            }

            public static void AssertCase13_18_19_22WithEndUserOthersOnly(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acutualRessult)
            {
                AssertCaseXxWithEndUserOthersOnly(authControllerTestRepository, acutualRessult);
            }

            public static void AssertCase13_18_19_22WithoutEndUser(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acutualRessult)
            {
                AssertCaseXxWithoutEndUser(authControllerTestRepository, acutualRessult);
            }

            public static void AssertCase15_16_17_20_21_23(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("1001", result["ErrorCode"]);
                Assert.Equal("Not Match SimAndDevice Info", result["ErrorMessage"]);
                var simDeviceAuthenticationLogSuccesses = authControllerTestRepository.GetSimDeviceAuthenticationLogSuccess();
                Assert.Empty(simDeviceAuthenticationLogSuccesses);
                var simDeviceAuthenticationStateDones = authControllerTestRepository.GetAllSimAndDeviceAuthenticated();
                Assert.Empty(simDeviceAuthenticationStateDones);
                var simDeviceAuthenticationLogFails = authControllerTestRepository.GetSimDeviceAuthenticationLogFail();
                Assert.NotEmpty(simDeviceAuthenticationLogFails);
                var radreply = authControllerTestRepository.GetRadreplys("user1@jincreek2");
                Assert.Single(radreply);
                Assert.Equal("Nw1Address", radreply.First().Value);
            }

            public static void AssertCaseXx(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.BadRequest, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.NotNull(result["traceId"]);
                Assert.Equal("One or more validation errors occurred.", result["title"]);
                Assert.Equal("400", result["status"]);
                var simDeviceAuthenticationLogFails = authControllerTestRepository.GetSimDeviceAuthenticationLogFail();
                Assert.Empty(simDeviceAuthenticationLogFails);
                var simDeviceAuthenticationLogSuccesses = authControllerTestRepository.GetSimDeviceAuthenticationLogSuccess();
                Assert.Empty(simDeviceAuthenticationLogSuccesses);
                var simDeviceAuthenticationStateDones = authControllerTestRepository.GetAllSimAndDeviceAuthenticated();
                Assert.Empty(simDeviceAuthenticationStateDones);
                var radreply = authControllerTestRepository.GetRadreplys("user1@jincreek2");
                Assert.Single(radreply);
                Assert.Equal("Nw1Address", radreply.First().Value);
            }
        }

        public abstract class SimDeviceTestCase
        {
            private readonly SimDeviceAuthenticationRequest _simDeviceAuthenticationRequest;

            // setterのため
            [SuppressMessage("ReSharper", "NotAccessedField.Local")]
            private HttpStatusCode _httpStatusCode;

            // setterのため
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            protected ITestOutputHelper _testOutputHelper;

            protected SimDeviceTestCase(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode)
            {
                _simDeviceAuthenticationRequest = simDeviceAuthenticationRequest;
                _httpStatusCode = httpStatusCode;
            }

            public virtual void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                setupRepository.SetUpInsertBaseData();
            }

            public abstract void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage);

            public void Test(ITestOutputHelper testOutputHelper, AuthControllerTestRepository authControllerTestRepository, SimDeviceAuthenticationControllerTestSetupRepository setupRepository, AuthHttpClientWrapper httpClientWrapper)
            {
                _testOutputHelper = testOutputHelper;
                Test(authControllerTestRepository, setupRepository, httpClientWrapper);
            }

            public void Test(AuthControllerTestRepository authControllerTestRepository, SimDeviceAuthenticationControllerTestSetupRepository setupRepository, AuthHttpClientWrapper httpClientWrapper)
            {
                SetUp(setupRepository);
                var response = httpClientWrapper.PostSimDeviceAuthentication(_simDeviceAuthenticationRequest);
                AssertThat(authControllerTestRepository, response);
            }
        }

        private abstract class ValidateSimDeviceTestCase : SimDeviceTestCase
        {
            protected ValidateSimDeviceTestCase(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case01 : ValidateSimDeviceTestCase
        {
            public Case01(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("01 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("01 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("certification_required", result["errors"]["ClientCertificationBase64"].First().ToString());
            }
        }

        private class Case02 : ValidateSimDeviceTestCase
        {
            public Case02(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("02 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("02 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("certification_invalid", result["errors"]["ClientCertificationBase64"].First().ToString());
            }
        }

        private class Case03 : ValidateSimDeviceTestCase
        {
            public Case03(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("03 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("03 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("certification_invalid", result["errors"]["ClientCertificationBase64"].First().ToString());
            }
        }


        private class Case04 : ValidateSimDeviceTestCase
        {
            public Case04(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("04 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("04 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("iccid_required", result["errors"]["SimIccId"].First().ToString());
            }
        }
        private class Case05 : ValidateSimDeviceTestCase
        {
            public Case05(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("05 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("05 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("iccid_is_only_number", result["errors"]["SimIccId"].First().ToString());
            }
        }
        private class Case06 : ValidateSimDeviceTestCase
        {
            public Case06(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("06 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("06 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("iccid_invalid_length", result["errors"]["SimIccId"].First().ToString());
            }
        }
        private class Case07 : ValidateSimDeviceTestCase
        {
            public Case07(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("07 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("07 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("imsi_required", result["errors"]["SimImsi"].First().ToString());
            }
        }
        private class Case08 : ValidateSimDeviceTestCase
        {
            public Case08(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("08 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("08 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("imsi_is_only_number", result["errors"]["SimImsi"].First().ToString());
            }
        }
        private class Case09 : ValidateSimDeviceTestCase
        {
            public Case09(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("09 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("09 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("imsi_invalid_length", result["errors"]["SimImsi"].First().ToString());
            }
        }
        private class Case10 : ValidateSimDeviceTestCase
        {
            public Case10(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("10 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("10 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("msisdn_required", result["errors"]["SimMsisdn"].First().ToString());
            }
        }
        private class Case11 : ValidateSimDeviceTestCase
        {
            public Case11(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("11 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("11 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("msisdn_is_only_number", result["errors"]["SimMsisdn"].First().ToString());
            }
        }
        private class Case12 : ValidateSimDeviceTestCase
        {
            public Case12(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("12 Setup");
                setupRepository.SetUpInsertDataForCase01_12();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("12 AssertThat");
                Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("msisdn_invalid_length", result["errors"]["SimMsisdn"].First().ToString());
            }
        }
        //private class Case14 : ValidateSimDeviceTestCase
        //{
        //    public Case14(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }

        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("14 Setup");
        //        setupRepository.SetUpInsertDataForCase14();
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("14 AssertThat");
        //        Utils.AssertCaseXx(authControllerTestRepository, acualRessult);
        //        var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
        //        Assert.Equal("device_ip_address_invalid", result["errors"]["DeviceIpAddress"].First().ToString());
        //    }
        //}

        //private abstract class Case13 : SimDeviceTestCase
        //{
        //    protected Case13(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1301 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_01 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1301();
        //        _testOutputHelper.WriteLine("13_01 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_01 AssertThat");
        //        Utils.AssertCase13_18_19_22WithoutEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1301(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1302 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_02 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1302();
        //        _testOutputHelper.WriteLine("13_02 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_02 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1302(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1303 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_03 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1303();
        //        _testOutputHelper.WriteLine("13_03 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_03 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1303(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1304 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_04 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1304();
        //        _testOutputHelper.WriteLine("13_04 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_04 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1304(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1305 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_05 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1305();
        //        _testOutputHelper.WriteLine("13_05 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_05 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1305(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1306 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_06 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1306();
        //        _testOutputHelper.WriteLine("13_06 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_06 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1306(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1307 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_07 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1307();
        //        _testOutputHelper.WriteLine("13_07 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_07 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1307(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1308 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_08 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1308();
        //        _testOutputHelper.WriteLine("13_08 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_08 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1308(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1309 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_09 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1309();
        //        _testOutputHelper.WriteLine("13_09 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_09 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1309(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1310 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_10 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1310();
        //        _testOutputHelper.WriteLine("13_10 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_10 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1310(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1311 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_11 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1311();
        //        _testOutputHelper.WriteLine("13_11 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_11 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1311(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1312 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_12 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1312();
        //        _testOutputHelper.WriteLine("13_12 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_12 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1312(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1313 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_13 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1313();
        //        _testOutputHelper.WriteLine("13_13 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_13 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1313(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1314 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_14 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1314();
        //        _testOutputHelper.WriteLine("13_14 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_14 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1314(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1315 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_15 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1315();
        //        _testOutputHelper.WriteLine("13_15 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_15 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1315(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1316 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_16 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1316();
        //        _testOutputHelper.WriteLine("13_16 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_03 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1316(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1317 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_17 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1317();
        //        _testOutputHelper.WriteLine("13_17 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_17 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1317(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case1318 : Case13
        //{
        //    public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("13_18 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase1318();
        //        _testOutputHelper.WriteLine("13_18 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("13_18 AssertThat");
        //        Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
        //    }

        //    public Case1318(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        private class Case15 : SimDeviceTestCase
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("15 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase15();
                _testOutputHelper.WriteLine("15 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("15 AssertThat");
                Utils.AssertCase15_16_17_20_21_23(authControllerTestRepository, acualRessult);
            }

            public Case15(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case16 : SimDeviceTestCase
        {

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("16 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase16();
                _testOutputHelper.WriteLine("16 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("16 AssertThat");
                Utils.AssertCase15_16_17_20_21_23(authControllerTestRepository, acualRessult);
            }

            public Case16(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case17 : SimDeviceTestCase
        {

            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("17 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase17();
                _testOutputHelper.WriteLine("17 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("17 AssertThat");
                Utils.AssertCase15_16_17_20_21_23(authControllerTestRepository, acualRessult);
            }

            public Case17(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private abstract class Case18 : SimDeviceTestCase
        {
            protected Case18(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1801 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_01 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1801();
                _testOutputHelper.WriteLine("18_01 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_01 AssertThat");
                Utils.AssertCase13_18_19_22WithoutEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1801(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1802 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_02 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1802();
                _testOutputHelper.WriteLine("18_02 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_02 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1802(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1803 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_03 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1803();
                _testOutputHelper.WriteLine("18_03 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_03 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1803(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1804 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_04 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1804();
                _testOutputHelper.WriteLine("18_04 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_04 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1804(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1805 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_05 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1805();
                _testOutputHelper.WriteLine("18_05 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_05 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1805(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1806 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_06 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1806();
                _testOutputHelper.WriteLine("18_06 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_06 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1806(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1807 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_07 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1807();
                _testOutputHelper.WriteLine("18_07 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_07 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1807(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1808 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_08 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1808();
                _testOutputHelper.WriteLine("18_08 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_08 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1808(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1809 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_09 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1809();
                _testOutputHelper.WriteLine("18_09 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_09 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1809(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1810 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_10 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1810();
                _testOutputHelper.WriteLine("18_10 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_10 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1810(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1811 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_11 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1811();
                _testOutputHelper.WriteLine("18_11 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_11 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1811(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1812 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_12 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1812();
                _testOutputHelper.WriteLine("18_12 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_12 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1812(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1813 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_13 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1813();
                _testOutputHelper.WriteLine("18_13 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_13 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1813(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1814 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_14 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1814();
                _testOutputHelper.WriteLine("18_14 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_14 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1814(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1815 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_15 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1815();
                _testOutputHelper.WriteLine("18_15 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_15 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1815(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1816 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_16 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1816();
                _testOutputHelper.WriteLine("18_16 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_03 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1816(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1817 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_17 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1817();
                _testOutputHelper.WriteLine("18_17 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_17 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1817(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1818 : Case18
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18_18 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1818();
                _testOutputHelper.WriteLine("18_18 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("18_18 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1818(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private abstract class Case19 : SimDeviceTestCase
        {
            protected Case19(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1901 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_01 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1901();
                _testOutputHelper.WriteLine("19_01 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_01 AssertThat");
                Utils.AssertCase13_18_19_22WithoutEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1901(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1902 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_02 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1902();
                _testOutputHelper.WriteLine("19_02 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_02 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1902(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1903 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_03 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1903();
                _testOutputHelper.WriteLine("19_03 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_03 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1903(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1904 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_04 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1904();
                _testOutputHelper.WriteLine("19_04 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_04 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1904(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1905 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_05 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1905();
                _testOutputHelper.WriteLine("19_05 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_05 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1905(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1906 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_06 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1906();
                _testOutputHelper.WriteLine("19_06 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_06 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1906(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1907 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_07 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1907();
                _testOutputHelper.WriteLine("19_07 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_07 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1907(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1908 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_08 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1908();
                _testOutputHelper.WriteLine("19_08 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_08 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1908(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1909 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_09 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1909();
                _testOutputHelper.WriteLine("19_09 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_09 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1909(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1910 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_10 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1910();
                _testOutputHelper.WriteLine("19_10 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_10 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case1910(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1911 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_11 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1911();
                _testOutputHelper.WriteLine("19_11 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_11 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1911(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1912 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_12 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1912();
                _testOutputHelper.WriteLine("19_12 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_12 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1912(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1913 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_13 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1913();
                _testOutputHelper.WriteLine("19_13 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_13 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1913(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1914 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_14 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1914();
                _testOutputHelper.WriteLine("19_14 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_14 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1914(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1915 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_15 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1915();
                _testOutputHelper.WriteLine("19_15 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_15 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1915(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1916 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_16 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1916();
                _testOutputHelper.WriteLine("19_16 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_03 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1916(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1917 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_17 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1917();
                _testOutputHelper.WriteLine("19_17 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_17 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1917(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case1918 : Case19
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19_18 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase1918();
                _testOutputHelper.WriteLine("19_18 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19_18 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case1918(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case20 : SimDeviceTestCase
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("20 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase20();
                _testOutputHelper.WriteLine("20 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("20 AssertThat");
                Utils.AssertCase15_16_17_20_21_23(authControllerTestRepository, acualRessult);
            }

            public Case20(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case21 : SimDeviceTestCase
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("21 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase21();
                _testOutputHelper.WriteLine("21 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("21 AssertThat");
                Utils.AssertCase15_16_17_20_21_23(authControllerTestRepository, acualRessult);
            }

            public Case21(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private abstract class Case22 : SimDeviceTestCase
        {
            protected Case22(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2201 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_01 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2201();
                _testOutputHelper.WriteLine("22_01 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_01 AssertThat");
                Utils.AssertCase13_18_19_22WithoutEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2201(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2202 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_02 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2202();
                _testOutputHelper.WriteLine("22_02 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_02 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2202(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2203 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_03 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2203();
                _testOutputHelper.WriteLine("22_03 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_03 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2203(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2204 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_04 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2204();
                _testOutputHelper.WriteLine("22_04 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_04 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2204(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2205 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_05 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2205();
                _testOutputHelper.WriteLine("22_05 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_05 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2205(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2206 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_06 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2206();
                _testOutputHelper.WriteLine("22_06 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_06 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2206(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2207 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_07 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2207();
                _testOutputHelper.WriteLine("22_07 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_07 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2207(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2208 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_08 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2208();
                _testOutputHelper.WriteLine("22_08 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_08 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2208(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2209 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_09 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2209();
                _testOutputHelper.WriteLine("22_09 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_09 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2209(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2210 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_10 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2210();
                _testOutputHelper.WriteLine("22_10 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_10 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUserOthersOnly(authControllerTestRepository, acualRessult);
            }

            public Case2210(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2211 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_11 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2211();
                _testOutputHelper.WriteLine("22_11 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_11 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2211(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2212 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_12 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2212();
                _testOutputHelper.WriteLine("22_12 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_12 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2212(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2213 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_13 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2213();
                _testOutputHelper.WriteLine("22_13 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_13 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2213(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2214 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_14 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2214();
                _testOutputHelper.WriteLine("22_14 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_14 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2214(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2215 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_15 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2215();
                _testOutputHelper.WriteLine("22_15 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_15 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2215(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2216 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_16 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2216();
                _testOutputHelper.WriteLine("22_16 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_03 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2216(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2217 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_17 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2217();
                _testOutputHelper.WriteLine("22_17 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_17 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2217(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case2218 : Case22
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("22_18 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase2218();
                _testOutputHelper.WriteLine("22_18 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("22_18 AssertThat");
                Utils.AssertCase13_18_19_22WithEndUser(authControllerTestRepository, acualRessult);
            }

            public Case2218(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }


        private class Case23 : SimDeviceTestCase
        {
            public override void SetUp(SimDeviceAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("23 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase23();
                _testOutputHelper.WriteLine("23 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("23 AssertThat");
                Utils.AssertCase15_16_17_20_21_23(authControllerTestRepository, acualRessult);
            }

            public Case23(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
        }
    }
}
