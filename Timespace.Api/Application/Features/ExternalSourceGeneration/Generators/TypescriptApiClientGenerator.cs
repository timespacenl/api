using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.CodeAnalysis;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators;

public class TypescriptApiClientGenerator : IExternalSourceGenerator
{
    private readonly ILogger<TypescriptApiClientGenerator> _logger;
    private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
    private readonly Compilation _compilation;

    public TypescriptApiClientGenerator(ILogger<TypescriptApiClientGenerator> logger, IApiDescriptionGroupCollectionProvider apiExplorer, Compilation compilation)
    {
        _logger = logger;
        _apiExplorer = apiExplorer;
        _compilation = compilation;
    }

    public void Execute()
    {
        var apiDescriptionGroups = _apiExplorer.ApiDescriptionGroups.Items;
        foreach (var apiDescriptionGroup in apiDescriptionGroups)
        {
            ProcessApiDescriptionGroup(apiDescriptionGroup.Items, apiDescriptionGroup.GroupName);
        }

        _logger.LogDebug("{CompilationAssemblyName}", _compilation.AssemblyName);
    }

    private void ProcessApiDescriptionGroup(IReadOnlyList<ApiDescription> apiDescriptions, string? groupName)
    {
        foreach (var apiDescription in apiDescriptions)
        {
            _logger.LogDebug(apiDescription.RelativePath);
        }
    }
}