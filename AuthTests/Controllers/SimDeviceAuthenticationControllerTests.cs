using JinCreek.Server.Auth;
using JinCreek.Server.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
                _testData.Add(new object[] { new Case131(new SimDeviceAuthenticationRequest(){ DeviceImei = "352555093320000",SimIccId = "8981100005819480000",SimImsi = "440103213100000",SimMsisdn = "02017911000"}, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case132(new SimDeviceAuthenticationRequest(){ DeviceImei = "352555093320000",SimIccId = "8981100005819480000",SimImsi = "440103213100000",SimMsisdn = "02017911000"}, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case133(new SimDeviceAuthenticationRequest(){ DeviceImei = "352555093320000",SimIccId = "8981100005819480000",SimImsi = "440103213100000",SimMsisdn = "02017911000"}, HttpStatusCode.OK) });
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
        }


        private abstract class Case13 : TestCase
        {
            protected Case13(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest, HttpStatusCode httpStatusCode) : base(simDeviceAuthenticationRequest, httpStatusCode)
            {
            }

            protected void AssertCase13(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
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
                Assert.Equal(testSimDeviceAuthenticationStateDone.SimDevice.Nw2AddressPool, testRadreply.Value);
                var list = result["canLogonUsers"].ToImmutableList();
                Assert.Empty(list);
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
    }
}
