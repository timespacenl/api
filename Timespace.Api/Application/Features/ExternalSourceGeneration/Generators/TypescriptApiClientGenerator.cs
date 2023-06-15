using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Models;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Walkers;
using Timespace.Api.Infrastructure.ExternalSourceGeneration;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators;

public class TypescriptApiClientGenerator : IExternalSourceGenerator
{
    private readonly ILogger<TypescriptApiClientGenerator> _logger;
    private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;
    private readonly Compilation _compilation;
    
    private StringBuilder _generation = new();
    private List<ValidatorCollectionItem> _validators = new();
    
    public TypescriptApiClientGenerator(ILogger<TypescriptApiClientGenerator> logger, IApiDescriptionGroupCollectionProvider apiExplorer, Compilation compilation)
    {
        _logger = logger;
        _apiExplorer = apiExplorer;
        _compilation = compilation;
    }

    public void Execute()
    {
        var apiDescriptionGroups = _apiExplorer.ApiDescriptionGroups.Items;
        foreach (var apiDescriptionGroup in apiDescriptionGroups)
        {
            ProcessApiDescriptionGroup(apiDescriptionGroup.Items, apiDescriptionGroup.GroupName);
        }
    }

    private void ProcessApiDescriptionGroup(IReadOnlyList<ApiDescription> apiDescriptions, string? groupName)
    {
        foreach (var apiDescription in apiDescriptions)
        {
            List<ApiParameterDescription> pathParameters = new();
            List<ApiParameterDescription> bodyParameters = new();
            List<ApiParameterDescription> queryParameters = new();
            var currentApiUrl = apiDescription.RelativePath ?? throw new Exception("Path is null for apiDescription");

            foreach (var parameterDescription in apiDescription.ParameterDescriptions)
            {
                if (parameterDescription.BindingInfo?.BindingSource == BindingSource.Path)
                    pathParameters.Add(parameterDescription);
                
                if(parameterDescription.BindingInfo?.BindingSource == BindingSource.Body)
                    bodyParameters.Add(parameterDescription);
                
                if(parameterDescription.BindingInfo?.BindingSource == BindingSource.Query)
                    queryParameters.Add(parameterDescription);
            }

            ProcessApiDescription(pathParameters, bodyParameters, queryParameters, currentApiUrl);
        }
    }

    private void ProcessApiDescription(
        IReadOnlyList<ApiParameterDescription> pathParameters,
        IReadOnlyList<ApiParameterDescription> bodyParameters,
        IReadOnlyList<ApiParameterDescription> queryParameters,
        string currentApiUrl
        )
    {
        var formattedUrl = currentApiUrl;
        
        if(pathParameters.Count > 0)
            formattedUrl = ProcessPathParameters(formattedUrl, pathParameters);
        
        if(queryParameters.Count > 0)
            formattedUrl = ProcessQueryParameters(formattedUrl, queryParameters);

        if (bodyParameters.Count > 0)
        {
            ProcessBodyParameters(bodyParameters);
            
            var baseClassFullName = bodyParameters.First().Type.FullName!.Split("+").First();
            var baseClassSymbol = _compilation.GetTypeByMetadataName(baseClassFullName);

            var generation = GenerateSchemaFromRequestWithValidation(baseClassSymbol!.Name, baseClassSymbol);
        }
        
        
        _logger.LogDebug("Formatted url: {Url}", formattedUrl);
    }

    private string GenerateSchemaFromRequestWithValidation(string baseClassName, INamedTypeSymbol baseClass)
    {
        var requestClass = baseClass
            .GetMembers()
            .OfType<INamedTypeSymbol>()
            .First(x => x.AllInterfaces.Any(y => y.Name == "IRequest"));
        
        var builder = new ZodSchemaBuilder((baseClassName + requestClass.Name + "Schema").ToCamelCase());

        ProcessDtoRecord(requestClass, "", builder, baseClass);

        _logger.LogDebug("ZObject: {ZObject}", builder.Build());
        
        return "";
    }

    private void ProcessDtoRecord(INamedTypeSymbol record, string currentPropertyPath, ZodSchemaBuilder builder, INamedTypeSymbol baseClass)
    {
        foreach (var propertySymbol in record.GetMembers().OfType<IPropertySymbol>())
        {
            if (propertySymbol.Name == "EqualityContract")
                continue;

            if(currentPropertyPath == "")
                currentPropertyPath = propertySymbol.Name;
            else
                currentPropertyPath += "." + propertySymbol.Name;

            _logger.LogDebug("Current property path: {PropertyPath}", currentPropertyPath);
                
            if (Constants.ZodTypeMapping.TryGetValue(propertySymbol.Type.Name, out var value))
            {
                builder.OpenZPropertyScope(propertySymbol.Name.ToCamelCase(), value);

                builder = ProcessValidators(builder, currentPropertyPath);
                
                builder.CloseZPropertyScope();
            }
            else
            {
                var deepProp = baseClass.GetMembers().OfType<INamedTypeSymbol>().FirstOrDefault(x => x.Name == propertySymbol.Type.Name);
                if (deepProp is null)
                    throw new Exception($"Cannot find property {propertySymbol.Type.Name} in {baseClass.Name}");

                builder.OpenZObjectScope(propertySymbol.Name.ToCamelCase());
                    
                ProcessDtoRecord(deepProp, currentPropertyPath, builder, baseClass);

                builder.CloseZObjectScope();
            }
                
            if(currentPropertyPath.Contains("." + propertySymbol.Name))
                currentPropertyPath = currentPropertyPath.Replace("." + propertySymbol.Name, "");
            else
                currentPropertyPath = currentPropertyPath.Replace(propertySymbol.Name, "");
        }
    }

