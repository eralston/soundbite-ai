using AutoMapper;
using Azure.Core.Extensions;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Masticore;
using Masticore.Ad;
using Masticore.Azure.MediaServices;
using Masticore.Storage;
using Masticore.Token;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Soundbite.Api.Middleware;
using Soundbite.App;
using Soundbite.Entity;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.IO;


namespace Soundbite.Api
{
    /// <summary>
    /// Instantiated when the API project starts up, configuring all ASP.Net Core settings
    /// </summary>
    public class Startup
    {
        #region Constants

        /// <summary>
        /// Environment name for test.soundbite.cloud
        /// </summary>
        public const string EnvTest = "Test";

        /// <summary>
        /// Environment name for localhost tunneled out to the internet via ngrok (https://ngrok.com/)
        /// </summary>
        public const string EnvNgrok = "Ngrok";

        /// <summary>
        /// Gets a flag indicating if the app is running without internet
        /// </summary>
        public static bool IsAirplaneMode { get; } = MasticoreExtensions.GetEnvBool("SB_IS_AIRPLANE_MODE", false);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the IConfiguration for this setup
        /// </summary>
        public IConfiguration Configuration { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for capturing the core configuration object
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        /// <summary>
        /// Reads the default token service settings from the application config and places them 
        /// into the <see cref="TokenServiceSettings.DefaultTokenServiceSettings"/> static property.  
        /// This is the fallback value used when no other configuration is defined.
        /// </summary>        
        private void DefaultTokenServiceSettings()
        {
            TokenServiceSettings tokenSettings = new TokenServiceSettings();
            Configuration.Bind("DefaultTokenServiceConfig", tokenSettings);
            TokenServiceSettings.DefaultTokenServiceSettings = tokenSettings;
        }

        /// <summary>
        /// Loads TeamsAppAzureSettings from the application configuration settings.
        /// </summary>
        /// <param name="services">Reference to the services container for the application.</param>
        private void TeamsAppAzureSettings(IServiceCollection services)
        {
            TeamsAppAzureSettings settings = new TeamsAppAzureSettings();
            Configuration.Bind("TeamsAppAzureSettings", settings);
            services.AddSingleton(settings);
        }

        /// <summary>
        /// Adds services to the app
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddAutoMapper(typeof(MappingProfile), typeof(Masticore.Entity.MappingProfile));
                services.AddCors();
                services
                    .AddControllersWithViews(cfg =>
                    {
                        // AllowEmptyInputInBodyModelBinding allows for null values to be passed in the
                        // body of post requests. SpaService.LoginThirdParty requires this setting and
                        // other situations may require it in the future.
                        cfg.AllowEmptyInputInBodyModelBinding = true;
                    });

                // Register the Swagger generator, defining 1 or more Swagger documents
                services.AddSwaggerGen(cfg =>
                {
                    cfg.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = @"JWT Authorization header using the Bearer scheme.<br/> 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      <br/></br><b>Example</b>: 'Bearer 12345abcdef'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    cfg.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                          new OpenApiSecurityScheme
                          {
                            Reference = new OpenApiReference
                              {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                              },
                              Scheme = "oauth2",
                              Name = "Bearer",
                              In = ParameterLocation.Header,
                            },
                            new List<string>()
                        }
                    });


                    cfg.SwaggerDoc("v1", new OpenApiInfo()
                    {
                        Version = "v1",
                        Title = "Soundbite.Ai Web API",
                        Description = "Soundbite.Ai is a platform that provides seamless synchronous and asynchronous messaging capabilities to our partners."
                    });

