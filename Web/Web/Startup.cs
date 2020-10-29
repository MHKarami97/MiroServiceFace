using MassTransit;
using Web.Services;
using Web.RestClients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Setting>(Configuration);
            
            services.AddMassTransit();

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(
                cfg =>
                {
                    cfg.Host("rabbitmq", "/", h => { });

                    services.AddSingleton<IBus>(p => provider
                        .GetRequiredService<IBusControl>());

                    services.AddSingleton<IHostedService, BusService>();
                }));

            services.AddHttpClient<IOrderManagementApi, OrderManagementApi>();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}