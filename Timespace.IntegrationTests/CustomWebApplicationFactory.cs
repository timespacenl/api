using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NodaTime;
using NodaTime.Testing;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.IntegrationTests;

using static Testing;

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
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
                
            services.Remove<IHttpContextAccessor>()
                .AddSingleton(httpContextAccessorMock.Object);

            services.Remove<IAuthenticationTokenProvider>()
                .AddScoped(_ => Mock.Of<IAuthenticationTokenProvider>(s =>
                    s.AuthenticationTokenType == AuthenticationTokenType.UserSession &&
                    s.AuthenticationToken == GetUserAuthToken()));
            
            services
                .Remove<IClock>()
                .AddSingleton<IClock>(_ => FakeClock.FromUtc(2021, 1, 1, 1, 1, 1));

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