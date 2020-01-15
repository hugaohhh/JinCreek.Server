using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
            var result = client.PostAsync(RegisterUrl, CreateJsonContent(new { username, password })).Result;
            if (!result.IsSuccessStatusCode)
            {
                throw new ArgumentException(result.Content.ReadAsStringAsync().Result);
            }
        }

        public static string GetAccessToken(HttpClient client, string username, string password)
        {
            var response = client.PostAsync(LoginUrl, CreateJsonContent(new { username, password })).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException(response.Content.ReadAsStringAsync().Result);
            }
            return (string)JObject.Parse(response.Content.ReadAsStringAsync().Result)["accessToken"];
        }

        public static HttpContent CreateJsonContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }

        public static HttpResponseMessage Get(HttpClient client, string url, string bearer)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            return client.SendAsync(request).Result;
        }

        public static HttpResponseMessage Delete(HttpClient client, string url, string bearer)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            return client.SendAsync(request).Result;
        }

        public static HttpResponseMessage Post(HttpClient client, string url, HttpContent content, string bearer)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.Content = content;
            return client.SendAsync(request).Result;
        }

        public static HttpResponseMessage Put(HttpClient client, string url, HttpContent content, string bearer)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.Content = content;
            return client.SendAsync(request).Result;
        }
    }
}
