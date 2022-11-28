using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Timespace.Api.Infrastructure.Swagger;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider) 
        => _apiVersionDescriptionProvider = apiVersionDescriptionProvider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateOpenApiInfo(description));
            options.SchemaGeneratorOptions.SchemaIdSelector = (type) =>
            {
                if (type.IsNested)
                {
                    return type.ToString().Split('.').Last().Replace("+", "");
                }

                return type.ToString().Split('.').Last();
            };
        }
    }

    private static OpenApiInfo CreateOpenApiInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = "Timespace API",
            Version = description.ApiVersion.ToString(),
        };

        if (description.IsDeprecated)
        {
            info.Description += " (deprecated)";
        }

        return info;
    }
}