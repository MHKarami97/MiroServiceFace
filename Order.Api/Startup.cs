using System;
using GreenPipes;
using MassTransit;
using Order.Api.Services;
using Order.Api.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Messaging.InterfacesConstants.Constants;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Hubs;
using Order.Api.Messages.Consumers;

namespace Order.Api
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
            services.AddDbContext<OrdersContext>(options => options.UseSqlServer
            (
                Configuration.GetConnectionString("OrdersContextConnection")
            ));

            services.AddSignalR()
                .AddJsonProtocol(options => { options.PayloadSerializerOptions.PropertyNamingPolicy = null; });

            services.AddMassTransit(
                c =>
                {
                    c.AddConsumer<RegisterOrderCommandConsumer>();
                    c.AddConsumer<OrderDispatchedEventConsumer>();
                });

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("localhost", "/", h => { });
                cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.RegisterOrderCommandQueue, e =>
                {
                    e.PrefetchCount = 16;
                    e.UseMessageRetry(x => x.Interval(2, TimeSpan.FromSeconds(10)));
                    e.Consumer<RegisterOrderCommandConsumer>(provider);
                });

                cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.OrderDispatchedServiceQueue, e =>
                {
                    e.PrefetchCount = 16;
                    e.UseMessageRetry(x => x.Interval(2, 100));

                    e.Consumer<OrderDispatchedEventConsumer>(provider);
                    //  EndpointConvention.Map<OrderDispatchedEvent>(e.InputAddress);
                });

                cfg.ConfigureEndpoints(provider);
            }));

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(host => true)
                        .AllowCredentials());
            });

            services.AddHttpClient();

            services.AddTransient<IOrderRepository, OrderRepository>();

            services.AddSingleton<IHostedService, BusService>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<OrderHub>("/orderhub");
            });
        }
    }
}