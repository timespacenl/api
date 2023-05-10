using Microsoft.CodeAnalysis.MSBuild;

namespace Timespace.Api.Infrastructure.ExternalSourceGeneration;

public static class ExternalSourceGeneratorExtensions
{
    public async static Task RunExternalSourceGenerators(this WebApplication app)
    {
        var type = typeof(IExternalSourceGenerator);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && p is { IsClass: true, IsAbstract: false })
            .ToList();

        foreach (var externalGenerator in types)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync($@"{Environment.CurrentDirectory}\Timespace.Api.csproj");
            var compilation = await project.GetCompilationAsync();

            if (compilation is null)
                throw new NullReferenceException("Compilation is null");
            
            var externalSourceGenerator =
                (IExternalSourceGenerator)ActivatorUtilities.CreateInstance(app.Services, externalGenerator, compilation);
            
            externalSourceGenerator.Execute();
        }
    }
}