namespace Timespace.Api.Infrastructure.Configuration;

public class ExternalSourceGenerationSettings
{
    public const string SectionName = "ExternalSourceGenerationSettings";

    public string ApiProjectPath { get; set; } = null!;
    public TypescriptGeneratorSettings TypescriptGenerator { get; set; } = null!;
    public PermissionsGenerator PermissionsGenerator { get; set; } = null!;

}

public class TypescriptGeneratorSettings
{
    public string GenerationPath { get; set; } = null!;
    public string GenerationFileName { get; set; } = null!;
}

public class PermissionsGenerator
{
    public string GenerationPath { get; set; } = null!;
    public string GenerationFileName { get; set; } = null!;
}