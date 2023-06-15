using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;

public static class SyntaxNodeExtensions
{
    private static IEnumerable<RecordDeclarationSyntax> FindRecordDeclarationByIdentifier(IEnumerable<SyntaxNode> nodes, string identifier)
    {
        return nodes
            .Where(d => d.IsKind(SyntaxKind.RecordDeclaration))
            .OfType<RecordDeclarationSyntax>()
            .Where(x => x.Identifier.ToString() == identifier);
    }

    private static IEnumerable<RecordDeclarationSyntax> FindRecordDeclarationByAttribute(IEnumerable<SyntaxNode> nodes, string attributeName)
    {
        return nodes
            .Where(d => d.IsKind(SyntaxKind.RecordDeclaration))
            .OfType<RecordDeclarationSyntax>()
            .Where(x =>
                x.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes)
                    .Where(attr => attr.Name.ToString() == attributeName).ToList().Count > 0);
    }
    
    private static IEnumerable<RecordDeclarationSyntax> FindRecordDeclarationByBaseList(IEnumerable<SyntaxNode> nodes, string baseClass)
    {
        return nodes
            .Where(d => d.IsKind(SyntaxKind.RecordDeclaration))
            .OfType<RecordDeclarationSyntax>()
            .Where(x => x.BaseList!.Types.Any(y => y.ToString() == baseClass));
    }
    
    private static IEnumerable<ClassDeclarationSyntax> FindClassDeclarationByIdentifier(IEnumerable<SyntaxNode> nodes, string identifier)
    {
        return nodes
            .Where(d => d.IsKind(SyntaxKind.ClassDeclaration))
            .OfType<ClassDeclarationSyntax>()
            .Where(x => x.Identifier.ToString() == identifier);
    }

    private static IEnumerable<ClassDeclarationSyntax> FindClassDeclarationByAttribute(IEnumerable<SyntaxNode> nodes, string attributeName)
    {
        return nodes
            .Where(d => d.IsKind(SyntaxKind.ClassDeclaration))
            .OfType<ClassDeclarationSyntax>()
            .Where(x =>
                x.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes)
                    .Where(attr => attr.Name.ToString() == attributeName).ToList().Count > 0);
    }
    
    private static IEnumerable<ClassDeclarationSyntax> FindClassDeclarationByBaseList(IEnumerable<SyntaxNode> nodes, string baseClass)
    {
        return nodes
            .Where(d => d.IsKind(SyntaxKind.ClassDeclaration))
                .OfType<ClassDeclarationSyntax>()
                .Where(x => x.BaseList!.Types.Any(y => y.ToString() == baseClass));
    }
    
    private static IEnumerable<ClassDeclarationSyntax> FindClassDeclarationByTypeName(IEnumerable<SyntaxNode> nodes, string typeName)
    {
        return nodes
            .Where(d => d.IsKind(SyntaxKind.ClassDeclaration))
            .OfType<ClassDeclarationSyntax>()
            .Where(x => x.GetType().FullName == typeName);
    }
    
    // Record declaration extension methods
    public static IEnumerable<RecordDeclarationSyntax> GetRecordDeclarationsByAttribute(this SyntaxNode node, string attributeName) => FindRecordDeclarationByAttribute(node.DescendantNodes(), attributeName);
    public static IEnumerable<RecordDeclarationSyntax> GetRecordDeclarationsByAttribute(this IEnumerable<SyntaxNode> nodes, string attributeName) => FindRecordDeclarationByAttribute(nodes, attributeName);
    
    public static IEnumerable<RecordDeclarationSyntax> GetRecordDeclarationsByBaseList(this SyntaxNode node, string attributeName) => FindRecordDeclarationByBaseList(node.DescendantNodes(), attributeName);
    public static IEnumerable<RecordDeclarationSyntax> GetRecordDeclarationsByBaseList(this IEnumerable<SyntaxNode> nodes, string attributeName) => FindRecordDeclarationByBaseList(nodes, attributeName);
    
    public static IEnumerable<RecordDeclarationSyntax> GetRecordDeclarationsByIdentifier(this SyntaxNode node, string attributeName) => FindRecordDeclarationByIdentifier(node.DescendantNodes(), attributeName);
    public static IEnumerable<RecordDeclarationSyntax> GetRecordDeclarationsByIdentifier(this IEnumerable<SyntaxNode> nodes, string attributeName) => FindRecordDeclarationByIdentifier(nodes, attributeName);
    
    // Class declaration extension methods
    public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarationsByAttribute(this SyntaxNode node, string attributeName) => FindClassDeclarationByAttribute(node.DescendantNodes(), attributeName);
    public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarationsByAttribute(this IEnumerable<SyntaxNode> nodes, string attributeName) => FindClassDeclarationByAttribute(nodes, attributeName);
    
    public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarationsByBaseList(this IEnumerable<SyntaxNode> nodes, string baseClass) => FindClassDeclarationByBaseList(nodes, baseClass);
    public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarationsByBaseList(this SyntaxNode node, string baseClass) => FindClassDeclarationByBaseList(node.DescendantNodes(), baseClass);
}