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
    public class MultiFactorAuthenticationControllerTests : WebApplicationBase
    {
        private readonly AuthControllerTestRepository _repository;
        private readonly MultiFactorAuthenticationControllerTestSetupRepository _setupRepository;

        private readonly AuthHttpClientWrapper _client;
        public MultiFactorAuthenticationControllerTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
            _setupRepository = new MultiFactorAuthenticationControllerTestSetupRepository(MainDbContext, RadiusDbContext);
            _repository = new AuthControllerTestRepository(MainDbContext, RadiusDbContext);
            _client = new AuthHttpClientWrapper(Client);
        }
        class TestDataClass : IEnumerable<object[]>
        {
            List<object[]> _testData = new List<object[]>();

            public TestDataClass()
            {
                _testData.Add(new object[] { new Case01(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776a"), Account = "" }, HttpStatusCode.BadRequest) });
                _testData.Add(new object[] { new Case02(new MultiFactorAuthenticationRequest { Account = "AccountUser1" }, HttpStatusCode.BadRequest) });
                //Case03はGuidオブジェクトが作成できないので別途記載

                // #14,#15はリクエストからのDeviceIpAddrの廃止により削除
                //_testData.Add(new object[] { new Case14(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                //_testData.Add(new object[] { new Case15(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.BadRequest) });

                _testData.Add(new object[] { new Case16(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case17(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case18(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case19(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case20(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case04(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case05(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "abcd" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case06(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776a"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case07(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case08(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
                _testData.Add(new object[] { new Case09(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case10(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case11(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.OK) });
                _testData.Add(new object[] { new Case12(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.OK) });
                //_testData.Add(new object[] { new Case13(new MultiFactorAuthenticationRequest { AuthId = Guid.Parse("0e4e88ae-c880-11e2-8598-5855cafa776b"), Account = "AccountUser1" }, HttpStatusCode.Unauthorized) });
            }

            public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void TestCase03()
        {
            _setupRepository.SetUpInsertDataForCase03();

            string request = "{accountName:'aaa', authId:'aaaabbbbbbbbcccccccc'}";
            var acualRessult = _client.PostMultiFactorAuthenticationCase03(request);
            MultiFatorUtils.Assert01_02_03_15(_repository, acualRessult);
            var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);

            Assert.StartsWith("Error converting value \"aaaabbbbbbbbcccccccc\" to type 'System.Nullable`1[System.Guid]'", result["errors"]["authId"].First().ToString());
        }

        [Theory(DisplayName = "多要素認証APIテスト")]
        [ClassData(typeof(TestDataClass))]
        public void MultiFactorValidation(MultiFactorTestCase multiFactorTestCase)
        {
            TestOutputHelper.WriteLine($"{multiFactorTestCase}");
            multiFactorTestCase.Test(TestOutputHelper, _repository, _setupRepository, _client);
        }

        public abstract class MultiFactorTestCase
        {
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            protected MultiFactorAuthenticationRequest _multiFactorAuthenticationRequest;

            // setterのため
            [SuppressMessage("ReSharper", "NotAccessedField.Local")]
            private HttpStatusCode _httpStatusCode;

            // setterのため
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            protected ITestOutputHelper _testOutputHelper;

            protected MultiFactorTestCase(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode)
            {
                _multiFactorAuthenticationRequest = multiFactorAuthenticationRequest;
                _httpStatusCode = httpStatusCode;
            }

            public virtual void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                setupRepository.SetUpInsertBaseDataForMainDb();
                setupRepository.SetUpInsertBaseDataForRadiusDb();
            }

            public abstract void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage httpResponseMessage);

            public void Test(ITestOutputHelper testOutputHelper, AuthControllerTestRepository authControllerTestRepository,
                MultiFactorAuthenticationControllerTestSetupRepository setupRepository, AuthHttpClientWrapper httpClientWrapper)
            {
                _testOutputHelper = testOutputHelper;
                Test(authControllerTestRepository, setupRepository, httpClientWrapper);
            }

            public void Test(AuthControllerTestRepository authControllerTestRepository, MultiFactorAuthenticationControllerTestSetupRepository setupRepository, AuthHttpClientWrapper httpClientWrapper)
            {
                SetUp(setupRepository);
                var response = httpClientWrapper.PostMultiFactorAuthentication(_multiFactorAuthenticationRequest);
                AssertThat(authControllerTestRepository, response);
            }
        }

        private static class MultiFatorUtils
        {
            public static void Assert04_05_07_08_14_16_17_18_19_20(AuthControllerTestRepository authControllerTestRepository,
                HttpResponseMessage acutalRessult, MultiFactorAuthenticationRequest request)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, acutalRessult.StatusCode);
                var result = JObject.Parse(acutalRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("1002", result["ErrorCode"]);
                Assert.Equal("Not Match MultiFactor Info", result["ErrorMessage"]);
                var multiFactorAuthenticationStateDones =
                    authControllerTestRepository.GetMultiFatorAuthenticationDone();
                Assert.Empty(multiFactorAuthenticationStateDones);
                var multiFactorAuthenticationLogSuccesses =
                    authControllerTestRepository.GetMultiFatorAuthenticationLogSuccess();
                Assert.Empty(multiFactorAuthenticationLogSuccesses);
                var simDeviceAuthenticationStateDone =
                    authControllerTestRepository.GetSimAndDeviceAuthenticated(request.AuthId);

                var multiFactorAuthenticationFailureLogs = authControllerTestRepository.GetMultiFactorAuthenticationFailureLogBySimAndDeviceId(
                    simDeviceAuthenticationStateDone.SimAndDevice.Id);
                Assert.NotEmpty(multiFactorAuthenticationFailureLogs);
                var radreplys =
                    authControllerTestRepository.GetRadreplys(simDeviceAuthenticationStateDone.SimAndDevice.Sim.UserName + "@" + simDeviceAuthenticationStateDone.SimAndDevice.Sim.SimGroup.UserNameSuffix);
                Assert.Single(radreplys);
                Assert.Equal(simDeviceAuthenticationStateDone.SimAndDevice.IsolatedNw2Ip, radreplys.First().Value);
            }

            public static void Assert09_10_11(AuthControllerTestRepository authControllerTestRepository,
                HttpResponseMessage acualRessult, MultiFactorAuthenticationRequest request)
            {
                Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Null(result["ErrorCode"]);
                Assert.Null(result["ErrorMessage"]);
                var isAuthenticateWhenScreenLock = result["IsAuthenticateWhenScreenLock"].ToObject<bool>();
                Assert.True(isAuthenticateWhenScreenLock);
                Assert.NotNull(result["AssignDeviceIpAddress"]);
                Assert.NotNull(result["AuthenticationDuration"]);
                var factorCombination =
                    authControllerTestRepository.GetFactorCombination(request.Account, request.AuthId);
                Assert.NotNull(factorCombination);
                Assert.NotNull(factorCombination.MultiFactorAuthenticated);

                var multiFactorAuthenticationSuccessLogs = authControllerTestRepository.GetMultiFactorAuthenticationSuccessLogByMultiFactorId(factorCombination.Id);
                var multiFactorAuthenticationFailureLogs = authControllerTestRepository.GetMultiFactorAuthenticationFailureLogBySimAndDeviceId(factorCombination.SimAndDevice.Id);
                Assert.NotEmpty(multiFactorAuthenticationSuccessLogs);
                Assert.Empty(multiFactorAuthenticationFailureLogs);
                var testRadreply = authControllerTestRepository.GetRadreply(factorCombination.SimAndDevice.Sim.UserName + "@" + factorCombination.SimAndDevice.Sim.SimGroup.UserNameSuffix);
                Assert.Equal(factorCombination.ClosedNwIp, testRadreply.Value);
            }

            public static void Assert01_02_03_15(AuthControllerTestRepository authControllerTestRepository,
                HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.BadRequest, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.NotNull(result["traceId"]);
                Assert.Equal("One or more validation errors occurred.", result["title"]);
                Assert.Equal("400", result["status"]);
                var multiFactorAuthenticationLogFails =
                    authControllerTestRepository.GetMultiFactorAuthenticationLogFail();
                Assert.Empty(multiFactorAuthenticationLogFails);
                var multiFactorAuthenticationLogSuccesses =
                    authControllerTestRepository.GetMultiFatorAuthenticationLogSuccess();
                Assert.Empty(multiFactorAuthenticationLogSuccesses);
                var multiFactorAuthenticationStateDones =
                    authControllerTestRepository.GetMultiFatorAuthenticationDone();
                Assert.Empty(multiFactorAuthenticationStateDones);

                var radreplys = authControllerTestRepository.GetRadreplys("user1@jincreek2");
                Assert.Single(radreplys);
                Assert.Equal("Nw2Address", radreplys.First().Value);
            }

            public static void Assert06(AuthControllerTestRepository authControllerTestRepository,
                HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("1003", result["ErrorCode"]);
                Assert.Equal("Not Match AuthId Info", result["ErrorMessage"]);
                var multiFactorAuthenticationStateDones =
                    authControllerTestRepository.GetMultiFatorAuthenticationDone();
                Assert.Empty(multiFactorAuthenticationStateDones);
                var multiFactorAuthenticationLogSuccesses =
                    authControllerTestRepository.GetMultiFatorAuthenticationLogSuccess();
                Assert.Empty(multiFactorAuthenticationLogSuccesses);
                var multiFactorAuthenticationLogFails =
                    authControllerTestRepository.GetMultiFactorAuthenticationLogFail();
                Assert.Empty(multiFactorAuthenticationLogFails);
            }

            public static void Assert13(AuthControllerTestRepository authControllerTestRepository,
                HttpResponseMessage acualRessult)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("1003", result["ErrorCode"]);
                Assert.Equal("Not Match AuthId Info", result["ErrorMessage"]);

                var multiFactorAuthenticationStateDones =
                    authControllerTestRepository.GetMultiFatorAuthenticationDone();
                // SetUpで投入したレコードはあるのでそれを確認
                Assert.Equal(2, multiFactorAuthenticationStateDones.Count);
                //Assert.Empty(multiFactorAuthenticationStateDones);
                var multiFactorAuthenticationLogSuccesses =
                    authControllerTestRepository.GetMultiFatorAuthenticationLogSuccess();
                Assert.Empty(multiFactorAuthenticationLogSuccesses);
                var multiFactorAuthenticationLogFails =
                    authControllerTestRepository.GetMultiFactorAuthenticationLogFail();
                Assert.Empty(multiFactorAuthenticationLogFails);
            }
        }

        private abstract class MultiFatorValidateTestCase : MultiFactorTestCase
        {
            protected MultiFatorValidateTestCase(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case01 : MultiFatorValidateTestCase
        {
            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("01 Setup");
                setupRepository.SetUpInsertDataForCase01();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("01 AssertThat");
                MultiFatorUtils.Assert01_02_03_15(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("account_required", result["errors"]["Account"].First().ToString());
            }

            public Case01(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case02 : MultiFatorValidateTestCase
        {
            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("02 Setup");
                setupRepository.SetUpInsertDataForCase02();
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("02 AssertThat");
                MultiFatorUtils.Assert01_02_03_15(authControllerTestRepository, acualRessult);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Equal("authId_required", result["errors"]["AuthId"].First().ToString());
            }

            public Case02(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

        //private class Case14 : MultiFactorTestCase
        //{
        //    public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("14 Setup 1");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase14();
        //        _testOutputHelper.WriteLine("14 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("14 AssertThat");
        //        MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
        //    }

        //    public Case14(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        //private class Case15 : MultiFatorValidateTestCase
        //{
        //    public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
        //    {
        //        _testOutputHelper.WriteLine("15 Setup");
        //        base.SetUp(setupRepository);
        //        setupRepository.SetUpInsertDataForCase15();
        //        _testOutputHelper.WriteLine("15 Setup 2");
        //    }

        //    public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
        //    {
        //        _testOutputHelper.WriteLine("15 AssertThat");
        //        MultiFatorUtils.Assert01_02_03_15(authControllerTestRepository, acualRessult);
        //        var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
        //        Assert.Equal("device_ip_address_invalid", result["errors"]["DeviceIpAddress"].First().ToString());
        //    }

        //    public Case15(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
        //    {
        //    }
        //}

        private class Case16 : MultiFactorTestCase
        {
            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("16 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase16();
                _testOutputHelper.WriteLine("16 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("16 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }

            public Case16(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case17 : MultiFactorTestCase
        {
            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("17 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase17();
                _testOutputHelper.WriteLine("17 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("17 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }

            public Case17(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case18 : MultiFactorTestCase
        {
            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("18 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase18();
                _testOutputHelper.WriteLine("18 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("16 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }

            public Case18(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case19 : MultiFactorTestCase
        {
            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("19 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase19();
                _testOutputHelper.WriteLine("19 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("19 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }

            public Case19(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case20 : MultiFactorTestCase
        {
            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("20 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase20();
                _testOutputHelper.WriteLine("20 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("20 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }

            public Case20(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

        private class Case04 : MultiFactorTestCase
        {
            public Case04(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("04 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase04();
                _testOutputHelper.WriteLine("04 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("04 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }
        }
        private class Case05 : MultiFactorTestCase
        {
            public Case05(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("05 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase05();
                _testOutputHelper.WriteLine("05 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("05 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }
        }

        private class Case06 : MultiFactorTestCase
        {
            public Case06(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("06 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase06();
                _testOutputHelper.WriteLine("06 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("06 AssertThat");
                MultiFatorUtils.Assert06(authControllerTestRepository, acualRessult);
            }
        }

        private class Case07 : MultiFactorTestCase
        {
            public Case07(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("07 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase07();
                _testOutputHelper.WriteLine("07 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("07 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }
        }

        private class Case08 : MultiFactorTestCase
        {
            public Case08(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("08 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase08();
                _testOutputHelper.WriteLine("08 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("08 AssertThat");
                MultiFatorUtils.Assert04_05_07_08_14_16_17_18_19_20(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }
        }

        private class Case09 : MultiFactorTestCase
        {
            public Case09(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("09 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase09();
                _testOutputHelper.WriteLine("09 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("09 AssertThat");
                MultiFatorUtils.Assert09_10_11(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }
        }

        private class Case10 : MultiFactorTestCase
        {
            public Case10(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("10 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase10();
                _testOutputHelper.WriteLine("10 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("10 AssertThat");
                MultiFatorUtils.Assert09_10_11(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }
        }
        private class Case11 : MultiFactorTestCase
        {
            public Case11(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("11 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase11();
                _testOutputHelper.WriteLine("11 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("11 AssertThat");
                MultiFatorUtils.Assert09_10_11(authControllerTestRepository, acualRessult, _multiFactorAuthenticationRequest);
            }
        }

        private class Case12 : MultiFactorTestCase
        {
            public Case12(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }

            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("12 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase12();
                _testOutputHelper.WriteLine("12 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("12 AssertThat");
                Assert.Equal(HttpStatusCode.OK, acualRessult.StatusCode);
                var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);
                Assert.Null(result["ErrorCode"]);
                Assert.Null(result["ErrorMessage"]);
                var isAuthenticateWhenScreenLock = result["IsAuthenticateWhenScreenLock"].ToObject<bool>();
                Assert.False(isAuthenticateWhenScreenLock);
                Assert.NotNull(result["AssignDeviceIpAddress"]);
                Assert.NotNull(result["AuthenticationDuration"]);
                var factorCombination = authControllerTestRepository.GetFactorCombination(_multiFactorAuthenticationRequest.Account, _multiFactorAuthenticationRequest.AuthId);
                Assert.NotNull(factorCombination);
                Assert.NotNull(factorCombination.MultiFactorAuthenticated);

                var multiFactorAuthenticationSuccessLogs = authControllerTestRepository.GetMultiFactorAuthenticationSuccessLogByMultiFactorId(factorCombination.Id);
                var multiFactorAuthenticationFailureLogs = authControllerTestRepository.GetMultiFactorAuthenticationFailureLogBySimAndDeviceId(factorCombination.SimAndDevice.Id);
                Assert.NotEmpty(multiFactorAuthenticationSuccessLogs);
                Assert.Empty(multiFactorAuthenticationFailureLogs);
                var testRadreply = authControllerTestRepository.GetRadreply(factorCombination.SimAndDevice.Sim.UserName + "@" + factorCombination.SimAndDevice.Sim.SimGroup.UserNameSuffix);
                Assert.Equal(factorCombination.ClosedNwIp, testRadreply.Value);
            }
        }

        private class Case13 : MultiFactorTestCase
        {
            public override void SetUp(MultiFactorAuthenticationControllerTestSetupRepository setupRepository)
            {
                _testOutputHelper.WriteLine("13 Setup 1");
                base.SetUp(setupRepository);
                setupRepository.SetUpInsertDataForCase13();
                _testOutputHelper.WriteLine("13 Setup 2");
            }

            public override void AssertThat(AuthControllerTestRepository authControllerTestRepository, HttpResponseMessage acualRessult)
            {
                _testOutputHelper.WriteLine("13 AssertThat");
                MultiFatorUtils.Assert13(authControllerTestRepository, acualRessult);
            }

            public Case13(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest, HttpStatusCode httpStatusCode) : base(multiFactorAuthenticationRequest, httpStatusCode)
            {
            }
        }

    }
}
