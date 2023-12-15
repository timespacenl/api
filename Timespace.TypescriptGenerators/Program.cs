﻿using Microsoft.Build.Locator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Timespace.TypescriptGenerators;
using Timespace.TypescriptGenerators.Helpers;

MSBuildLocator.RegisterDefaults();

static IHost AppStartup() {
    var builder = new ConfigurationBuilder();

    builder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    // now we can setup serilog, reads from appsettings, and creates logger
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Build())
        .CreateLogger();

    Log.Logger.Information("Starting typescript generator...");

    var host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) => {
            services.Configure<ExternalSourceGenerationSettings>(
                context.Configuration.GetSection(ExternalSourceGenerationSettings.SectionName));
        })
        .UseSerilog()
        .Build();

    return host;
}

var host = AppStartup();

await host.RunSourceGenerators();