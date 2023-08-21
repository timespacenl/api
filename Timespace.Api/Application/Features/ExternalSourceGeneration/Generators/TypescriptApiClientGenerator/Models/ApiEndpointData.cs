using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Models;

public class EndpointInfo
{
    public string GroupName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public ApiDescription OriginalApiDescription { get; set; } = null!;
    public INamedTypeSymbol HandlerClassSymbol { get; set; } = null!;
    public INamedTypeSymbol RequestSymbol { get; set; } = null!;
    public INamedTypeSymbol ResponseSymbol { get; set; } = null!;

    public List<ApiParameterDescription> Parameters { get; set; } = new();
    public ObjectInfo Request { get; set; } = null!;
    public ObjectInfo Response { get; set; } = null!;
}

public class ObjectInfo
{
    public string Name { get; set; } = null!;
    public List<MemberInfo> Members { get; set; } = new();
    public INamedTypeSymbol OriginalSymbol { get; set; } = null!;
}

public class MemberInfo
{
    public string Name { get; set; } = null!;
    public ITypeSymbol Type { get; set; } = null!;
    public bool IsList { get; set; } = false;
    public List<MemberInfo> Children { get; set; } = new();
    public List<FluentValidator> FluentValidators { get; set; } = new();
    public IPropertySymbol OriginalSymbol { get; set; } = null!;
}

public class FluentValidator
{
    public string ValidatorName { get; set; } = null!;
    public List<FluentValidatorParameter> Parameters { get; set; } = new();
    public IInvocationOperation OriginalInvocation { get; set; } = null!;
}

public class FluentValidatorParameter
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public ITypeSymbol Type { get; set; } = null!;
}