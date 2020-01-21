
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using JinCreek.Server.Auth;
using JinCreek.Server.Interfaces;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.AuthTests.Controllers
{
    [Collection("Sequential")]
    public class MultiFactorAuthenticationControllerTests : WebApplicationBase
    {
        protected MultiFactorAuthenticationControllerTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
        }
        class TestDataClass : IEnumerable<object[]>
        {
            List<object[]> _testData = new List<object[]>();

            public TestDataClass()
            {
                _testData.Add(new object[] { new MultiFactorAuthenticationRequest { Account = "AccountUser1"}, HttpStatusCode.BadRequest, SimDeviceAuthenticationControllerValidateTests.ValidateAssertType.IMEI_REQUIRED });
            }

            public IEnumerator<object[]> GetEnumerator() => _testData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        [Theory(DisplayName = "バリデーションテスト")]
        [ClassData(typeof(TestDataClass))]
        public void Validation(MultiFactorAuthenticationRequest request, HttpStatusCode statusCode, SimDeviceAuthenticationControllerValidateTests.ValidateAssertType type)
        {
            var acualRessult = PostMultiFactorAuthentication(request);

            Assert.Equal(statusCode, acualRessult.StatusCode);
            var result = JObject.Parse(acualRessult.Content.ReadAsStringAsync().Result);

            Assert.NotNull(result["traceId"]);
            Assert.Equal("One or more validation errors occurred.", result["title"]);
            Assert.Equal("400", result["status"]);
        }
    }
}
