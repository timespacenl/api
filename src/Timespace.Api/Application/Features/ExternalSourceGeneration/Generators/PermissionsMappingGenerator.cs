// using Microsoft.CodeAnalysis;
// using Microsoft.Extensions.Options;
// using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
// using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
//
// namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators;
//
// public class PermissionsMappingGenerator : IExternalSourceGenerator
// {
//     private readonly Compilation _compilation;
//     private readonly ILogger<PermissionsMappingGenerator> _logger;
//     private readonly ExternalSourceGenerationSettings _options;
//     
//     private readonly TypescriptObjectBuilder _builder;
//     
//     public PermissionsMappingGenerator(Compilation compilation, ILogger<PermissionsMappingGenerator> logger, IOptions<ExternalSourceGenerationSettings> settings)
//     {
//         _compilation = compilation;
//         _logger = logger;
//         _options = settings.Value;
//         _builder = new TypescriptObjectBuilder("permissions");
//     }
//     
//     public void Execute()
//     {
//         var allNodes = _compilation.SyntaxTrees.SelectMany(x => x.GetRoot().DescendantNodes()).ToList();
//         var permissionDeclaration = allNodes
//             .Where(d => d.IsKind(SyntaxKind.ClassDeclaration))
//             .OfType<ClassDeclarationSyntax>()
//             .FirstOrDefault(x => x.Identifier.ToString() == "Permissions");
//         
//         if(permissionDeclaration == null)
//             throw new Exception("Permissions class not found");
//         
//         var semanticModel = _compilation.GetSemanticModel(permissionDeclaration.SyntaxTree);
//         var symbol = semanticModel.GetDeclaredSymbol(permissionDeclaration);
//         
//         ProcessNext(symbol!);
//         
//         File.WriteAllText(_options.PermissionsGenerator.GenerationPath + '/' + _options.PermissionsGenerator.GenerationFileName + ".ts", _builder.Build());
//     }
//
//     public void ProcessNext(ITypeSymbol symbol)
//     {
//         foreach (ISymbol member in symbol.GetMembers())
//         {
//             if (member is ITypeSymbol typeSymbol)
//             {
//                 _builder.OpenScope(member.Name.ToCamelCase());
//                 if(typeSymbol.GetMembers().Length > 0)
//                     ProcessNext(typeSymbol);
//                 
//                 _builder.CloseScope();
//             }
//             else
//             {
//                 if (member is IFieldSymbol fieldSymbol)
//                 {
//                     if(fieldSymbol.ConstantValue == null)
//                         throw new Exception("Field value is null for " + fieldSymbol.ContainingNamespace + "." + fieldSymbol.ContainingType + "." + fieldSymbol.Name);
//                     
//                     _builder.AddProperty(fieldSymbol.Name.ToCamelCase(), fieldSymbol.ConstantValue.ToString()!.ToCamelCase());
//                 }
//             }
//         }
//     }
// }
