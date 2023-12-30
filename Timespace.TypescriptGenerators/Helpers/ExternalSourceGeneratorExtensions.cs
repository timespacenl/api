using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;

namespace Timespace.TypescriptGenerators.Helpers;

public static class ExternalSourceGeneratorExtensions
{
    public static async Task RunSourceGenerators(this IHost host)
    {
        var type = typeof(IExternalSourceGenerator);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && p is { IsClass: true, IsAbstract: false })
            .ToList();

        MSBuildWorkspace workspace = MSBuildWorkspace.Create();

        var options = host.Services.GetRequiredService<IOptions<ExternalSourceGenerationSettings>>();
        
        workspace.LoadMetadataForReferencedProjects = true;
        var project = await workspace.OpenProjectAsync(options.Value.ApiProjectPath);
        var compilation = await project.GetCompilationAsync();

        if (compilation is null)
            throw new NullReferenceException("Compilation is null");

        var errorDiagnostics = compilation.GetDiagnostics().Where(x => x.WarningLevel == 0).ToList();
        if (errorDiagnostics.Any())
        {
            errorDiagnostics.ForEach(x => Console.WriteLine($"{x.GetMessage()} @ {x.Location.GetLineSpan().ToString()}"));
            throw new Exception("Compilation failed");
        }
        
        foreach (var externalGenerator in types)
        {
            var externalSourceGenerator =
                (IExternalSourceGenerator)ActivatorUtilities.CreateInstance(host.Services, externalGenerator, compilation);
            
            externalSourceGenerator.Execute();
        }
    }
}