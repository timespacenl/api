using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Timespace.SourceGenerators.Tests;

public class MediatrGeneratorTests
{
    [Fact]
    public void GenerateMediatr_HappyPath()
    {
        const string sample = @"
            namespace Timespace.Api.Application.Features.Modules.Test.Queries;

            public record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

            [Timespace.SourceGenerators.GenerateMediatr]
            public static partial class WrapperClass
            {
                public partial record Command();

                public record Response();

                public static async Task<PaginatedResult<Response>> Handle(Command command, CancellationToken ct)
                {
                    return new Response();
                }
            }";
        
        Compilation inputCompilation = CreateCompilation(sample);
        
        // Create an instance of the source generator.
        var generator = new MediatrGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();
        
        Assert.True(runResult.GeneratedTrees.Length == 2);
        Assert.Empty(diagnostics);
    }
    
    [Fact]
    public void GenerateMediatr_NoHandleMethod()
    {
        const string sample = @"
            namespace Timespace.Api.Application.Features.Modules.Test.Queries;

            [Timespace.SourceGenerators.GenerateMediatr]
            public static partial class WrapperClass
            {
                public partial record Command();

                public record Response();
            }";
        
        Compilation inputCompilation = CreateCompilation(sample);
        
        // Create an instance of the source generator.
        var generator = new MediatrGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();
        
        Assert.True(runResult.GeneratedTrees.Length == 1);
        Assert.Empty(diagnostics);
    }
    
    [Fact]
    public void GenerateMediatr_NoCommand()
    {
        const string sample = @"
            namespace Timespace.Api.Application.Features.Modules.Test.Queries;

            [Timespace.SourceGenerators.GenerateMediatr]
            public static partial class WrapperClass
            {
                public record Response();
            }";
        
        Compilation inputCompilation = CreateCompilation(sample);
        
        // Create an instance of the source generator.
        var generator = new MediatrGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();
        
        Assert.True(runResult.GeneratedTrees.Length == 1);
        Assert.Empty(diagnostics);
    }
    
    private static Compilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}