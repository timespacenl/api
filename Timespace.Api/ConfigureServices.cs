using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Timespace.Api.Infrastructure.Errors;
using Timespace.Api.Infrastructure.Services;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Timespace.Api;

public static class ConfigureServices
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, DateTimeProvider>();
        services.AddProblemDetails(ConfigureProblemDetails);
        //services.AddTransient<ProblemDetailsFactory, TimespaceProblemDetailsFactory>();
    }

    private static void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        options.IncludeExceptionDetails = (_, _) => false;
        // Custom mapping function for FluentValidation's ValidationException.
        // options.MapFluentValidationException();

        options.Rethrow<NotSupportedException>();

        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);

        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);
        
        options.Map<Exception>((context, ex) =>
        {
            if(ex is IBaseException baseException)
            {
                var problemDetails = new ProblemDetails
                {
                    Type = baseException.Type,
                    Title = baseException.Title,
                    Status = baseException.StatusCode,
                    Instance = context.Request.Path,
                    Detail = baseException.Detail
                };
                
                foreach(var kvpair in baseException.Extensions)
                {
                    problemDetails.Extensions.Add(kvpair);
                }

                return problemDetails;
            }

            var internalErrorProblemDetails = new ProblemDetails
            {
                Type = "internal-server-error",
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An internal server error occurred.",
                Instance = context.Request.Path
            };

            return internalErrorProblemDetails;
        });
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