namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

public record SharedType(
    string TypeName,
    string RequestZodType,
    string RequestZodTypeStringified,
    string ResponseZodType,
    string ResponseZodTypeStringified,
    string ImportPath,
    bool IsEnum
    );
