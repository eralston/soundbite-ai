using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Soundbite.Api
{
    /// <summary>
    /// Entry point class for the app
    /// </summary> 
    public class Program
    {
        /// <summary>
        /// Bootstraps the app and runs it
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                // Despite 
                LogUtils.LogCritical($"Could not start in {nameof(Program)}.{nameof(Main)}: '{ex.Message}' with trace {ex.StackTrace}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create the initial configuration object for the system
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                           .ConfigureAppConfiguration((context, config) =>
                           {
                               LoadKeyVault(config);
                           })
                           .ConfigureWebHostDefaults(webBuilder =>
                           {
                               webBuilder.UseStartup<Startup>();
                           });
        }

        private static void LoadKeyVault(IConfigurationBuilder config)
        {
            try
            {
                string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (envName != "Development" && envName != "Ngrok")
                {
                    Uri keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
                    config.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
                }
            }
            catch (Exception ex)
            {
                LogUtils.LogCritical($"Could not load key vault while starting in {nameof(Program)}.{nameof(LoadKeyVault)}: '{ex.Message}' with trace {ex.StackTrace}", ex);
                throw;
            }
        }
    }
}
