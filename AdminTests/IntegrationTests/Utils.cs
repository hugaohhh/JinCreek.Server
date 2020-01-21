using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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

        // see https://stackoverflow.com/questions/20621950/asp-net-identitys-default-password-hasher-how-does-it-work-and-is-it-secure
        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
    }
}
