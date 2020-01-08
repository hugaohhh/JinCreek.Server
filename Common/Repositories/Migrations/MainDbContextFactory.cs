using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.IO;

namespace JinCreek.Server.Common.Repositories.Migrations
{
    class MainDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
    {
        public MainDbContext CreateDbContext(string[] args)
        {
            var config = GetConfiguration();

            var optionsBuilder = new DbContextOptionsBuilder<MainDbContext>();
            //            optionsBuilder.UseMySql(config.GetConnectionString("MainDbConnection"));
            optionsBuilder.UseMySql(
                config.GetConnectionString("MainDbConnection"),
                builder => builder.ServerVersion(
                    new Version(10, 4, 11),
                    ServerType.MariaDb
                    ));

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
