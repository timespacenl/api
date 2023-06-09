using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators;

public class PermissionsMappingGenerator : IExternalSourceGenerator
{
    private readonly Compilation _compilation;
    private readonly ILogger<PermissionsMappingGenerator> _logger;
    
    private TypescriptObjectBuilder _builder;
    
    public PermissionsMappingGenerator(Compilation compilation, ILogger<PermissionsMappingGenerator> logger)
    {
        _compilation = compilation;
        _logger = logger;
        _builder = new TypescriptObjectBuilder("permissions");
    }
    
    public void Execute()
    {
        var allNodes = _compilation.SyntaxTrees.SelectMany(x => x.GetRoot().DescendantNodes()).ToList();
        var permissionDeclaration = allNodes
            .Where(d => d.IsKind(SyntaxKind.ClassDeclaration))
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(x => x.Identifier.ToString() == "Permissions");
        
        if(permissionDeclaration == null)
            throw new Exception("Permissions class not found");
        
        var semanticModel = _compilation.GetSemanticModel(permissionDeclaration.SyntaxTree);
        var symbol = semanticModel.GetDeclaredSymbol(permissionDeclaration);

        ProcessNext(symbol!);
        
        _logger.LogDebug("Builder: \n{Builder}", _builder.Build());
        
        throw new NotImplementedException();
    }

    public void ProcessNext(ITypeSymbol symbol)
    {
        foreach (ISymbol member in symbol.GetMembers())
        {
            if (member is ITypeSymbol typeSymbol)
            {
                _builder.OpenScope(member.Name.ToCamelCase());
                if(typeSymbol.GetMembers().Length > 0)
                    ProcessNext(typeSymbol);
                
                _builder.CloseScope();
            }
            else
            {
                if (member is IFieldSymbol fieldSymbol)
                {
                    if(fieldSymbol.ConstantValue == null)
                        throw new Exception("Field value is null for " + fieldSymbol.ContainingNamespace + "." + fieldSymbol.ContainingType + "." + fieldSymbol.Name);
                    
                    _builder.AddProperty(fieldSymbol.Name.ToCamelCase(), fieldSymbol.ConstantValue.ToString()!.ToCamelCase());
                    _logger.LogDebug("Field: {Name} Value: {Value}", fieldSymbol.Name, fieldSymbol.ConstantValue);
                }
            }
        }
    }
}