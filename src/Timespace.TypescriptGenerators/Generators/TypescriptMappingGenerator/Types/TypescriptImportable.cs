namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

public record TypescriptImportable(ImportType ImportType, SharedType? ImportableType, ImportFlags? ImportFlags);

[Flags]
public enum ImportFlags
{
	Type = 1 << 0,
	Request = 1 << 1,
	Response = 1 << 2,
	RequestStringified = 1 << 3,
	ResponseStringified = 1 << 4,
}
