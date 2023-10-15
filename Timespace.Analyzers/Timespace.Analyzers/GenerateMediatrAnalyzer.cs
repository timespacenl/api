using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Timespace.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GenerateMediatrAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "Usage";

    public const string HasHandleMethodDiagnosticId = "TSP001";
    private static readonly string HasHandleMethodDiagnosticTitle = "Type name '{0}' needs to have a private static Handle() method";
    private static readonly string HasHandleMethodDiagnosticMessageFormat = "Type name '{0}' needs to have a private static Handle() method";
    private static readonly string HasHandleMethodDiagnosticDescription = "Type name '{0}' needs to have a private static Handle() method.";
    
    private static readonly DiagnosticDescriptor HasHandleMethodDiagnostic = new(HasHandleMethodDiagnosticId, HasHandleMethodDiagnosticTitle, HasHandleMethodDiagnosticMessageFormat, Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: HasHandleMethodDiagnosticDescription);

    public const string HandleMethodHasCorrectArgumentsId = "TSP002";
    private static readonly string HandleMethodHasCorrectArgumentsTitle = "Handle method needs to have either a Command or Query as the first parameter and a CancellationToken as the last parameter";
    private static readonly string HandleMethodHasCorrectArgumentsMessageFormat = "Handle method needs to have either a Command or Query as the first parameter and a CancellationToken as the last parameter";
    private static readonly string HandleMethodHasCorrectArgumentsDescription = "Type Handle method needs to have either a Command or Query as the first parameter and a CancellationToken as the last parameter.";
    
    private static readonly DiagnosticDescriptor HandleMethodHasCorrectArgumentsDiagnostic = new(HandleMethodHasCorrectArgumentsId, HandleMethodHasCorrectArgumentsTitle, HandleMethodHasCorrectArgumentsMessageFormat, Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: HandleMethodHasCorrectArgumentsDescription);
    
    public const string WrapperClassDefinesCommandOrQueryId = "TSP003";
    private static readonly string WrapperClassDefinesCommandOrQueryTitle = "This class should define atleast a Command or Query record";
    private static readonly string WrapperClassDefinesCommandOrQueryMessageFormat = "Type name '{0}' should define atleast a Command or Query record";
    private static readonly string WrapperClassDefinesCommandOrQueryDescription = "This class should define atleast a Command or Query record.";
    
    private static readonly DiagnosticDescriptor WrapperClassDefinesCommandOrQueryDiagnostic = new(WrapperClassDefinesCommandOrQueryId, WrapperClassDefinesCommandOrQueryTitle, WrapperClassDefinesCommandOrQueryMessageFormat, Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true, description: WrapperClassDefinesCommandOrQueryDescription);
    
    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.CreateRange(new[] {HasHandleMethodDiagnostic, HandleMethodHasCorrectArgumentsDiagnostic, WrapperClassDefinesCommandOrQueryDiagnostic});

    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();

        // Subscribe to the Syntax Node with the appropriate 'SyntaxKind' (ClassDeclaration) action.
        // To figure out which Syntax Nodes you should choose, consider installing the Roslyn syntax tree viewer plugin Rossynt: https://plugins.jetbrains.com/plugin/16902-rossynt/
        context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.ClassDeclaration);

        // Check other 'context.Register...' methods that might be helpful for your purposes.
    }
    
    private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationNode)
            return;

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationNode);
        
        if(symbol is null)
            return;
        
        // Check if the class has the GenerateMediatrAttribute
        var attribute = symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name == "GenerateMediatrAttribute");
        
        if(attribute is null)
            return;

        var members = symbol.GetMembers().Where(x => x.Name is "Command" or "Query");

        if (members.Count() != 1)
            context.ReportDiagnostic(Diagnostic.Create(WrapperClassDefinesCommandOrQueryDiagnostic,
                classDeclarationNode.Identifier.GetLocation(), classDeclarationNode.Identifier.ToString()));
        
        // Check if the class name contains a handle method
        var handleMethod = symbol.GetMembers("Handle").FirstOrDefault();
        
        if(handleMethod is null)
            context.ReportDiagnostic(Diagnostic.Create(HasHandleMethodDiagnostic, classDeclarationNode.Identifier.GetLocation(), classDeclarationNode.Identifier.ToString()));

        if (handleMethod is IMethodSymbol handleMethodSymbol)
        {
            var parameters = handleMethodSymbol.Parameters.Select(x => x.Type).ToList();
            
            if(parameters.Count < 2)
                context.ReportDiagnostic(Diagnostic.Create(HandleMethodHasCorrectArgumentsDiagnostic, handleMethodSymbol.Locations.First(), handleMethodSymbol.ToString()));

            if (parameters.Last().Name != "CancellationToken")
                context.ReportDiagnostic(Diagnostic.Create(HandleMethodHasCorrectArgumentsDiagnostic, handleMethodSymbol.Locations.First(), handleMethodSymbol.ToString()));

            var firstParam = parameters.First().Name.Split('+').Last();
            
            if (firstParam != "Query" && firstParam != "Command")
                context.ReportDiagnostic(Diagnostic.Create(HandleMethodHasCorrectArgumentsDiagnostic, handleMethodSymbol.Locations.First(), handleMethodSymbol.ToString()));
        }
    }
}