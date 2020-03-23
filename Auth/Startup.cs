using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;
using JinCreek.Server.Auth.Services;

namespace JinCreek.Server.Auth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(new ServerVersionHeaderAttribute());

            }).AddNewtonsoftJson(options =>
            {
                // Use the default property (Pascal) casing
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();

                // Configure a custom converter
                //options.SerializerOptions.Converters.Add(new MyCustomJsonConverter());
            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Azure")
            {
                services.AddDbContext<MainDbContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("MainDbConnection"));
                });
                services.AddDbContext<RadiusDbContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("RadiusDbConnection"));
                });
                services.BuildServiceProvider().GetService<MainDbContext>().Database.Migrate();
            }
            else
            {
                var a = Configuration.GetConnectionString("MainDbConnection");
                services.AddDbContext<MainDbContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("MainDbConnection"));
                });
                services.AddDbContext<RadiusDbContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("RadiusDbConnection"));
                });
            }
            services.AddTransient<UserRepository>();
            services.AddTransient<AuthenticationRepository>();

            services.AddDbContext<RadiusDbContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("RadiusDbConnection"));
            });
            services.AddTransient<RadiusRepository>();

            services.AddOpenApiDocument(config =>
                {
                    config.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "JinCreek 認証アプリ API";
                        document.Info.Description = "JinCreekサービス の 認証アプリ APIです。";
                        document.Info.TermsOfService = "None";
                    };
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseLoggingMiddleware();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}
