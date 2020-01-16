using Microsoft.AspNetCore.Hosting;

namespace JinCreek.Server.AuthTests
{
    public class WebApplicationFactory<TStartup> : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                //// Remove the app's ApplicationDbContext registration.
                //services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MainDbContext>)));

                //// Add ApplicationDbContext using an in-memory database for testing.
                //services.AddDbContext<MainDbContext>(options =>
                //{
                //    options.UseInMemoryDatabase("InMemoryDbForTesting");
                //});

                // Build the service provider.
                //var db = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<MainDbContext>();

                // Ensure the database is created.
                //db.Database.EnsureCreated();
                //db.Database.Migrate();
            });
        }
    }
}
