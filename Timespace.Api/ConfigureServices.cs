using NodaTime;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api;

public static class ConfigureServices
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, DateTimeProvider>();
    }

    public static void AddAspnetServices(this IServiceCollection services)
    {
        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
    
    public static void AddConfiguration(this IServiceCollection services, ConfigurationManager configuration)
    {
        // services.Configure<AuthenticationSettings>(configuration.GetSection(AuthenticationSettings.SectionName));
        // services.Configure<SessionCookieSettings>(configuration.GetSection(SessionCookieSettings.SectionName));
    }
}