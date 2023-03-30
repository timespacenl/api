// See https://aka.ms/new-console-template for more information

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Operations;
using Timespace.TypescriptGenerator;

MSBuildLocator.RegisterDefaults();

MSBuildWorkspace workspace = MSBuildWorkspace.Create();
var project = await workspace.OpenProjectAsync(@"I:\Timespace Repos\Timespace\Timespace.Api\Timespace.Api.csproj");
var compilation = await project.GetCompilationAsync();

if(compilation is null)
    throw new Exception("Compilation is null");

var allNodes = compilation.SyntaxTrees.SelectMany(x => x.GetRoot().DescendantNodes());

var syntaxNodes = allNodes.ToList();

IEnumerable<ClassDeclarationSyntax> allClasses = syntaxNodes.GetClassDeclarationsByAttribute("GenerateTsTypes");

foreach (var schemaClass in allClasses)
{
    GenerateRequestDto(schemaClass);
}

void GenerateRequestDto(ClassDeclarationSyntax schemaClass)
{
    var commandOrQuery = schemaClass.GetRecordDeclarationsByBaseList("IRequest<Response>").First();
    
    Console.WriteLine(schemaClass.Identifier + "" + commandOrQuery.Identifier);

    var semanticModel = compilation.GetSemanticModel(commandOrQuery.SyntaxTree);
    
    var validatorClass = schemaClass.GetClassDeclarationsByBaseList($"AbstractValidator<{commandOrQuery.Identifier}>").FirstOrDefault();
    if (validatorClass is null)
        throw new Exception($"Validator class not found for {schemaClass.Identifier}.{commandOrQuery.Identifier}");
    
    var ctor = (ConstructorDeclarationSyntax)validatorClass.Members.First(x => x.IsKind(SyntaxKind.ConstructorDeclaration));
    
    var ctorExpressions = ctor.ChildNodes()
        .OfType<BlockSyntax>()
        .SelectMany(x => x.ChildNodes()
            .OfType<ExpressionStatementSyntax>());
    
    foreach (var ctorChild in ctorExpressions)
    {
        IOperation? operation = null;
        
        InvocationExpressionSyntaxWalker walker = new();
        walker.Visit(ctorChild);

        foreach (var invocation in walker.Invocations.Reverse())
        {
            var children = invocation.ChildNodes();

            if (children.Any(x => x.GetText().ToString().Trim() == "RuleFor"))
            {
                operation = semanticModel.GetOperation(invocation) as IInvocationOperation;
                var operationWalker = new PropertyReferenceOperationWalker();
                operationWalker.Visit(operation);
        
                var referencedProperty = operationWalker.PropertyReferences.First();
        
                Console.WriteLine($"- Validator for: {referencedProperty.Property.Name} ({referencedProperty.Property.ContainingType})");
            }
            
            Console.WriteLine($"- - {invocation.Expression.GetText().ToString().Trim()}");
            foreach (var child in children)
            {
                if (child is MemberAccessExpressionSyntax member)
                {
                    Console.WriteLine($"- - - {member.Kind()}: {member.Name}");
                }
                else
                {
                    Console.WriteLine($"- - - {child.Kind()}: {child.GetText().ToString().Trim()}");
                }
            }
        }

        Console.WriteLine();
    }

    
    //TraverseDtoTree(schemaClass, commandOrQuery, semanticModel);
}

void TraverseDtoTree(ClassDeclarationSyntax baseClass, RecordDeclarationSyntax dto, SemanticModel semanticModel)
{
    var props = dto.Members.OfType<PropertyDeclarationSyntax>();
    
    foreach (var prop in props)
    {
        IPropertySymbol? symbol = ModelExtensions.GetDeclaredSymbol(semanticModel, prop) as IPropertySymbol;
        if(symbol is null)
            throw new Exception($"Symbol for property {prop.Identifier} is null");
        
        Console.WriteLine($"- {symbol.Type.Name} {prop.Identifier} (Nullable: {symbol.Type.NullableAnnotation})");
        
        if(Constants.ZodTypeMapping.ContainsKey(symbol.Type.Name))
        {
            
        }
        else
        {
            var deepProp = baseClass.GetRecordDeclarationsByIdentifier(prop.Type.ToString()).FirstOrDefault();
            if (deepProp is null)
                throw new Exception("Not found: " + symbol.Type.Name + " in namespace " + symbol.ContainingNamespace.ToDisplayString());
            
            TraverseDtoTree(baseClass, deepProp, semanticModel);
        }
    }
}