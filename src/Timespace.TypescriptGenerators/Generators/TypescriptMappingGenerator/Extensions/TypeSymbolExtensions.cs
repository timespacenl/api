using Microsoft.CodeAnalysis;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Extensions;

public static class TypeSymbolExtensions
{
	public static bool IsNullable(this ITypeSymbol? typeSymbol)
	{
		return typeSymbol is not null && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
	}
	public static bool IsPassthroughType(this ITypeSymbol type)
	{
		return type is INamedTypeSymbol {IsGenericType: true} namedTypeSymbol
			&& (Constants.PassthroughTypes.Contains(namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
				|| Constants.PassthroughTypes.Contains(namedTypeSymbol.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
	}

	public static bool IsWrappedNullable(this ITypeSymbol typeSymbol)
	{
		return typeSymbol is INamedTypeSymbol {IsGenericType: true} namedTypeSymbol
			&& namedTypeSymbol.ConstructedFrom.ToFullyQualifiedDisplayString() == "global::System.Nullable<T>";
	}

	public static bool IsDefaultMappable(this ITypeSymbol? typeSymbol)
	{
		return typeSymbol is not null && Constants.DefaultTypeMappings.Keys.Contains(typeSymbol.ToFullyQualifiedDisplayString().Replace("?", ""));
	}

	public static string ToFullyQualifiedDisplayString(this ITypeSymbol typeSymbol)
	{
		return typeSymbol.ToDisplayString(
			SymbolDisplayFormat.FullyQualifiedFormat
				.WithMiscellaneousOptions(
					SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions &
					(SymbolDisplayMiscellaneousOptions.UseSpecialTypes ^ (SymbolDisplayMiscellaneousOptions)int.MaxValue)
				)
		);
	}
}
