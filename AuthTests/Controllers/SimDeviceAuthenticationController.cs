using JinCreek.Server.Interfaces;
using System.Collections;
using System.Collections.Generic;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.AuthTests.Controllers
{
    public class Sample1
    {
        [Fact]
        public void Case1()
        {
            Assert.Equal("1", "1");
        }
    }

    class TestDataClass : IEnumerable<object[]>
    {
        List<object[]> _testData = new List<object[]>();

        public TestDataClass()
        {
            //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "1" }, 200, new ValidationProblemDetails(){Status = 200} });
            //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "1", SimIccId = "2" }, 200, new ErrorResponse(){ErrorCode = "1111", ErrorMessage = "aaaa"} });
            //_testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "3" }, 400, new SimDeviceAuthenticationResponse(){AuthId = "hoge"} });
            _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "1" }, 200});
            _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "1", SimIccId = "2" }, 200});
            _testData.Add(new object[] { new SimDeviceAuthenticationRequest() { DeviceImei = "", SimIccId = "3" }, 400});
        }

        public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Sample2
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly AuthenticationRepository _repository;

        public Sample2(ITestOutputHelper testOutputHelper, AuthenticationRepository authenticationRepository)
        {
            _testOutputHelper = testOutputHelper;
            _repository = authenticationRepository;
        }

        [Theory(DisplayName = "サンプル")]
        [ClassData(typeof(TestDataClass))]
        public void Case10(SimDeviceAuthenticationRequest request, int statusCode)
        {
            _testOutputHelper.WriteLine(message: $">{request.DeviceImei}, {request.SimIccId}");
            _testOutputHelper.WriteLine(message: $">{statusCode}");
            //_testOutputHelper.WriteLine(message: $">{response}");
            Assert.Equal("1", "1");
        }
    }
}
