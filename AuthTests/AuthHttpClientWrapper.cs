using JinCreek.Server.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Mime;
using System.Text;

namespace JinCreek.Server.AuthTests
{
    public class AuthHttpClientWrapper
    {
        private HttpClient _httpClient;

        public AuthHttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpResponseMessage PostSimDeviceAuthentication(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest)
        {
            var url = "api/sim-and-device/authentication";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(simDeviceAuthenticationRequest), Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            return Post(httpRequestMessage);
        }

        public HttpResponseMessage PostMultiFactorAuthentication(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest)
        {
            var url = "api/multi-factor/authentication";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(multiFactorAuthenticationRequest), Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            return Post(httpRequestMessage);
        }
        public HttpResponseMessage PostMultiFactorAuthenticationCase03(string multiFactorAuthenticationRequest)
        {
            var url = "api/multi-factor/authentication";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(multiFactorAuthenticationRequest, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            return Post(httpRequestMessage);
        }

        public HttpResponseMessage PostDeauthentication(DeauthenticationRequest deauthenticationRequest)
        {
            var url = "api/deauthentication";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(deauthenticationRequest), Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            return Post(httpRequestMessage);
        }

        //public HttpResponseMessage PostDeauthenticationCase03(string multiFactorAuthenticationRequest)
        //{
        //    var url = "api/deauthentication";
        //    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        //    {
        //        Content = new StringContent(multiFactorAuthenticationRequest, Encoding.UTF8, MediaTypeNames.Application.Json)
        //    };
        //    return Post(httpRequestMessage);
        //}

        private HttpResponseMessage Post(HttpRequestMessage httpRequestMessage)
        {
            return _httpClient.SendAsync(httpRequestMessage).Result;
        }
    }
}
