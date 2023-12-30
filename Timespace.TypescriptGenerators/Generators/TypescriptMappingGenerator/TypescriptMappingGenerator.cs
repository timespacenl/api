using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

public partial class TypescriptMappingGenerator : IExternalSourceGenerator
{
    private readonly Compilation _compilation;
    private readonly ILogger<TypescriptMappingGenerator> _logger;
    private readonly IReadOnlyList<EndpointDescription> _endpoints;
    private readonly ExternalSourceGenerationSettings _settings;
    
    public TypescriptMappingGenerator(Compilation compilation, ILogger<TypescriptMappingGenerator> logger, IOptions<ExternalSourceGenerationSettings> settings, IReadOnlyList<EndpointDescription>? endpoints = null)
    {
        _compilation = compilation;
        _logger = logger;
        _endpoints = endpoints ?? JsonSerializer.Deserialize<List<EndpointDescription>>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "api-details.json"))) ?? [];
        _settings = settings.Value;
    }

    public void Execute()
    {
        var endpoints = new List<ApiEndpoint>();
        foreach (var endpoint in _endpoints)
        {
            var generatableEndpoint = TransformEndpointDescription(endpoint);
            
            if(generatableEndpoint == null)
                _logger.LogWarning($"Endpoint {endpoint.ControllerTypeName}.{endpoint.ActionName} was not generated because something went wrong during the discovery process.");
            else
                endpoints.Add(generatableEndpoint);
        }

        var typescriptSourceFiles = GenerateTypescriptCode(endpoints);

        if(Directory.Exists(_settings.TypescriptGenerator.GenerationRoot))
            Directory.Delete(_settings.TypescriptGenerator.GenerationRoot, true);
        
        foreach (var sourceFile in typescriptSourceFiles)
        {
            Directory.CreateDirectory(Path.Combine(_settings.TypescriptGenerator.GenerationRoot, sourceFile.DirectoryPath));
            File.WriteAllText(Path.Combine(_settings.TypescriptGenerator.GenerationRoot, sourceFile.DirectoryPath, sourceFile.FileName), sourceFile.Content);
        }
        
        _logger.LogInformation("TypescriptMappingGenerator executed");
    }
}