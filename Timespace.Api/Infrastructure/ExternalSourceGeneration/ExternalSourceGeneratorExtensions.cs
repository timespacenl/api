using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Options;
using Timespace.Api.Infrastructure.Configuration;

namespace Timespace.Api.Infrastructure.ExternalSourceGeneration;

public static class ExternalSourceGeneratorExtensions
{
    public static async Task RunExternalSourceGenerators(this WebApplication app)
    {
        var type = typeof(IExternalSourceGenerator);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && p is { IsClass: true, IsAbstract: false })
            .ToList();

        MSBuildWorkspace workspace = MSBuildWorkspace.Create();

        var options = app.Services.GetRequiredService<IOptions<ExternalSourceGenerationSettings>>();
        
        workspace.LoadMetadataForReferencedProjects = true;
        var project = await workspace.OpenProjectAsync(options.Value.ApiProjectPath);
        var compilation = await project.GetCompilationAsync();

        if (compilation is null)
            throw new NullReferenceException("Compilation is null");
        
        foreach (var externalGenerator in types)
        {
            var externalSourceGenerator =
                (IExternalSourceGenerator)ActivatorUtilities.CreateInstance(app.Services, externalGenerator, compilation);
            
            externalSourceGenerator.Execute();
        }
    }
}