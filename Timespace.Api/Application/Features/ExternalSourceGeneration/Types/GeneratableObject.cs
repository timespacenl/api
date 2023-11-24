namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

public class GeneratableEndpoint
{
    public string HandlerName { get; set; } = null!;
    public string RouteUrl { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public string Version { get; set; } = null!;
    public GeneratableObject Request { get; set; } = null!;
    public GeneratableObject Response { get; set; } = null!;
}

public class GeneratableObject
{
    public string Name { get; set; } = null!;
    public Type? ObjectType { get; set; } = null!;
    public List<GeneratableMember> Members { get; } = new();
}

public record GeneratableMember
{
    public string Name { get; set; } = null!;
    public Type? MemberType { get; set; } = null!;
    public List<GeneratableMember> Members { get; } = new();
}
