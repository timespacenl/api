namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

public record TypescriptSourceFragment(
	HashSet<TypescriptImportable> Imports,
	string TypeSource
	);
