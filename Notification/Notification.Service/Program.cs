﻿using System;
using System.IO;
using GreenPipes;
using MassTransit;
using EmailService;
using System.Threading.Tasks;
using Messaging.InterfacesConstants.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Service.Consumers;

namespace Notification.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile($"appsettings.json", optional: false);
                    configHost.AddEnvironmentVariables();
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        false);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var emailConfig = hostContext.Configuration
                        .GetSection("EmailConfiguration")
                        .Get<EmailConfig>();

                    services.AddSingleton(emailConfig);
                    services.AddScoped<IEmailSender, EmailSender>();
                    services.AddMassTransit(c => { c.AddConsumer<OrderProcessedEventConsumer>(); });

                    services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        cfg.Host("rabbitmq", "/", h => { });
                        cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.NotificationServiceQueue, e =>
                        {
                            e.PrefetchCount = 16;
                            e.UseMessageRetry(x => x.Interval(2, TimeSpan.FromSeconds(10)));
                            e.Consumer<OrderProcessedEventConsumer>(provider);
                        });
                        cfg.ConfigureEndpoints(provider);
                    }));

                    services.AddSingleton<IHostedService, BusService>();
                });
            return hostBuilder;
        }
    }
}