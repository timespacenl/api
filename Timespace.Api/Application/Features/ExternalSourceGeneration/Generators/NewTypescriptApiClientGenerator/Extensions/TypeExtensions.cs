namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.NewTypescriptApiClientGenerator.Extensions;

public static class TypeExtensions
{
    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    
    public static bool IsList(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }
    
    public static string GetTsType(this Type type)
    {
        if(Constants.TsTypeMapping.TryGetValue(type.Name, out var tsType))
        {
            return tsType;
        }
        
        throw new NotImplementedException($"Type {type.Name} is not implemented in TS type mapping");
    }
}