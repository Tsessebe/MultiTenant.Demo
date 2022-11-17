using System.Reflection;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Serilog;

namespace MultiTenant.Demo.Api;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var webHost = CreateHostBuilder(args).Build();

            webHost.Run();
        }
        catch (Exception runtimeException)
        {
            Log.Logger.Fatal(runtimeException, "Fatal Exception");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        var env = hostingContext.HostingEnvironment;
                        var envName = env.EnvironmentName;
                        var appName = env.ApplicationName;
                        // Add Configuration Files
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) //Standard
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true,
                                reloadOnChange: true) // Environment
                            .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true,
                                reloadOnChange: true); // Machine Specific

                        // Add Environment Variables
                        var envPrefix = $"{envName}_{appName}_";
                        config.AddEnvironmentVariables(envPrefix);

                        var programConfig = config.Build();

                        Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(programConfig)
                            .CreateLogger();


                        var awsConfig = programConfig.GetSection("AWS");

                        var profileName = awsConfig["Profile"];
                        if (!string.IsNullOrEmpty(profileName))
                        {
                            var keyChain = new CredentialProfileStoreChain();
                            if (keyChain.TryGetProfile(profileName, out var profile))
                            {
                                var credentials = profile.GetAWSCredentials(profile.CredentialProfileStore);
                                config.AddSecretsManager(credentials: credentials, region: profile.Region,
                                    configurator: options =>
                                    {
                                        options.SecretFilter = entry => entry.Name.StartsWith($"{envName}_{appName}_");
                                        options.KeyGenerator = (_, s) => s
                                            .Replace($"{envName}_{appName}_", string.Empty)
                                            .Replace("__", ":");
                                    });
                            }
                        }
                        else
                        {
                            var region = RegionEndpoint.GetBySystemName(awsConfig["Region"]);
                            config.AddSecretsManager(region: region,
                                configurator: options =>
                                {
                                    options.SecretFilter = entry => entry.Name.StartsWith($"{envName}_{appName}_");
                                    options.KeyGenerator = (_, s) => s
                                        .Replace($"{envName}_{appName}_", string.Empty)
                                        .Replace("__", ":");
                                });
                        }


                        if (args.Length > 0)
                        {
                            config.AddCommandLine(args);
                        }
                    })
                    .UseStartup<Startup>();
            })
            .UseSerilog();
}