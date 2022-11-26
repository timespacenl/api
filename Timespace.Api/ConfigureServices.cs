﻿using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using Swashbuckle.AspNetCore.SwaggerGen;
using Timespace.Api.Application.Common.Behaviours;
using Timespace.Api.Infrastructure;
using Timespace.Api.Infrastructure.Errors;
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
        
        services.AddProblemDetails(ConfigureProblemDetails);
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatR(typeof(IAssemblyMarker).Assembly);
        services.AddValidatorsFromAssembly(typeof(IAssemblyMarker).Assembly);
        services.RegisterBehaviours();
        
        services.AddDistributedMemoryCache();
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
        });
        
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    }
    
    public static void AddAspnetServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwagger();
    }
    
    public static void AddConfiguration(this IServiceCollection services, ConfigurationManager configuration)
    {
        // services.Configure<AuthenticationSettings>(configuration.GetSection(AuthenticationSettings.SectionName));
        // services.Configure<SessionCookieSettings>(configuration.GetSection(SessionCookieSettings.SectionName));
    }
}