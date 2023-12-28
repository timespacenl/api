using Microsoft.CodeAnalysis;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;

public static class TypeSymbolExtensions
{
    public static bool IsNullable(this ITypeSymbol? typeSymbol) => typeSymbol is not null && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
    public static bool IsPassthroughType(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol {IsGenericType: true} namedTypeSymbol && (Constants.PassthroughTypes.Contains(namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                                                                                  || Constants.PassthroughTypes.Contains(namedTypeSymbol.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
    }
    
    public static bool IsWrappedNullable(this ITypeSymbol typeSymbol) => typeSymbol is INamedTypeSymbol {IsGenericType: true} namedTypeSymbol && namedTypeSymbol.ConstructedFrom.ToFullyQualifiedDisplayString() == "global::System.Nullable<T>";
    
    public static bool IsDefaultMappable(this ITypeSymbol? typeSymbol) => typeSymbol is not null && Constants.DefaultMappableTypes.Contains(typeSymbol.ToFullyQualifiedDisplayString().Replace("?", ""));
    public static bool IsDefaultMappable(string typeSymbol) => Constants.DefaultMappableTypes.Contains(typeSymbol.Replace("?", ""));
    public static string ToFullyQualifiedDisplayString(this ITypeSymbol typeSymbol) => typeSymbol.ToDisplayString(
        SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions &
            (SymbolDisplayMiscellaneousOptions.UseSpecialTypes ^ (SymbolDisplayMiscellaneousOptions)Int32.MaxValue)
            )
        );
}