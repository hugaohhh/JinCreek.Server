using JinCreek.Server.Common.Repositories;
using JinCreek.Server.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace JinCreek.Server.AuthTests
{
    public class WebApplicationBase : IClassFixture<WebApplicationFactory<Auth.Startup>>, IDisposable
    {
        protected readonly ITestOutputHelper TestOutputHelper;

        protected readonly HttpClient Client;

        protected readonly MainDbContext MainDbContext;
        protected readonly RadiusDbContext RadiusDbContext;

        protected readonly SimDeviceRepository SimDeviceRepository;
        protected readonly UserRepository UserRepository;
        protected readonly AuthenticationRepository AuthenticationRepository;

        protected WebApplicationBase(WebApplicationFactory<Auth.Startup> factory, ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            TestOutputHelper.WriteLine("Construct");

            Client = factory.CreateClient();
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            var scope = factory.Services.GetService<IServiceScopeFactory>().CreateScope();
            MainDbContext = scope.ServiceProvider.GetService<MainDbContext>();
            RadiusDbContext = scope.ServiceProvider.GetService<RadiusDbContext>();

            SimDeviceRepository = scope.ServiceProvider.GetService<SimDeviceRepository>();
            UserRepository = scope.ServiceProvider.GetService<UserRepository>();
            AuthenticationRepository = scope.ServiceProvider.GetService<AuthenticationRepository>();


            TestOutputHelper.WriteLine("Database Init");
            MainDbContext.Database.EnsureDeleted();
            MainDbContext.Database.EnsureCreated();
            MainDbContext.Database.Migrate();
        }


        public void Dispose()
        {
            TestOutputHelper.WriteLine("Dispose");
            MainDbContext.Database.EnsureDeleted();
        }


        protected HttpResponseMessage PostSimDeviceAuthentication(SimDeviceAuthenticationRequest simDeviceAuthenticationRequest)
        {
            var url = "api/sim_device/authentication";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(simDeviceAuthenticationRequest), Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            return Post(httpRequestMessage);
        }

        protected HttpResponseMessage PostMultiFactorAuthentication(MultiFactorAuthenticationRequest multiFactorAuthenticationRequest)
        {
            var url = "api/multi_factor/authentication";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(multiFactorAuthenticationRequest), Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            return Post(httpRequestMessage);
        }

        protected HttpResponseMessage PostDeauthentication(DeauthenticationRequest deauthenticationRequest)
        {
            var url = "api/deauthentication";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(deauthenticationRequest), Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            return Post(httpRequestMessage);
        }

        private HttpResponseMessage Post(HttpRequestMessage httpRequestMessage)
        {
            return Client.SendAsync(httpRequestMessage).Result;
        }
    }
}
