using Microsoft.CodeAnalysis;
using TimeSpace.Shared.TypescriptGenerator;

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

	private static string? GetName(IEnumerable<AttributeData> attributes)
	{
		return attributes.FirstOrDefault(x =>
				x.AttributeClass?.Name is "FromQueryAttribute" or "FromPathAttribute" or "FromBodyAttribute" or "FromFormAttribute")?
			.NamedArguments
			.FirstOrDefault(x => x.Key == "Name")
			.Value
			.Value?
			.ToString();
	}

	public static string? GetNameFromBindingAttributeIfExists(this ISymbol symbol)
	{
		return GetName(symbol.GetAttributes());
	}

	public static ParameterSource? GetSymbolBindingSource(this ISymbol symbol)
	{
		var attributes = symbol.GetAttributes();
		if (attributes.Any(x => x.AttributeClass?.Name == "FromQueryAttribute"))
			return ParameterSource.Query;

		if (attributes.Any(x => x.AttributeClass?.Name == "FromRouteAttribute"))
			return ParameterSource.Path;

		if (attributes.Any(x => x.AttributeClass?.Name == "FromBodyAttribute"))
			return ParameterSource.Body;

		if (attributes.Any(x => x.AttributeClass?.Name == "FromFormAttribute"))
			return ParameterSource.Form;

		return null;
	}
}
