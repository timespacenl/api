using Microsoft.CodeAnalysis;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;

public static class TypeSymbolExtensions
{
    public static string GetTsType(this ITypeSymbol typeSymbol)
    {
        if(Constants.TsTypeMapping.TryGetValue(typeSymbol.Name, out var tsType))
        {
            return tsType;
        }
        
        throw new NotImplementedException($"Type {typeSymbol.Name} is not implemented in TS type mapping");
    }
}