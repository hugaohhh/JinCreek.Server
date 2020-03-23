using JinCreek.Server.Common.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Xunit.Abstractions;

namespace JinCreek.Server.Batch
{
    public class BatchTestBase
    {
        public readonly ITestOutputHelper TestOutputHelper;

        protected IHostBuilder HostBuilder;

        protected readonly MainDbContext MainDbContext;
        protected readonly RadiusDbContext RadiusDbContext;

        protected readonly IConfigurationRoot Configuration;
        protected readonly UserRepository UserRepository;
        protected readonly AuthenticationRepository AuthenticationRepository;
        protected readonly RadiusRepository RadiusRepository;

        public BatchTestBase(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .Build();
            HostBuilder = Host.CreateDefaultBuilder()
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
                });


            var mainDbContextOptions = new DbContextOptionsBuilder<MainDbContext>()
                .UseMySql(Configuration.GetConnectionString("MainDbConnection"))
                .Options;
            var radiusDdbContextOptions = new DbContextOptionsBuilder<RadiusDbContext>()
                .UseMySql(Configuration.GetConnectionString("RadiusDbConnection"))
                .Options;
            MainDbContext = new MainDbContext(mainDbContextOptions);
            RadiusDbContext = new RadiusDbContext(radiusDdbContextOptions);

            UserRepository = new UserRepository(MainDbContext);
            AuthenticationRepository = new AuthenticationRepository(MainDbContext);
            RadiusRepository = new RadiusRepository(RadiusDbContext);

            TestOutputHelper.WriteLine("Database Init");
            //MainDbContext.Database.EnsureDeleted();
            //MainDbContext.Database.EnsureCreated();
            //MainDbContext.Database.Migrate();

            //RadiusDbContext.Database.EnsureDeleted();
            //RadiusDbContext.Database.EnsureCreated();
            //RadiusDbContext.Database.Migrate();

            RemoveAllEntities(MainDbContext, RadiusDbContext);
        }

        public void RemoveAllEntities(MainDbContext context,RadiusDbContext radiusDbContext)
        {
            context.RemoveRange(context.AuthenticationLog); // 認証操作
            context.RemoveRange(context.AuthenticationState); // 認証状態
            context.RemoveRange(context.MultiFactor); // 多要素組合せ
            context.RemoveRange(context.SimAndDevice); // SIM & 端末組合せ
            context.RemoveRange(context.Sim); // SIM
            context.RemoveRange(context.SimGroup); // SIMグループ

            context.RemoveRange(context.Device); // 端末
            context.RemoveRange(context.LteModule); // LTEモジュール
            context.RemoveRange(context.DeviceGroup); // 端末グループ
            context.RemoveRange(context.DeviceGroupDevice); // 端末グループ - 端末

            context.RemoveRange(context.OrganizationClientApp); // 組織端末アプリ
            context.RemoveRange(context.ClientApp); // JinCreek端末アプリ
            context.RemoveRange(context.ClientOs); // JinCreekサポート端末OS

            context.RemoveRange(context.AvailablePeriod); // 利用期間
            context.RemoveRange(context.User); // ユーザー
            context.RemoveRange(context.UserGroup); // ユーザーグループ
            context.RemoveRange(context.UserGroupEndUser); // ユーザーグループ - ユーザー
            context.RemoveRange(context.Domain); // ドメイン
            context.RemoveRange(context.Organization); // 組織

            radiusDbContext.RemoveRange(radiusDbContext.Radusergroup);
            radiusDbContext.RemoveRange(radiusDbContext.Radcheck);
            radiusDbContext.RemoveRange(radiusDbContext.Radgroupcheck);
            radiusDbContext.RemoveRange(radiusDbContext.Radgroupreply);
            radiusDbContext.RemoveRange(radiusDbContext.Radippool);

            context.SaveChanges();
            radiusDbContext.SaveChanges();
        }

        public void Dispose()
        {
            TestOutputHelper.WriteLine("Dispose");
            //MainDbContext.Database.EnsureDeleted();
            //RadiusDbContext.Database.EnsureDeleted();
        }
    }
}
