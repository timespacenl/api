using Microsoft.CodeAnalysis;

namespace Timespace.Api.Infrastructure.ExternalSourceGeneration;

public interface IExternalSourceGenerator
{
    public void Execute(Compilation compilation);
}