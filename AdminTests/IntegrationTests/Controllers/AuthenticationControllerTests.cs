using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace AdminTests.IntegrationTests.Controllers
{
    public class AuthenticationControllerTests : IClassFixture<CustomWebApplicationFactory<Admin.Startup>>
    {
        private readonly CustomWebApplicationFactory<Admin.Startup> _factory;
        private const string RegisterUrl = "/api/authentication/register";
        private const string LoginUrl = "/api/authentication/login";
        private const string RefreshUrl = "/api/authentication/refresh";

        public AuthenticationControllerTests(CustomWebApplicationFactory<Admin.Startup> factory)
        {
            _factory = factory;

            // Arrange
            HttpClient client = _factory.CreateClient();
            Utils.RegisterUser(client, "t-suzuki@indigo.co.jp", "9Q'vl!");
        }

        [Fact]
        public async void TestStatusCode()
        {
            HttpClient client = _factory.CreateClient();

            {
                // GET, DELETE, POST, PUT, PATCH
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.GetAsync(LoginUrl)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.DeleteAsync(LoginUrl)).StatusCode);
                Assert.NotEqual(HttpStatusCode.MethodNotAllowed, (await client.PostAsync(LoginUrl, null)).StatusCode); // POST: see below
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PutAsync(LoginUrl, null)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PatchAsync(LoginUrl, null)).StatusCode);

                // POSTs
                Assert.Equal(HttpStatusCode.OK, (await client.PostAsync(LoginUrl, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LoginUrl, CreateHttpContent(new { username = "aaaaaaaaaaaaaaaaaaaaa", password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LoginUrl, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp", password = "aaaaaa" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LoginUrl, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LoginUrl, CreateHttpContent(new { password = "9Q'vl!" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(LoginUrl, CreateHttpContent(null))).StatusCode);
            }

            {
                HttpResponseMessage response = await client.PostAsync(LoginUrl, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }));
                string refreshToken = (string)JObject.Parse(await response.Content.ReadAsStringAsync())["refreshToken"];

                // GET, DELETE, POST, PUT, PATCH
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.GetAsync(RefreshUrl)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.DeleteAsync(RefreshUrl)).StatusCode);
                Assert.NotEqual(HttpStatusCode.MethodNotAllowed, (await client.PostAsync(RefreshUrl, null)).StatusCode); // POST: see below
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PutAsync(RefreshUrl, null)).StatusCode);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PatchAsync(RefreshUrl, null)).StatusCode);

                // POST
                Assert.Equal(HttpStatusCode.OK, (await client.PostAsync(RefreshUrl, CreateHttpContent(new { refreshToken }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(RefreshUrl, CreateHttpContent(new { refreshToken = "aaaa" }))).StatusCode);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(RefreshUrl, CreateHttpContent(null))).StatusCode);
            }
        }

        [Fact]
        public async void TestContent()
        {
            HttpClient client = _factory.CreateClient();

            string refreshToken;
            {
                var json = JObject.Parse(await (await client.PostAsync(LoginUrl, CreateHttpContent(new { username = "t-suzuki@indigo.co.jp", password = "9Q'vl!" }))).Content.ReadAsStringAsync());
                Assert.NotNull(json["accessToken"]);
                Assert.NotNull(json["refreshToken"]);
                refreshToken = (string)json["refreshToken"];
            }

            {
                var json = JObject.Parse(await (await client.PostAsync(RefreshUrl, CreateHttpContent(new { refreshToken }))).Content.ReadAsStringAsync());
                Assert.NotNull(json["accessToken"]);
            }
        }

        private static HttpContent CreateHttpContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }
    }
}
