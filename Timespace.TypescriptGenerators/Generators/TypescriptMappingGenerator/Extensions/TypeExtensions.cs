using System.Collections;
using Microsoft.CodeAnalysis;
using NodaTime;
using Timespace.Api.Application.Features.ExternalSourceGeneration;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;

public static class TypeExtensions
{
    public static bool IsBuiltInType(this ITypeSymbol type)
    {
        return Constants.BuiltInTypes.Contains(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
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
