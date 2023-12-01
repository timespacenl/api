using System.Collections;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;

public static class TypeExtensions
{
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
