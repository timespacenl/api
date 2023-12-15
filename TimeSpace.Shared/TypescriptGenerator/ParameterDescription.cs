namespace TimeSpace.Shared.TypescriptGenerator;

public record ParameterDescription
{
    public string Name { get; init; } = null!;
    public string? Type { get; init; } = null!;
    public string? DeclaringType { get; init; } = null!;
    public ParameterSource Source { get; init; }
}

public enum ParameterSource
{
    Query,
    Path,
    Body,
    Form
}