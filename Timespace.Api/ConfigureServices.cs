using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using MicroElements.Swashbuckle.NodaTime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NodaTime.Serialization.SystemTextJson;
using Swashbuckle.AspNetCore.SwaggerGen;
using Timespace.Api.Application.Common.Behaviours;
using Timespace.Api.Infrastructure;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Errors;
using Timespace.Api.Infrastructure.Middleware;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;
using Timespace.Api.Infrastructure.Swagger;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Timespace.Api;

public static class ConfigureServices
{
    public static void AddServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddSingleton<IClock, DateTimeProvider>();
        services.AddScoped<IAuthenticationTokenProvider, AuthenticationTokenProvider>();
        services.AddScoped<IUsageContext, UsageContext>();
        services.AddScoped<ICaptchaVerificationService, CaptchaVerificationService>();
        
        services.AddProblemDetails(ConfigureProblemDetails);
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), 
                opt => opt.UseNodaTime())
        );

        services.AddMediatR(typeof(IAssemblyMarker).Assembly);
        services.AddValidatorsFromAssembly(typeof(IAssemblyMarker).Assembly);
        services.RegisterBehaviours();

        services.AddHttpClient();
        
        services.AddDistributedMemoryCache();
        
        // Middleware
        services.AddTransient<AuthenticationTokenExtractor>();
    }

    private static void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        options.IncludeExceptionDetails = (_, _) => false;

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
                
                foreach(var kvpair in baseException.MapExtensions())
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

    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddApiVersioning(o =>
        {
            o.AssumeDefaultVersionWhenUnspecified = true;
            o.DefaultApiVersion = new ApiVersion(1, 0);
            o.ApiVersionReader = new UrlSegmentApiVersionReader();
        });
        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        services.AddSwaggerGen(opt =>
        {
            opt.OperationFilter<SwaggerDefaultValues>();
            opt.SchemaFilter<TestSchemaFilter>();
            opt.DocumentFilter<GlobalTagsDocumentFilter>();
            opt.ConfigureForNodaTime();
        });
        
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    }
    
    public static void AddAspnetServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            });
        
        services.AddSwagger();
        services.AddHttpContextAccessor();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "https://timespace.nl")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
    }
    
    public static void AddConfiguration(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<AuthenticationConfiguration>(configuration.GetSection(AuthenticationConfiguration.SectionName));
        services.Configure<UserSettingsConfiguration>(configuration.GetSection(UserSettingsConfiguration.SectionName));
        services.Configure<CaptchaConfiguration>(configuration.GetSection(CaptchaConfiguration.SectionName));
        services.Configure<ExternalSourceGenerationSettings>(
            configuration.GetSection(ExternalSourceGenerationSettings.SectionName));
    }
    
    public static void AddApiExplorerServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IApiDescriptionProvider, DefaultApiDescriptionProvider>());
    }

}

