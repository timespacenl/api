using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using Timespace.SourceGenerators.MediatrSourceGenerator;

namespace Timespace.SourceGenerators;

[Generator]
public class MediatrGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "Attributes.generator.cs", ThisAssembly.Resources.Attributes.Text));
        
        var nodes = context.SyntaxProvider.ForAttributeWithMetadataName("Timespace.SourceGenerators.GenerateMediatrAttribute",
            static (node, _) => node is ClassDeclarationSyntax,
            GetTypeToGenerate);
        
        context.RegisterSourceOutput(
            nodes,
            action: (spc, n) =>
            {
                if (n is null)
                    return;
                
                var generatable = n.Value;

                var handlerDependencies = string.Join("\n", generatable.Dependencies.Select(x => $"private readonly {x.TypeName} _{x.ParameterName};"));
                var handlerDependenciesConstructorArguments = string.Join(",\n\t\t\t\t\t   ", generatable.Dependencies.Select(x => $"{x.TypeName} {x.ParameterName}"));
                var handlerDependenciesConstructorAssignments = string.Join("\n", generatable.Dependencies.Select(x => $"_{x.ParameterName} = {x.ParameterName};"));
                var handlerDependenciesStaticCallArguments = string.Join(", ", generatable.Dependencies.Select(x => $"_{x.ParameterName}"));
                
                var source = ThisAssembly.Resources.MediatrGenerationTemplate.Text;
                var template = Template.ParseLiquid(source);
                var model = new
                {
                    generatable.Fqns,
                    generatable.WrapperClassName,
                    generatable.RequestTypeName,
                    generatable.ResponseTypeName,
                    generatable.HasDependencies,
                    handlerDependencies,
                    handlerDependenciesConstructorArguments,
                    handlerDependenciesConstructorAssignments,
                    handlerDependenciesStaticCallArguments,
                };
                
                var result = template.Render(model);
                
                spc.AddSource($"{generatable.WrapperClassName}.g.cs", SourceText.From(result, Encoding.UTF8));
            });
    }

    private static MediatrGeneratable? GetTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        var node = (ClassDeclarationSyntax)context.TargetNode;
        
        var classSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(node)!;
        
        var handleMethodMember = classSymbol.GetMembers("Handle").FirstOrDefault();
        if(handleMethodMember is not IMethodSymbol handleMethodSymbol)
            return null;
        
        var dependencies = handleMethodSymbol.Parameters
            .Select(x => (
                x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), 
                x.Name)
            )
            .ToList();
        
        // Remove the first and last parameters, which are the request and the cancellation token
        dependencies.RemoveAt(0);
        dependencies.RemoveAt(dependencies.Count - 1);
        
        var requestTypeName = handleMethodSymbol.Parameters.FirstOrDefault()?.Type.Name;
        
        if(requestTypeName is null)
            return null;

        string responseTypeName;
        
        if(handleMethodSymbol.ReturnType is INamedTypeSymbol returnTypeSymbol)
            responseTypeName =  returnTypeSymbol.TypeArguments.FirstOrDefault()?.Name ?? handleMethodSymbol.ReturnType.Name;
        else
            responseTypeName = handleMethodSymbol.ReturnType.Name;
        
        return new MediatrGeneratable(classSymbol.Name, classSymbol.ContainingNamespace.ToString(), dependencies, requestTypeName, responseTypeName);
    }
    
}