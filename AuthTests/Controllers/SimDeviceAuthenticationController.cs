using JinCreek.Server.Auth;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.AuthTests.Controllers
{
    [Collection("Sequential")]
    public class ValidateTests : WebApplicationBase
    {
        public ValidateTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
        }

        class TestDataClass : IEnumerable<object[]>
        {
            List<object[]> _testData = new List<object[]>();

            public TestDataClass()
            {
                //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "1" }, 200, new ValidationProblemDetails(){Status = 200} });
                //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "1", SimIccId = "2" }, 200, new ErrorResponse(){ErrorCode = "1111", ErrorMessage = "aaaa"} });
                //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "3" }, 400, new SimDeviceAuthenticationResponse(){AuthId = "hoge"} });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "1" }, HttpStatusCode.BadRequest });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "1", SimIccId = "2" }, HttpStatusCode.BadRequest });
                _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "3" }, HttpStatusCode.BadRequest });
            }

            public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        [Theory(DisplayName = "バリデーションテスト")]
        [ClassData(typeof(TestDataClass))]
        public void Validation(SimDeviceAuthenticationRequest request, HttpStatusCode statusCode)
        {
            TestOutputHelper.WriteLine(message: $">{request.DeviceImei}, {request.SimIccId}");
            TestOutputHelper.WriteLine(message: $">{statusCode}");


            var acualRessult = PostSimDeviceAuthentication(request);

            Assert.Equal(statusCode, acualRessult.StatusCode);

            TestOutputHelper.WriteLine($"{acualRessult.StatusCode.ToString()}, {acualRessult.Content.ReadAsStringAsync().Result}, {acualRessult.Headers}");
        }
    }


    [Collection("Sequential")]
    public class DbTests : WebApplicationBase
    {
        public DbTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
        }

        [Fact(DisplayName = "DBテストCaseXX")]
        public void DbTestCaseXx()
        {
            var org = new Organization()
            {
                Name = "a",
                Address = "b",
                DelegatePhone = "0",
                AdminPhone = "c",
                AdminMail = "d",
                Url = "url",
                StartDay = DateTime.Now,
                EndDay = DateTime.Now,
                IsValid = true
            };
            UserRepository.Create(org);



            var request = new SimDeviceAuthenticationRequest()
            {
                DeviceImei = "1",
                SimIccId = "2",
                SimImsi = "3",
                SimMsisdn = "4"
            };

            var acualRessult = PostSimDeviceAuthentication(request);
            var acualOrganization = UserRepository.GetOrganization(org.Code);


            Assert.Equal(HttpStatusCode.Unauthorized, acualRessult.StatusCode);
            Assert.Equal(org.Code, acualOrganization.Code);
        }

        [Fact(DisplayName = "DBテストCaseYy")]
        public void DbTestCaseYy()
        {
            var org = new Organization()
            {
                Name = "a",
                Address = "b",
                DelegatePhone = "0",
                AdminPhone = "c",
                AdminMail = "d",
                Url = "url",
                StartDay = DateTime.Now,
                EndDay = DateTime.Now,
                IsValid = true
            };
            UserRepository.Create(org);



            var request = new SimDeviceAuthenticationRequest()
            {
                DeviceImei = "1",
                SimIccId = "2",
                SimImsi = "3",
                SimMsisdn = "4"
            };

            var acualRessult = PostSimDeviceAuthentication(request);
            var acualOrganization = UserRepository.GetOrganization(org.Code);


            Assert.Equal(HttpStatusCode.Unauthorized, acualRessult.StatusCode);
            Assert.Equal(org.Code, acualOrganization.Code);
        }
    }
}
