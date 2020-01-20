using JinCreek.Server.Auth;
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
    public class SimDeviceAuthenticationControllerValidateTests : WebApplicationBase
    {
        public SimDeviceAuthenticationControllerValidateTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ValidateAssertType
        {
            IMEI_REQUIRED = 1,
            IMEI_INVALID = 2,
            ICCID_REQUIRED = 3,
            ICCID_INVALID = 4,
            IMSI_REQUIRED = 5,
            IMSI_INVALID = 6,
            MSISDN_REQUIRED = 7,
            MSISDN_INVALID = 8,
            IP_INVALID = 9,
            IMEI_NUMBER_INVALID = 10,
            ICCID_NUMBER_INVALID = 11,
            IMSI_NUMBER_INVALID = 12,
            MSISDN_NUMBER_INVALID = 13
        }

        class TestDataClass : IEnumerable<object[]>
        {
            List<object[]> _testData = new List<object[]>();

            public TestDataClass()
            {
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest, ValidateAssertType.IMEI_REQUIRED });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "abcdef123456", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest ,ValidateAssertType.IMEI_NUMBER_INVALID});
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000111", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest, ValidateAssertType.IMEI_INVALID });

                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest, ValidateAssertType.ICCID_REQUIRED });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "abcdef123456", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest ,ValidateAssertType.ICCID_NUMBER_INVALID});
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000111", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest, ValidateAssertType.ICCID_INVALID });

                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest, ValidateAssertType.IMSI_REQUIRED });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "abcdef123456", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest ,ValidateAssertType.IMSI_NUMBER_INVALID});
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "4401032131000001111", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest, ValidateAssertType.IMSI_INVALID });

                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "" }, HttpStatusCode.BadRequest, ValidateAssertType.MSISDN_REQUIRED });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "abcdef123456" }, HttpStatusCode.BadRequest ,ValidateAssertType.MSISDN_NUMBER_INVALID});
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "020179110001111111111" }, HttpStatusCode.BadRequest, ValidateAssertType.MSISDN_INVALID });
                //case14
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceIpAddress = "aaabbbccc", DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.BadRequest, ValidateAssertType.IP_INVALID });
            }

            public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory(DisplayName = "バリデーションテスト")]
        [ClassData(typeof(TestDataClass))]
        public void Validation(SimDeviceAuthenticationRequest request, HttpStatusCode statusCode, ValidateAssertType type)
        {
            var acualRessult = PostSimDeviceAuthentication(request);

            Assert.Equal(statusCode, acualRessult.StatusCode);
            var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);

            Assert.NotNull(result["traceId"]);
            Assert.Equal("One or more validation errors occurred.", result["title"]);
            Assert.Equal("400", result["status"]);
            switch (type)
            {
                case ValidateAssertType.IMEI_REQUIRED: Assert.Equal("imei_required", result["errors"]["DeviceImei"].First().ToString()); break;
                case ValidateAssertType.IMEI_INVALID: Assert.Equal("imei_invalid_length", result["errors"]["DeviceImei"].First().ToString()); break;
                case ValidateAssertType.IMEI_NUMBER_INVALID: Assert.Equal("imei_is_only_number", result["errors"]["DeviceImei"].First().ToString()); break;
                case ValidateAssertType.IMSI_REQUIRED: Assert.Equal("imsi_required", result["errors"]["SimImsi"].First().ToString()); break;
                case ValidateAssertType.IMSI_INVALID: Assert.Equal("imsi_invalid_length", result["errors"]["SimImsi"].First().ToString()); break;
                case ValidateAssertType.IMSI_NUMBER_INVALID: Assert.Equal("imsi_is_only_number", result["errors"]["SimImsi"].First().ToString()); break;
                case ValidateAssertType.ICCID_REQUIRED: Assert.Equal("iccid_required", result["errors"]["SimIccId"].First().ToString()); break;
                case ValidateAssertType.ICCID_INVALID: Assert.Equal("iccid_invalid_length", result["errors"]["SimIccId"].First().ToString()); break;
                case ValidateAssertType.ICCID_NUMBER_INVALID: Assert.Equal("iccid_is_only_number", result["errors"]["SimIccId"].First().ToString()); break;
                case ValidateAssertType.MSISDN_REQUIRED: Assert.Equal("msisdn_required", result["errors"]["SimMsisdn"].First().ToString()); break;
                case ValidateAssertType.MSISDN_INVALID: Assert.Equal("msisdn_invalid_length", result["errors"]["SimMsisdn"].First().ToString()); break;
                case ValidateAssertType.MSISDN_NUMBER_INVALID: Assert.Equal("msisdn_is_only_number", result["errors"]["SimMsisdn"].First().ToString()); break;
                case ValidateAssertType.IP_INVALID: Assert.Equal("ip_address_invalid", result["errors"]["DeviceIpAddress"].First().ToString()); break;
            }
        }
    }

    [Collection("Sequential")]
    public class SimDeviceAuthenticationControllerTests : WebApplicationBase
    {
        private readonly AuthControllerTestRepository _repository;

        private readonly AuthHttpClientWrapper _client;

        public SimDeviceAuthenticationControllerTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
            _repository = new AuthControllerTestRepository(MainDbContext, RadiusDbContext);
            _client = new AuthHttpClientWrapper(Client);
        }

        class TestDataClass : IEnumerable<object[]>
        {
            readonly List<object[]> _testData = new List<object[]>();

            public TestDataClass()
            {
                _testData.Add(new object[] { new Case131(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case132(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case133(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case15(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000",DeviceIpAddress = "0011122"}, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case16(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case17(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case181(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case182(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case183(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case191(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case192(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case193(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case201(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case202(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case203(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case211(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case212(new SimDeviceAuthenticationRequest() { DeviceImei = "352555093320000", SimIccId = "8981100005819480000", SimImsi = "440103213100000", SimMsisdn = "02017911000", DeviceIpAddress = "0011122" }, HttpStatusCode.OK) });
            }

            public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory(DisplayName = "DBアクセス付きテスト")]
        [ClassData(typeof(TestDataClass))]
        public void TestWitDb(TestCase testCase)
        {
            TestOutputHelper.WriteLine($"{testCase}");
            testCase.Test(TestOutputHelper, _repository, _client);
        }

        public abstract class TestCase
        {
            private readonly SimDeviceAuthenticationRequest _simDeviceAuthenticationRequest;

            // setterのため
            [SuppressMessage("ReSharper", "NotAccessedField.Local")]
            private HttpStatusCode _httpStatusCode;

            // setterのため
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            protected ITestOutputHelper _testOutputHelper;

            protected TestCase(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode)
            {
                _simDeviceAuthenticationRequest = simDeviceAuthenticationRequest;
                _httpStatusCode = httpStatusCode;
            }

            public virtual void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                authControllerTestRepository.SetUpInsertBaseData();
            }

            public abstract void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage);

            public void Test(ITestOutputHelper testOutputHelper, AuthControllerTestRepository authControllerTestRepository,
                AuthHttpClientWrapper httpClientWrapper)
            {
                _testOutputHelper = testOutputHelper;
                Test(authControllerTestRepository, httpClientWrapper);
            }

            public void Test(AuthControllerTestRepository authControllerTestRepository, AuthHttpClientWrapper httpClientWrapper)
            {
                SetUp(authControllerTestRepository);
                var response = httpClientWrapper.PostSimDeviceAuthentication(_simDeviceAuthenticationRequest);
                AssertThat(authControllerTestRepository, response);
            }
            protected void AssertCase13_18_19(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Null(result["errorCode"]);
                Assert.Null(result["errorMessage"]);
                Assert.NotNull(result["authId"]);
                Assert.NotNull(result["assignDeviceIpAddress"]);
                var testSimDeviceAuthenticationStateDone = authControllerTestRepository.GetSimDeviceAuthenticationStateDone(Guid.Parse(result["authId"].ToString()));
                Assert.NotNull(testSimDeviceAuthenticationStateDone);
                Assert.NotNull(testSimDeviceAuthenticationStateDone.SimDevice);
                var testRadreply = authControllerTestRepository.GetRadreply(testSimDeviceAuthenticationStateDone.SimDevice.Sim.UserName);
                Assert.Equal(testSimDeviceAuthenticationStateDone.SimDevice.Nw2IpAddressPool, testRadreply.Value);
                var list = result["canLogonUsers"].ToImmutableList();
                Assert.Empty(list);
            }

            protected void AssertCase15_16_17(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("1001", result["errorCode"]);
                Assert.Equal("Not Match SimDevice Info", result["errorMessage"]);
            }
        }

        private abstract class Case13 : TestCase
        {
            protected Case13(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            protected void AssertCase13(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                AssertCase13_18_19(authControllerTestRepository, acualRessult);
            }
        }

        private class Case131 : Case13
        {
            public Case131(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("13_1 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase131();
                _testOutputHelper.WriteLine("13_1 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("13_1 AssertThat");
                AssertCase13(authControllerTestRepository, httpResponseMessage);
            }
        }

        private class Case132 : Case13
        {
            public Case132(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("13_2 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase132();
                _testOutputHelper.WriteLine("13_2 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("13_2 AssertThat");
                AssertCase13(authControllerTestRepository, httpResponseMessage);
            }
        }

        private class Case133 : Case13
        {
            public Case133(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("13_3 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase133();
                _testOutputHelper.WriteLine("13_3 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("13_3 AssertThat");
                AssertCase13(authControllerTestRepository, httpResponseMessage);
            }
        }

        private class Case15 : TestCase
        {
            public Case15(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("15 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase15();
                _testOutputHelper.WriteLine("15 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("15 AssertThat");
               AssertCase15_16_17(authControllerTestRepository,acualRessult);
            }
        }

        private class Case16 : TestCase
        {
            public Case16(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("16 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase16();
                _testOutputHelper.WriteLine("16 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("16 AssertThat");
                AssertCase15_16_17(authControllerTestRepository, acualRessult);
            }
        }

        private class Case17 : TestCase
        {
            public Case17(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("17 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase17();
                _testOutputHelper.WriteLine("17 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("17 AssertThat");
                AssertCase15_16_17(authControllerTestRepository, acualRessult);
            }
        }
        private abstract class Case18 : TestCase
        {
            protected Case18(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public void AssertCase18(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
               AssertCase13_18_19(authControllerTestRepository,acualRessult);
            }
        }

        private class Case181 : Case18
        {
            public Case181(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("18_1 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase181();
                _testOutputHelper.WriteLine("18_1 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("18_1 AssertThat");
                AssertCase18(authControllerTestRepository, httpResponseMessage);
            }
        }
        private class Case182 : Case18
        {
            public Case182(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("18_2 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase182();
                _testOutputHelper.WriteLine("18_2 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("18_2 AssertThat");
                AssertCase18(authControllerTestRepository, httpResponseMessage);
            }
        }
        private class Case183 : Case18
        {
            public Case183(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("18_3 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase183();
                _testOutputHelper.WriteLine("18_3 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("18_3 AssertThat");
                AssertCase18(authControllerTestRepository, httpResponseMessage);
            }
        }

        private abstract class Case19 : TestCase
        {
            protected Case19(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public void AssertCase19(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                AssertCase13_18_19(authControllerTestRepository, acualRessult);
            }
        }

        private class Case191 : Case19
        {
            public Case191(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("19_1 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase191();
                _testOutputHelper.WriteLine("19_1 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("19_1 AssertThat");
                AssertCase19(authControllerTestRepository, httpResponseMessage);
            }
        }
        private class Case192 : Case19
        {
            public Case192(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("19_2 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase192();
                _testOutputHelper.WriteLine("19_2 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("19_2 AssertThat");
                AssertCase19(authControllerTestRepository, httpResponseMessage);
            }
        }
        private class Case193 : Case19
        {
            public Case193(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("19_3 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase193();
                _testOutputHelper.WriteLine("19_3 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("19_3 AssertThat");
                AssertCase19(authControllerTestRepository, httpResponseMessage);
            }
        }

        private abstract class Case20 : TestCase
        {
            protected Case20(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public void AssertCase20(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Null(result["errorCode"]);
                Assert.Null(result["errorMessage"]);
                Assert.NotNull(result["authId"]);
                Assert.NotNull(result["assignDeviceIpAddress"]);
                Assert.NotNull(result["simDeviceConfigureDictionary"]);
                var simDeviceAuthenticationStateDone = authControllerTestRepository.GetSimDeviceAuthenticationStateDone(Guid.Parse(result["authId"].ToString()));
                Assert.NotNull(simDeviceAuthenticationStateDone);
                Assert.NotNull(simDeviceAuthenticationStateDone.SimDevice);
                var list = result["canLogonUsers"].ToImmutableList();
                Assert.Empty(list);
                var testRadreply = authControllerTestRepository.GetRadreply(simDeviceAuthenticationStateDone.SimDevice.Sim.UserName);
                Assert.Equal(simDeviceAuthenticationStateDone.SimDevice.Nw2IpAddressPool, testRadreply.Value);
            }
        }

        private class Case201 : Case20
        {
            public Case201(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("20_1 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase201();
                _testOutputHelper.WriteLine("20_1 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("20_1 AssertThat");
                AssertCase20(authControllerTestRepository, httpResponseMessage);
            }
        }
        private class Case202 : Case20
        {
            public Case202(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("20_2 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase202();
                _testOutputHelper.WriteLine("20_2 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("20_2 AssertThat");
                AssertCase20(authControllerTestRepository, httpResponseMessage);
            }
        }
        private class Case203 : Case20
        {
            public Case203(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("20_3 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase193();
                _testOutputHelper.WriteLine("20_3 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("20_3 AssertThat");
                AssertCase20(authControllerTestRepository, httpResponseMessage);
            }
        }

        private abstract class Case21 : TestCase
        {
            protected Case21(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            public void AssertCase21(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Null(result["errorCode"]);
                Assert.Null(result["errorMessage"]);
                Assert.NotNull(result["authId"]);
                Assert.NotNull(result["assignDeviceIpAddress"]);
                Assert.NotNull(result["simDeviceConfigureDictionary"]);
                var simDeviceAuthenticationStateDone = authControllerTestRepository.GetSimDeviceAuthenticationStateDone(Guid.Parse(result["authId"].ToString()));
                Assert.NotNull(simDeviceAuthenticationStateDone);
                Assert.NotNull(simDeviceAuthenticationStateDone.SimDevice);
                var list = result["canLogonUsers"].ToImmutableList();
                Assert.NotEmpty(list);
                var testRadreply = authControllerTestRepository.GetRadreply(simDeviceAuthenticationStateDone.SimDevice.Sim.UserName);
                Assert.Equal(simDeviceAuthenticationStateDone.SimDevice.Nw2IpAddressPool, testRadreply.Value);
            }
        }

        private class Case211 : Case21
        {
            public Case211(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("21_1 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase211();
                _testOutputHelper.WriteLine("21_1 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("21_1 AssertThat");
                AssertCase21(authControllerTestRepository, httpResponseMessage);
            }
        }
        private class Case212 : Case21
        {
            public Case212(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }
            public override void SetUp(AuthControllerTestRepository authControllerTestRepository)
            {
                _testOutputHelper.WriteLine("21_2 Setup 1");
                base.SetUp(authControllerTestRepository);
                authControllerTestRepository.SetUpInsertDataForCase212();
                _testOutputHelper.WriteLine("21_2 Setup 2");
            }
            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage)
            {
                _testOutputHelper.WriteLine("21_2 AssertThat");
                AssertCase21(authControllerTestRepository, httpResponseMessage);
            }
        }

    }
}
