using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

public class GeneratableEndpoint
{
    public string HandlerName { get; set; } = null!;
    public string RouteUrl { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public string Version { get; set; } = null!;
    public GeneratableObject Request { get; set; } = null!;
    public GeneratableObject Response { get; set; } = null!;
    public ApiDescription OriginalApiDescription { get; set; } = null!;
    public bool UseFormData
    {
        get
        {
             return OriginalApiDescription.ParameterDescriptions.Any(x => x.Source == BindingSource.Form);
        }
    }
}

public class GeneratableObject
{
    public string Name { get; set; } = null!;
    public Type? ObjectType { get; set; } = null!;
    public List<GeneratableMember> Members { get; set; } = new();
}

public record GeneratableMember
{
    public string Name { get; set; } = null!;
    public Type? MemberType { get; set; } = null!;
    public List<GeneratableMember> Members { get; } = new();
    public bool IsNullable { get; set; }
    public bool IsList { get; set; }
}
