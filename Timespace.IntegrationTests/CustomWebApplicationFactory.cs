using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.IntegrationTests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly IConfigurationRoot IntegrationConfig = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddConfiguration(IntegrationConfig);
        });
        
        builder.ConfigureServices((_, services) =>
        {
            services
                .Remove<IClock>()
                .AddSingleton<IClock, FakeClock>(_ => FakeClock.FromUtc(2022, 11, 29));
            
            services
                .Remove<DbContextOptions<AppDbContext>>()
                .AddDbContext<AppDbContext>((_, options) =>
                    options.UseNpgsql(IntegrationConfig.GetConnectionString("DefaultConnection"),
                        npgsqlDbContextOptionsBuilder =>
                        {
                            npgsqlDbContextOptionsBuilder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                            npgsqlDbContextOptionsBuilder.UseNodaTime();
                        }));
        });
    }
}