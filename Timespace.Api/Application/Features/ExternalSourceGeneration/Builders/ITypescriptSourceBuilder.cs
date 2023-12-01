using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public interface ITypescriptSourceBuilder
{
    public ITypescriptSourceBuilder Initialize(string name, bool canBeNull = false);
    public ITypescriptSourceBuilder AddProperty(GeneratableMember member, string? typeNameOverride = null);
    public string Build();
}
