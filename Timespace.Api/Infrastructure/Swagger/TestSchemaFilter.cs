using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Timespace.Api.Infrastructure.Swagger;

public class TestSchemaFilter : ISchemaFilter
{
    private readonly string[] blacklist;
    // keep types that have matching System type names with our model
    private readonly string[] whitelist = new[] { "Currency" };

    public TestSchemaFilter()
    {
        var mscorlib = typeof(string).Assembly;
        var types = mscorlib.GetTypes()
            .Where(t => t.Namespace?.Contains("System") == true);
        blacklist = types.Select(t => t.Name)
            .Where(t => !whitelist.Contains(t)).ToArray();
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties != null)
        {
            foreach (var prop in schema.Properties)
            {
                if (prop.Value.Reference != null
                    && blacklist.Contains(prop.Value.Reference.Id))
                {
                    prop.Value.Reference = null;
                }
            }
        }

        foreach (var key in blacklist)
        {
            context.SchemaRepository.Schemas.Remove(key);
        }
    }
}