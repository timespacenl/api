using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

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

public record GeneratableObject
{
    public string Name { get; init; } = null!;
    public ITypeSymbol? ObjectType { get; init; } = null!;
    public List<GeneratableMember> Members { get; init; } = new();
}

public interface IGeneratableMember
{
    public ITypeSymbol? MemberType { get; init; }
    public List<IGeneratableMember> Members { get; init; }
    public CollectionType CollectionType { get; init; }
    public bool IsNullable => MemberType is not null && MemberType?.NullableAnnotation == NullableAnnotation.Annotated;
}

public record GeneratableMember : IGeneratableMember
{
    public string Name { get; init; } = null!;
    public ITypeSymbol? MemberType { get; init; } = null!;
    public List<IGeneratableMember> Members { get; init; } = new();
    public CollectionType CollectionType { get; init; }
}

public record GeneratableTypeArgument : IGeneratableMember
{
    public ITypeSymbol? MemberType { get; init; } = null!;
    public List<IGeneratableMember> Members { get; init; } = new();
    public CollectionType CollectionType { get; init; }
    public int ArgumentPosition { get; init; }
}

public enum CollectionType
{
    None,
    List,
    Dictionary
}