    private ZodSchemaBuilder ProcessValidators(ZodSchemaBuilder builder, string currentPropertyPath)
    {
        var validationItem = _validators.FirstOrDefault(x => x.PropertyPath == currentPropertyPath);

        if (validationItem is not null)
        {
            foreach (var validator in validationItem.Validators)
            {
                if (Constants.ZodFluentValidationValidatorMapping.TryGetValue(validator.TargetMethod.Name,
                        out var value))
                {
                    if (value.Mapping != "")
                    {
                        var list = validator.Arguments
                            .Where(x => x.ArgumentKind == ArgumentKind.Explicit)
                            .ToList();
                        
                        var parameters = value.HasParameters ? string.Join(", ", validator.Arguments
                            .Where(x => x.ArgumentKind == ArgumentKind.Explicit)
                            .Select(x => x.Value.ConstantValue).ToList()) : "";
                        
                        builder.WithValidator(value.Mapping, parameters);
                    }
                }
                
                _logger.LogDebug("Validator: {Validator}", validator.TargetMethod.Name);
            }
        }

        return builder;
    }
    
    private void ProcessBodyParameters(IReadOnlyList<ApiParameterDescription> bodyParameters)
    {
        foreach (var parameter in bodyParameters)
        {
            var baseClassFullName = parameter.Type.FullName!.Split("+").First();
            var baseClassSymbol = _compilation.GetTypeByMetadataName(baseClassFullName);

            if (baseClassSymbol == null)
                throw new Exception($"Cannot find symbol for ${baseClassFullName}");

            var validatorClassSymbol = baseClassSymbol.GetMembers().OfType<INamedTypeSymbol>().FirstOrDefault(x => x.BaseType?.Name == "AbstractValidator");

            if(validatorClassSymbol == null)
                throw new Exception($"Cannot find validator class for ${baseClassFullName}");
            
            var ctor = validatorClassSymbol.Constructors.First();
            var syntax = ctor.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).First() as ConstructorDeclarationSyntax;

            var semanticModel = _compilation.GetSemanticModel(syntax!.SyntaxTree);
            var operation = semanticModel.GetOperation(syntax.Body as SyntaxNode ?? syntax.ExpressionBody ?? throw new Exception("No constructor body"))!;

            foreach (var childOperation in operation.ChildOperations)
            {
                var walker = new InvocationOperationWalker();
                walker.Visit(childOperation);
                
                var invocations = walker.InvocationOperations;

                ProcessRuleForInvocations(Enumerable.Reverse(invocations).ToList());
            }
            
            _logger.LogDebug("Body parameter: {Name}", parameter.Type.FullName);
        }
    }

    private void ProcessRuleForInvocations(List<IInvocationOperation> invocations)
    {
        foreach (var invocation in invocations)
        {
            if (invocation.TargetMethod.MetadataName == "RuleFor")
            {
                var operationWalker = new PropertyReferenceOperationWalker();
                operationWalker.Visit(invocation);
        
                var completeProp = operationWalker.PropertyReferences.Select(x => x.Property.Name).Reverse().Aggregate((x, y) => x + "." + y);
                
                _validators.Add(new ValidatorCollectionItem
                {
                    PropertyPath = completeProp,
                    Validators = invocations
                        .Where(x => x?.TargetMethod.MetadataName != "RuleFor")
                        .ToList()
                });
            }
        }
    }

    private string ProcessPathParameters(string currentUrl, IReadOnlyList<ApiParameterDescription> pathParameters)
    {
        foreach (var parameter in pathParameters)
        {
            currentUrl = currentUrl.Replace($"{{{parameter.Name}}}", "${" + parameter.Name.ToCamelCase() + "}");   
        }

        return currentUrl;
    }
    
    private string ProcessQueryParameters(string currentUrl, IReadOnlyList<ApiParameterDescription> queryParameters)
    {
        var queryBuilder = new UrlQueryParamBuilder();
        
        foreach (var parameter in queryParameters)
        {
            queryBuilder.Add(parameter.Name.ToCamelCase(), $"${{{parameter.Name.ToCamelCase()}}}");
        }

        return currentUrl + queryBuilder;
    }
}