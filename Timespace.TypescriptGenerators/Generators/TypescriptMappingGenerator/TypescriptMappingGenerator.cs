using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;
using TimeSpace.Shared.TypescriptGenerator;
using Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator.Types;

namespace Timespace.TypescriptGenerators.Generators.TypescriptMappingGenerator;

public class TypescriptMappingGenerator : IExternalSourceGenerator
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

    public GeneratableEndpoint? TransformEndpointDescription(EndpointDescription description)
    {
        var allNodes = _compilation.SyntaxTrees.SelectMany(x => x.GetRoot().DescendantNodes()).ToList();

        var declaringType = _compilation.GetTypeByMetadataName(description.ControllerTypeName);

        if (declaringType is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a parent controller type that could be found", description.RelativePath);
            return null;
        }
        
        var actionSym = declaringType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(x => x.Name == description.ActionName);
        var actionParams = actionSym.Parameters.Select(x =>
        {
            return new
            {
                name = x.Name,
                attrs = x.GetAttributes()
            };
        });
        
        var action = allNodes
            .Where(d => d.IsKind(SyntaxKind.MethodDeclaration))
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(x => x.Identifier.ToString() == description.ActionName);

        if (action is null)
        {
            _logger.LogError("API endpoint with route url {RouteUrl} does not have a method declaration that could be found", description.RelativePath);
            return null;
        }
        
        var actionSymbol = ModelExtensions.GetDeclaredSymbol(_compilation.GetSemanticModel(action.SyntaxTree), action);

        return null;
    }
}