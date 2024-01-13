using Microsoft.CodeAnalysis;
using TimeSpace.Shared.TypescriptGenerator;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

public record EndpointParameter(string Name, ITypeSymbol TypeSymbol, ITypeSymbol? DefinedBy, ParameterSource Source);

public abstract record ApiType
{
	public required string TypeName { get; init; } = null!;
	public required string FullyQualifiedTypeName { get; init; } = null!;
}

public record ApiTypeClass : ApiType
{
	public required List<ApiClassProperty> Properties { get; init; } = new();
}

public record ApiTypeEnum : ApiType
{
	public required List<ApiEnumValue> Values { get; init; } = new();
}

public record CollectionInfo
{
	public required CollectionType CollectionType { get; init; }
	public bool IsNullable { get; init; }
	public string? ValueTypeName { get; init; }
	public string? FullyQualifiedValueTypeName { get; init; }
	public string? KeyTypeName { get; init; }
	public string? FullyQualifiedKeyTypeName { get; init; }
}

public static class CollectionInfoHelpers
{
	public static CollectionInfo None => new()
	{
		CollectionType = CollectionType.None,
	};

	public static CollectionInfo List(string? valueTypeName, string? fullyQualifiedValueTypeName)
	{
		return new CollectionInfo
		{
			CollectionType = CollectionType.List, ValueTypeName = valueTypeName, FullyQualifiedValueTypeName = fullyQualifiedValueTypeName,
		};
	}

	public static CollectionInfo Dictionary(string? keyTypeName, string? fullyQualifiedKeyTypeName, string? valueTypeName,
		string? fullyQualifiedValueTypeName)
	{
		return new CollectionInfo
		{
			CollectionType = CollectionType.Dictionary,
			KeyTypeName = keyTypeName,
			FullyQualifiedKeyTypeName = fullyQualifiedKeyTypeName,
			ValueTypeName = valueTypeName,
			FullyQualifiedValueTypeName = fullyQualifiedValueTypeName,
		};
	}
}

public record ApiClassProperty
{
	public required CollectionInfo CollectionInfo { get; init; } = null!;
	public required bool IsNullable { get; init; }
	public required string Name { get; init; } = null!;
	public required string TypeName { get; init; } = null!;
	public required string FullyQualifiedTypeName { get; init; } = null!;
}

public record ApiEnumValue
{
	public required string Name { get; init; } = null!;
	public required string? Value { get; init; } = null!;
}

public record ApiEndpoint
{
	public required List<EndpointParameter> Parameters { get; init; } = new();
	public required Dictionary<string, ApiType> RequestTypes { get; init; } = new();
	public required Dictionary<string, ApiType> ResponseTypes { get; init; } = new();
	public required string? BodyTypeName { get; init; } = null;
	public required string? QueryTypeName { get; init; } = null;
	public required string? PathTypeName { get; init; } = null;
	public required string ResponseTypeName { get; init; } = null!;
	public required string HttpMethod { get; init; } = null!;
	public required string RouteUrl { get; init; } = null!;
	public required string Version { get; init; } = null!;
	public required string ActionName { get; init; } = null!;
	public required bool FormData { get; init; }
}
