using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddControllers();

            services.AddDbContext<MainDbContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("MainDbConnection"));
            });
            services.AddTransient<UserRepository>();
            services.AddTransient<SimDeviceRepository>();
            services.AddTransient<AuthenticationRepository>();

            services.AddDbContext<RadiusDbContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("RadiusDbConnection"));
            });
            services.AddTransient<RadiusRepository>();

            services.AddSwaggerDocument(config =>
                {
                    config.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "JinCreek �F�؃A�v�� API";
                        document.Info.Description = "JinCreek API";
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
