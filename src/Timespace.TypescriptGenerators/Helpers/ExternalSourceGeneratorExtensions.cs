using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Timespace.TypescriptGenerators.Helpers;

internal static class ExternalSourceGeneratorExtensions
{
	[SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "TODO: Replace with custom exception type")]
	public static async Task RunSourceGenerators(this IHost host)
	{
		var type = typeof(IExternalSourceGenerator);
		var types = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(s => s.GetTypes())
			.Where(p => type.IsAssignableFrom(p) && p is { IsClass: true, IsAbstract: false })
			.ToList();

		using var workspace = MSBuildWorkspace.Create();

		var options = host.Services.GetRequiredService<IOptions<ExternalSourceGenerationSettings>>();

		workspace.LoadMetadataForReferencedProjects = true;
		var project = await workspace.OpenProjectAsync(options.Value.ApiProjectPath);
		var compilation = await project.GetCompilationAsync() ?? throw new ArgumentNullException(nameof(host));

		var errorDiagnostics = compilation.GetDiagnostics().Where(x => x.WarningLevel == 0).ToList();
		if (errorDiagnostics.Count != 0)
		{
			errorDiagnostics.ForEach(x => Console.WriteLine($"{x.GetMessage()} @ {x.Location.GetLineSpan()}"));
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
