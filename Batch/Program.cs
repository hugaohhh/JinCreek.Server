using ConsoleAppFramework;
using JinCreek.Server.Common.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Threading.Tasks;


namespace JinCreek.Server.Batch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .Build();

            await Host.CreateDefaultBuilder()
                    .ConfigureLogging(logging =>
                    {
                        //logging.ReplaceToSimpleConsole();
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Trace);
                        logging.AddNLog(config);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.Configure<IConfiguration>(hostContext.Configuration);

                        services.AddDbContext<MainDbContext>(options =>
                        {
                            options.UseMySql(hostContext.Configuration.GetConnectionString("MainDbConnection"));
                        });
                        services.AddDbContext<RadiusDbContext>(options =>
                        {
                            options.UseMySql(hostContext.Configuration.GetConnectionString("RadiusDbConnection"));
                        });

                        services.AddTransient<UserRepository>();
                        services.AddTransient<AuthenticationRepository>();
                        services.AddTransient<RadiusRepository>();
                    })
                .RunConsoleAppFrameworkAsync(args);
        }
    }
}
