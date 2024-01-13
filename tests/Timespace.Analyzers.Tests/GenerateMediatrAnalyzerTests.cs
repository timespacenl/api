using System.Threading.Tasks;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        Timespace.Analyzers.GenerateMediatrAnalyzer>;

namespace Timespace.Analyzers.Tests;

public class GenerateMediatrAnalyzerTests
{
    [Fact]
    public async Task ClassWithoutHandleMethod_AlertDiagnostic()
    {
        const string text = @"
[System.AttributeUsage(System.AttributeTargets.Class)]
public class GenerateMediatrAttribute : System.Attribute
{
}

[GenerateMediatr]
public static class WrapperClass
{
}
";

        var expected = Verifier.Diagnostic().WithSpan(8, 21, 8, 33).WithArguments("WrapperClass");
        await Verifier.VerifyAnalyzerAsync(text, expected).ConfigureAwait(false);
    }
    
    [Fact]
    public async Task ClassWithHandleMethod_NoAlertDiagnostic()
    {
        const string text = @"
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class IsExternalInit
    {
    }
}

[System.AttributeUsage(System.AttributeTargets.Class)]
public class GenerateMediatrAttribute : System.Attribute
{
}

[GenerateMediatr]
public static class WrapperClass
{
    public record Query;
    public record Response(string Test);

    private static async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        return new Response(""Test"");
    }
}
";
        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
    }
}