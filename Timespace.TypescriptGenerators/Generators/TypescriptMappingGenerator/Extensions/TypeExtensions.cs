using System.Collections;
using Microsoft.CodeAnalysis;
using NodaTime;
using Timespace.Api.Application.Features.ExternalSourceGeneration;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;

public static class TypeExtensions
{
    public static bool IsCollectionType(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol {IsGenericType: true} namedTypeSymbol && namedTypeSymbol.Interfaces.Any(x => x.Name == "IEnumerable");
    }
    
    public static bool IsDictionaryType(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol {IsGenericType: true} namedTypeSymbol && namedTypeSymbol.Interfaces.Any(x => x.Name == "IDictionary");
    }
    
    private static bool IsQuerySource(IEnumerable<AttributeData> attributes)
    {
        return attributes.Any(x => x.AttributeClass?.Name == "FromQueryAttribute");
    }
    
    private static bool IsNotQuerySource(IEnumerable<AttributeData> attributes)
    {
        return attributes.Any(x => x.AttributeClass?.Name is "FromPathAttribute" or "FromBodyAttribute" or "FromFormAttribute");
    }
    
    private static string? GetName(IEnumerable<AttributeData> attributes)
    {
        return attributes.FirstOrDefault(x => x.AttributeClass?.Name is "FromQueryAttribute" or "FromPathAttribute" or "FromBodyAttribute" or "FromFormAttribute")?
            .NamedArguments
            .FirstOrDefault(x => x.Key == "Name")
            .Value
            .Value?
            .ToString();
    }
    
    public static bool IsQuerySource(this ISymbol symbol) => IsQuerySource(symbol.GetAttributes());
    public static bool IsNotQuerySource(this ISymbol symbol) => IsNotQuerySource(symbol.GetAttributes());
    public static string? GetNameFromBindingAttributeIfExists(this ISymbol symbol) => GetName(symbol.GetAttributes());
    
    public static ParameterSource? GetSymbolBindingSource(this ISymbol symbol)
    {
        var attributes = symbol.GetAttributes();
        if(attributes.Any(x => x.AttributeClass?.Name == "FromQueryAttribute"))
            return ParameterSource.Query;
        
        if(attributes.Any(x => x.AttributeClass?.Name == "FromRouteAttribute"))
            return ParameterSource.Path;
        
        if(attributes.Any(x => x.AttributeClass?.Name == "FromBodyAttribute"))
            return ParameterSource.Body;
        
        if(attributes.Any(x => x.AttributeClass?.Name == "FromFormAttribute"))
            return ParameterSource.Form;
        
        return null;
    }
    
    public static bool IsMediatrHandlerClass(this ITypeSymbol type)
    {
        var attributes = type.GetAttributes();
        if(attributes.Any(x => x.AttributeClass?.Name == "GenerateMediatrAttribute"))
            return true;
        
        return false;
    }
    
    public static bool IsNullableValueType(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    
    public static bool IsEnumerableT(this Type type)
    {
        return type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null;
    }
    
    public static bool IsNodaTimeType(this Type type)
    {
        return type == typeof(Instant) || type == typeof(LocalDate) || type == typeof(LocalDateTime);
    }
    
    public static bool IsMappablePrimitive(this Type type)
    {
        if(Constants.MappableTypesMapping.TryGetValue(type.Name, out var tsType))
        {
            return true;
        }
        
        return false;
    }
    
    public static bool IsSharedType(this List<SharedType> sharedTypes, Type type)
    {
        return sharedTypes.Any(x => x.OriginalType == type);
    }
}
