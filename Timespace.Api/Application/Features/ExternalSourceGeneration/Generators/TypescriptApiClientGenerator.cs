using Microsoft.CodeAnalysis;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators;

public class TypescriptApiClientGenerator : IExternalSourceGenerator
{
    private readonly ILogger<TypescriptApiClientGenerator> _logger;

    public TypescriptApiClientGenerator(ILogger<TypescriptApiClientGenerator> logger)
    {
        _logger = logger;
    }

    public void Execute(Compilation compilation)
    {
        _logger.LogDebug("{CompilationAssemblyName}", compilation.AssemblyName);
    }
}