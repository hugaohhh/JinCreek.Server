using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Common.Repositories.Migrations
{
    class MainDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
    {
        public MainDbContext CreateDbContext(string[] args)
        {
            var config = GetConfiguration();

            var optionsBuilder = new DbContextOptionsBuilder<MainDbContext>();
            optionsBuilder.UseMySql(config.GetConnectionString("MainDbConnection"));

            return new MainDbContext(optionsBuilder.Options);
        }

        private IConfiguration GetConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Directory.GetCurrentDirectory());

            configBuilder.AddJsonFile(@"appsettings.json");

            return configBuilder.Build();
        }
    }
}
