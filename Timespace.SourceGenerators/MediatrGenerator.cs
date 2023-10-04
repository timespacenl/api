using System;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using Scriban;

namespace Timespace.SourceGenerators;

[Generator]
public class MediatrGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "GenerateMediatrAttribute.g.cs", 
            SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));
        
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
                var handlerDependenciesStaticCallArguments = string.Join(", ", generatable.Dependencies.Select(x => $"_{x.ParameterName}")) +
                                                             (generatable.Dependencies.Count > 0 ? "," : "");
                
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
        var node = context.TargetNode as ClassDeclarationSyntax ?? throw new Exception("Node is not a class: " + context.TargetNode);
        
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol ?? throw new Exception("Class symbol not found");
        
        var handleMethod = classSymbol.GetMembers("Handle").FirstOrDefault() as IMethodSymbol ?? throw new Exception("Handle method not found for class with name: " + classSymbol.Name);
        
        var dependencies = handleMethod.Parameters.Select(x =>
        {
            var type = x.Type as INamedTypeSymbol;
            
            if(type is null)
                return (x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), x.Name);
            
            var typename = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            
            return (typename, x.Name);
        }).ToList();
        
        dependencies.RemoveAt(0);
        dependencies.RemoveAt(dependencies.Count - 1);
        
        var requestTypeName = handleMethod.Parameters.FirstOrDefault()?.Type.Name;
        var responseTypeName = (handleMethod.ReturnType as INamedTypeSymbol)!.IsGenericType ? (handleMethod.ReturnType as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()?.Name : handleMethod.ReturnType.Name;
        
        if(requestTypeName is null || responseTypeName is null)
            return null;
        
        return new MediatrGeneratable(classSymbol.Name, classSymbol.ContainingNamespace.ToString(), dependencies, requestTypeName, responseTypeName);
    }
    
}