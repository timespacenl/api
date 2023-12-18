using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

public partial class TypescriptMappingGenerator : IExternalSourceGenerator
{
    private readonly Compilation _compilation;
    private readonly ILogger<TypescriptMappingGenerator> _logger;
    private readonly IReadOnlyList<EndpointDescription> _endpoints;
    
    public TypescriptMappingGenerator(Compilation compilation, ILogger<TypescriptMappingGenerator> logger)
    {
        _compilation = compilation;
        _logger = logger;
        _endpoints = JsonSerializer.Deserialize<List<EndpointDescription>>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "api-details.json"))) ?? [];
    }

    public void Execute()
    {
        foreach (var endpoint in _endpoints)
        {
            var generatableEndpoint = TransformEndpointDescription(endpoint);
        }
        _logger.LogInformation("TypescriptMappingGenerator executed");
    }
    
    private List<GeneratableMember> GetMembersFromParameters(List<ParameterDescription> parameters)
    {
        return new();
    }
}