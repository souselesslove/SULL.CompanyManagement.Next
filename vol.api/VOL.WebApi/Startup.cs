using Autofac;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using VOL.Core.Configuration;
using VOL.Core.Extensions;
using VOL.Core.Filters;
using VOL.Core.Middleware;
using VOL.Core.ObjectActionValidator;
using VOL.Core.Quartz;
using VOL.Core.Utilities.PDFHelper;
using VOL.Core.WorkFlow;
using VOL.Entity.DomainModels;
using VOL.WebApi.Controllers.Hubs;
using VOL.WebApi.Utils;
using SULL.HospitalManagement.Services.Consumers;
using VOL.Core.RabbitMQ;
using static VOL.Core.RabbitMQ.IMQDeclarator;
using SULL.HospitalManagement.Jobs;
using Microsoft.AspNetCore.StaticFiles;

namespace VOL.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private IServiceCollection Services { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 限制上传或请求内容大小
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 1073741824; //1GB
            });
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = 1073741824; //1GB
                x.MultipartBodyLengthLimit = 1073741824; //1GB
            });
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 1073741824; //1GB
            });

            //初始化模型验证配置
            services.UseMethodsModelParameters().UseMethodsGeneralParameters();
            services.AddSingleton<IObjectModelValidator>(new NullObjectModelValidator());
            Services = services;
            // services.Replace( ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            services.AddSession();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ApiAuthorizeFilter));
                options.Filters.Add(typeof(ActionExecuteFilter));
                //  options.SuppressAsyncSuffixInActionNames = false;
            });
            services.AddControllers()
                .AddNewtonsoftJson(op =>
                {
                    op.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                    op.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                });

            Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        SaveSigninToken = true, //保存token,后台验证token是否生效(重要)
                        ValidateIssuer = true, //是否验证Issuer
                        ValidateAudience = true, //是否验证Audience
                        ValidateLifetime = true, //是否验证失效时间
                        ValidateIssuerSigningKey = true, //是否验证SecurityKey
                        ValidAudience = AppSetting.Secret.Audience, //Audience
                        ValidIssuer = AppSetting.Secret.Issuer, //Issuer，这两项和前面签发jwt的设置一致
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.Secret.JWT))
                    };
                    options.Events = new JwtBearerEvents()
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.Clear();
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = 401;
                            context.Response.WriteAsync(
                                new { message = "授权未通过", status = false, code = 401 }.Serialize());
                            return Task.CompletedTask;
                        }
                    };
                });
            //必须appsettings.json中配置
            string corsUrls = Configuration["CorsUrls"];
            if (string.IsNullOrEmpty(corsUrls))
            {
                throw new Exception("请配置跨请求的前端Url");
            }

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
                            .AllowAnyHeader().AllowAnyMethod();
                    });
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // 配置RabbitMQ
            services.AddRabbitMq(Configuration);
            services.AddSingleton<IMQDeclarator, MQDeclarator>();
            services.AddSingleton<IMessageProducer, RabbitMessageProducer>();
            services.AddHostedService<TodoConsumer>(); // 注册消息消费者

            // 注册后台定时任务
            services.AddSingleton<IBackgroundJob, CheckWarningsJob>();
            services.AddHostedService<SchedulerHostedService>();


            services.AddControllers();
            services.AddSwaggerGen(c =>
                {
                    //分为2份接口文档
                    //控制器里使用[ApiExplorerSettings(GroupName = "v2")]              
                    //启用中文注释功能
                    // var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                    //  var xmlPath = Path.Combine(basePath, "VOL.WebApi.xml");
                    //   c.IncludeXmlComments(xmlPath, true);//显示控制器xml注释内容
                    //添加过滤器 可自定义添加对控制器的注释描述 如：添加url过滤
                    c.DocumentFilter<SwaggerFilter>();
                    c.SwaggerDoc("v1",
                        new OpenApiInfo { Title = "VOL.Core后台Api", Version = "v1", Description = "这是对文档的描述。。" });
                    c.SwaggerDoc("v2",
                        new OpenApiInfo { Title = "VOL.Core对外三方Api", Version = "v2", Description = "xxx接口文档" });
                    var security = new Dictionary<string, IEnumerable<string>>
                        { { AppSetting.Secret.Issuer, new string[] { } } };
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Description = "JWT授权token前面需要加上字段Bearer与一个空格,如Bearer token",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
                })
                .AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressConsumesConstraintForFormFileParameters = true;
                    options.SuppressInferBindingSourcesForParameters = true;
                    options.SuppressModelStateInvalidFilter = true;
                    options.SuppressMapClientErrors = true;
                    options.ClientErrorMapping[404].Link =
                        "https://*/404";
                });
            services.AddSignalR();
            //services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            //services.AddTransient<IPDFService, PDFService>();

            services.AddHttpClient();
            Services.AddTransient<HttpResultfulJob>();
            Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            Services.AddSingleton<Quartz.Spi.IJobFactory, IOCJobFactory>();
            Services.AddSerilog();

            #region UniPush

            Services.AddHttpClient<IUniPushService, UniPushService>((serviceProvider, client) =>
                {
                    var config = serviceProvider.GetRequiredService<IConfiguration>();
                    client.BaseAddress = new Uri(config["UniPush:BaseUrl"]);
                    client.Timeout = TimeSpan.FromSeconds(30);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    AutomaticDecompression =
                        DecompressionMethods.GZip | DecompressionMethods.Deflate
                });
            Services.AddSingleton<IUniPushService, UniPushService>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(typeof(UniPushService).FullName); // 专用客户端
                var logger = sp.GetRequiredService<ILogger<UniPushService>>();
                var url = sp.GetRequiredService<IConfiguration>()["UniPush:Url"]; // 完整 URL

                return new UniPushService(httpClient, logger, url);
            });

            #endregion
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            Services.AddModule(builder, Configuration);
            //初始化流程表，表里面必须有AuditStatus字段
            WorkFlowContainer.Instance
               .Use<MES_ProductionReporting>(
                 "生产报工",
                    filterFields: x => new { x.ReportingNumber, x.AcceptedQuantity, x.RejectedQuantity, x.Total, x.ReportedBy, x.ReportingTime },
                    //审批界面显示表数据字段
                    formFields: x => new { x.ReportedBy, x.ReportingNumber, x.ReportingTime, x.AcceptedQuantity, x.RejectedQuantity, x.Total }
                )
                .Use<WorkFlow>(
                    "工作流程",
                    filterFields: x => new { x.Topic, x.CreateDate, x.AuditStatus },
                    //审批界面显示表数据字段
                    formFields: x => new { x.Topic, x.CreateDate, x.Creator, x.AuditStatus, x.AuditUser }
                )
                //run方法必须写在最后位置
                .Run();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseQuartz(env);
            }

            app.UseMiddleware<ExceptionHandlerMiddleWare>();
            app.UseDefaultFiles();

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".apk"] = "application/vnd.android.package-archive";
            app.UseStaticFiles().UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                ContentTypeProvider = provider
            });
            app.Use(HttpRequestMiddleware.Context);

            //2021.06.27增加创建默认upload文件夹
            var uploadPath = (env.ContentRootPath + "/Upload").ReplacePath();

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), @"Upload")),
                //配置访问虚拟目录时文件夹别名
                RequestPath = "/Upload",
                OnPrepareResponse = (Microsoft.AspNetCore.StaticFiles.StaticFileResponseContext staticFile) =>
                {
                    //可以在此处读取请求的信息进行权限认证
                    //  staticFile.File
                    //  staticFile.Context.Response.StatusCode;
                }
            });
            //配置HttpContext
            app.UseStaticHttpContext();

            // 配置转发
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                // 可以根据需要配置 KnownProxies 和 KnownNetworks 来增加安全性
                // KnownProxies = { IPAddress.Parse("::1"), IPAddress.Parse("127.0.0.1") },
                // KnownNetworks = { new IPNetwork(IPAddress.Parse("10.0.0.0"), 8) }
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //2个下拉框选项  选择对应的文档
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VOL.Core后台Api");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "测试第三方Api");
                c.RoutePrefix = "";
            });
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //配置SignalR
                if (AppSetting.UseSignalR)
                {
                    string corsUrls = Configuration["CorsUrls"];

                    endpoints.MapHub<HomePageMessageHub>("/message")
                    .RequireCors(t =>
                    t.WithOrigins(corsUrls.Split(',')).
                    AllowAnyMethod().
                    AllowAnyHeader().
                    AllowCredentials());

                    endpoints.MapHub<MLPrinterHub>("/mlprinter")
                        .RequireCors(t =>
                            t.WithOrigins(corsUrls.Split(',')).
                                AllowAnyMethod().
                                AllowAnyHeader().
                                AllowCredentials());
                }
            });
        }
    }
}
