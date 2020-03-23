using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace JinCreek.Server.AdminTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration.
                services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MainDbContext>)));

                // Add ApplicationDbContext using an in-memory database for testing.
                services.AddDbContext<MainDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.EnableSensitiveDataLogging();
                });

                // Build the service provider.
                var db = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<MainDbContext>();

                // Ensure the database is created.
                db.Database.EnsureCreated();
            });
        }
    }
}
