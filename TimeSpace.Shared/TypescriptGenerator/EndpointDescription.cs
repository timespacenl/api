namespace TimeSpace.Shared.TypescriptGenerator;

public record EndpointDescription
{
    public string RelativePath { get; init; } = null!;
    public string? HttpMethod { get; init; } = null!;
    public string? Version { get; init; } = null!;
    public string? ActionName { get; init; } = null!;
    public string ControllerTypeName { get; init; } = null!;
    public List<ParameterDescription> Parameters { get; init; } = new();
}