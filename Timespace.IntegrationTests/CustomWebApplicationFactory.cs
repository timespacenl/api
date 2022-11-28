using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.IntegrationTests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            configurationBuilder.AddConfiguration(integrationConfig);
        });
        
        builder.ConfigureServices((webHostBuilderContext, services) =>
        {
            services
                .Remove<DbContextOptions<AppDbContext>>()
                .AddDbContext<AppDbContext>((sp, options) =>
                    options.UseNpgsql(webHostBuilderContext.Configuration.GetConnectionString("DefaultConnection"),
                        npgsqlDbContextOptionsBuilder => npgsqlDbContextOptionsBuilder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        });
    }
}