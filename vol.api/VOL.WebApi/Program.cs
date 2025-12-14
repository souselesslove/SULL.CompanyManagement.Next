using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Sinks.MSSqlServer;
using VOL.Core.Configuration;
using VOL.WebApi.Controllers.MqDataHandle;
using Microsoft.AspNetCore.Builder;
using VOL.Core.RabbitMQ;
using static VOL.Core.RabbitMQ.IMQDeclarator;

namespace VOL.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

                // 配置日志开始
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile("printerMap.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(
                        $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                        optional: true)
                    .Build();
                if (!string.IsNullOrWhiteSpace(configuration["App:Region"]))
                {
                    Console.Title = $"{configuration["App:DisplayTitle"]} - {configuration["App:Company"]} - {configuration["App:Region"]}";
                }
                else
                {
                    Console.Title = $"{configuration["App:DisplayTitle"]} - {configuration["App:Company"]}";
                }
                var builder = CreateHostBuilder(args, configuration);
                ConfigureLogging(builder, configuration);
                Log.Information($">> Starting {configuration["App:DisplayTitle"]}...");
                var host = builder.Build();

                var declarator = host.Services.GetRequiredService<IMQDeclarator>();
                Task.WaitAll(declarator.MQDeclareAsync());

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, ">> Host terminated unexpectedly!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureLogging(IHostBuilder builder, IConfigurationRoot configuration)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var format = configuration["Serilog:OutFormat"];
            if (string.IsNullOrWhiteSpace(format))
            {
                // "[{Timestamp:HH:mm:ss.fff} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"
                format = "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            }
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("ProductName", configuration["App:Name"])
                .Enrich.WithProperty("ProductDesc", configuration["App:DisplayTitle"])
                .WriteTo.Async(writeTo =>
                {
                    writeTo.Debug(outputTemplate: format);
                    writeTo.Console(outputTemplate: format);
                    writeTo.File("Logs/SULL_.log",
                    fileSizeLimitBytes: 10485760,
                    outputTemplate: format,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 5000,
                    rollingInterval: RollingInterval.Hour
                );
                    writeTo.MSSqlServer(connectionString: configuration["Connection:DbConnectionString"],
                        sinkOptions: new MSSqlServerSinkOptions()
                        {
                            TableName = "DbLogs",
                            AutoCreateSqlTable = true,
                        });
                })
                .Enrich.WithProperty("Environment", environment)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            builder.UseSerilog();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot configuration) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(a => a.AddConfiguration(configuration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // webBuilder.ConfigureKestrel(serverOptions =>
                    // {
                    //     serverOptions.Limits.MaxRequestBodySize = 10485760;
                    //     // Set properties and call methods on options
                    // });
                    var hostUri = new Uri(configuration["App:SelfUrl"]);
                    var url = $"{hostUri.Scheme}://{hostUri.Host}:{hostUri.Port}";

                    Console.Title = $"{Console.Title} - {url}";
                    if (hostUri.Host == "0.0.0.0")
                    {
                        webBuilder.UseKestrel(opt =>
                        {
                            opt.ListenAnyIP(hostUri.Port);
                        });
                    }
                    else
                    {
                        webBuilder.UseKestrel().UseUrls(url);
                    }
                    webBuilder.UseIIS();
                    webBuilder.UseStartup<Startup>();
                }).UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
