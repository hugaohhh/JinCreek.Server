using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdminTests.IntegrationTests
{
    class Utils
    {
        private const string RegisterUrl = "/api/authentication/register";
        private const string LoginUrl = "/api/authentication/login";

        public static void RegisterUser(HttpClient client, string username, string password)
        {
            var result = client.PostAsync(RegisterUrl, CreateHttpContent(new { username, password })).Result;
            if (!result.IsSuccessStatusCode)
            {
                throw new ArgumentException(result.Content.ReadAsStringAsync().Result);
            }
        }

        public static string GetAccessToken(HttpClient client, string username, string password)
        {
            HttpResponseMessage response = client.PostAsync(LoginUrl, CreateHttpContent(new { username, password })).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
            }
            return (string)JObject.Parse(response.Content.ReadAsStringAsync().Result)["accessToken"];
        }

        public static HttpContent CreateHttpContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }
    }
}
