using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace AdminTests.IntegrationTests.Controllers
{
    public class AuthenticationControllerTests : IClassFixture<WebApplicationFactory<Admin.Startup>>
    {
        private readonly WebApplicationFactory<Admin.Startup> _factory;
        private const string LOGIN_URL = "/api/authentication/login";
        private const string REFRESH_URL = "/api/authentication/refresh";

        public AuthenticationControllerTests(WebApplicationFactory<Admin.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async void TestStatusCode()
        {
            HttpClient client = _factory.CreateClient();

            {
                // GET, DELETE, POST, PUT, PATCH
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.GetAsync(LOGIN_URL)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.DeleteAsync(LOGIN_URL)).StatusCode);
                Assert.NotEqual(HttpStatusCode.MethodNotAllowed, (await client.PostAsync(LOGIN_URL, null)).StatusCode); // POST: see below
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PutAsync(LOGIN_URL, null)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PatchAsync(LOGIN_URL, null)).StatusCode);

                // POSTs
                Assert.Equal(HttpStatusCode.OK, (await client.PostAsync(LOGIN_URL, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LOGIN_URL, CreateHttpContent(new { username = "aaaaaaaaaaaaaaaaaaaaa", password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LOGIN_URL, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp", password = "aaaaaa" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LOGIN_URL, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LOGIN_URL, CreateHttpContent(new { password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LOGIN_URL, CreateHttpContent(null))).StatusCode);
            }

            {
                HttpResponseMessage response = await client.PostAsync(LOGIN_URL, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }));
                string refreshToken = (string)JObject.Parse(await response.Content.ReadAsStringAsync())["refreshToken"];

                // GET, DELETE, POST, PUT, PATCH
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.GetAsync(REFRESH_URL)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.DeleteAsync(REFRESH_URL)).StatusCode);
                Assert.NotEqual(HttpStatusCode.MethodNotAllowed, (await client.PostAsync(REFRESH_URL, null)).StatusCode); // POST: see below
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PutAsync(REFRESH_URL, null)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PatchAsync(REFRESH_URL, null)).StatusCode);

                // POST
                Assert.Equal(HttpStatusCode.OK, (await client.PostAsync(REFRESH_URL, CreateHttpContent(new { refreshToken }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(REFRESH_URL, CreateHttpContent(new { refreshToken = "aaaa" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(REFRESH_URL, CreateHttpContent(null))).StatusCode);
            }
        }

        [Fact]
        public async void TestContent()
        {
            HttpClient client = _factory.CreateClient();

            string refreshToken;
            {
                var json = JObject.Parse(await (await client.PostAsync(LOGIN_URL, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }))).Content.ReadAsStringAsync());
                Assert.NotNull(json["accessToken"]);
                Assert.NotNull(json["refreshToken"]);
                refreshToken = (string)json["refreshToken"];
            }

            {
                var json = JObject.Parse(await (await client.PostAsync(REFRESH_URL, CreateHttpContent(new { refreshToken }))).Content.ReadAsStringAsync());
                Assert.NotNull(json["accessToken"]);
            }
        }

        private static HttpContent CreateHttpContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }
    }
}
