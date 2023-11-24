using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public interface ITypescriptSourceBuilder
{
    public ITypescriptSourceBuilder Initialize(string name);
    public ITypescriptSourceBuilder AddProperty(string name, string type, bool isNullable, bool isList);
    public string Build();
}
