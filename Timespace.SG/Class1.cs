using System.Linq;
using Microsoft.CodeAnalysis;

namespace Timespace.SG;

[Generator]
public class TestSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // get all types from the compilation
        var types = context.Compilation.GetSymbolsWithName(x => true, SymbolFilter.Type).OfType<INamedTypeSymbol>();
        
        // get all types that end with "Exception"
        var exceptions = types.Where(x => x.Name.EndsWith("Exception"));
        
        
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        throw new System.NotImplementedException();
    }
}