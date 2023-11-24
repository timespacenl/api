using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Timespace.Api.Infrastructure.Swagger;

public class GlobalTagsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var apiControllerClasses = typeof(IAssemblyMarker).Assembly.GetTypes()
            .Where(x => x.GetCustomAttribute<ApiControllerAttribute>() != null).ToList();

        var version = context.ApiDescriptions.First().GetApiVersion();
        
        var tagList = new List<OpenApiTag>();

        foreach (var apiControllerClass in apiControllerClasses)
        {
            var swaggerTagAttribute = apiControllerClass.GetCustomAttribute<TagsAttribute>();
            var apiVersion = apiControllerClass.GetCustomAttribute<ApiVersionAttribute>();

            if (apiVersion == null || !apiVersion.Versions.Contains(version))
                continue;

            if (swaggerTagAttribute != null)
            {
                foreach (var tag in swaggerTagAttribute.Tags)
                {
                    tagList.Add(new()
                    {
                        Name = tag
                    });
                }

                continue;
            }

            tagList.Add(new()
            {
                Name = apiControllerClass.Name.Replace("Controller", ""),
            });
        }
        
        swaggerDoc.Tags = tagList.OrderBy(x => x.Name).ToList();
    }
}