                    // Set the comments path for the Swagger JSON and UI.
                    cfg.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Soundbite.Api.xml"));
                    cfg.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Masticore.xml"));
                });

                services.AddSbApi(Configuration);
                services.AddRbac(true);
                services.AddAdSecurity(Configuration);
                services.AddAdAuthToken();

                bool isMessagingQueued = MasticoreExtensions.GetEnvBool("SB_QUEUED_MESSAGES", true);
                services.AddMessaging(Configuration, isMessagingQueued);

                bool isSyncQueued = MasticoreExtensions.GetEnvBool("SB_QUEUED_SYNC", true);
                services.AddOrgSync(Configuration, isSyncQueued);

                services.AddSpaConfig(Configuration);

                TeamsAppAzureSettings(services);
                services.AddMasticoreAzureMediaServices(Configuration);
                DefaultTokenServiceSettings();

                services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
                services.AddAzureClients(builder =>
                {
                    builder.AddBlobServiceClient(Configuration["ConnectionStrings:SbStorage:blob"], preferMsi: true);
                    builder.AddQueueServiceClient(Configuration["ConnectionStrings:SbStorage:queue"], preferMsi: true);
                });
            }
            catch (Exception ex)
            {
                LogUtils.LogCritical($"Could not {nameof(Startup)}.{nameof(ConfigureServices)}: '{ex.Message}' with trace {ex.StackTrace}", ex);
                throw;
            }
        }

        /// <summary>
        /// Responsible for setting up the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application builder reference.</param>
        /// <param name="env">Environmental information used to vary setup by environment.</param>
        /// <param name="logger">Reference to the logging mechanism.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            try
            {
#if DEBUG
                app.UsePerfLogging(logger, env);
#endif

                // Dev exceptions for dev & test
                bool isDev = env.IsDevelopment() || env.IsEnvironment(EnvTest) || env.IsEnvironment(EnvNgrok);
                if (isDev)
                {
                    app.UseDeveloperExceptionPage();
                }

                // Localhost configurations are assumed to be using the local storage emulator
                AzBlobs.IsLocalStorageEmulator = env.IsDevelopment();

                // Show all exceptions unfiltered in responses
                ApiError.Init(isDev || ApiError.IsEnvirionmentVariableEnabled);

                // Do not use HTTPS redirection when in the Ngrok environment. Https redirection uses
                // an HTTP 307 redirect which contains a reference to LOCALHOST.  While this works
                // locally in a development environment it will not work externally for testing.
                if (!env.IsEnvironment(EnvNgrok))
                {
                    app.UseHttpsRedirection();
                }

                app.UseSbErrorDetails(logger);
                app.UsePathBase("/api/v1");

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Soundbite.Ai Api v1");
                });

                app.UseRouting();
                app.UseCors(policy => policy
                    .AllowAnyMethod()
                    .WithExposedHeaders(new string[] { "*" })
                    .AllowAnyHeader()
                    //TODO: allowing any origin + allowing credentials (which we do below) is dangerous and we need to review options (see https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-5.0)
                    .SetIsOriginAllowed(origin => true)   // allow any origin  
                    .AllowCredentials());                 // CORS responses header Access-Control-Allow-Credentials is set to true

                // TODO: This probably works better as a build configuration that also removed the [Authorize] attribute from the controllers
                // No internet must be on AND in a dev mode for this to skip
                if (IsAirplaneMode && isDev)
                {
                    // Just setup the security context based on the database and god status
                    app.UseMiddleware<NoSecurityMiddleware>();
                }
                else
                {
                    app.UseMiddleware<SecurityMiddleware>();  // Authenticates Soundbite tokens and sets up security context
                    app.UseAuthorization();                   // Ensures secured endpoints are only accessed by authenticated users
                }

                // No default route. All Controllers must use attribute routing
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Could not {nameof(Startup)}.{nameof(Configure)}: '{ex.Message}' with trace {ex.StackTrace}", ex);
                throw;
            }
        }
    }

    /// <summary>
    /// Extensions added by configuring the Blob container via Publish configuration
    /// </summary>
    internal static class StartupExtensions
    {
        public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddBlobServiceClient(serviceUri);
            }
            else
            {
                return builder.AddBlobServiceClient(serviceUriOrConnectionString);
            }
        }
        public static IAzureClientBuilder<QueueServiceClient, QueueClientOptions> AddQueueServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddQueueServiceClient(serviceUri);
            }
            else
            {
                return builder.AddQueueServiceClient(serviceUriOrConnectionString);
            }
        }
    }
}