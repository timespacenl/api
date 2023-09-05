using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Models;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Walkers;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator;

public partial class TypescriptApiClientGenerator
{
    private List<EndpointInfo> GetEndpointInfoForApiDescriptionGroup(IReadOnlyList<ApiDescription> apiDescriptions, string? groupName)
    {
        var endpointDescriptions = new List<EndpointInfo>();
        
        foreach (var apiDescription in apiDescriptions)
        {
            var currentEndpointInfo = new EndpointInfo()
            {
                GroupName = apiDescription.GroupName ?? "",
                HttpMethod = apiDescription.HttpMethod ?? "",
                Url = apiDescription.RelativePath ?? throw new Exception("Path is null for apiDescription"),
                Parameters = apiDescription.ParameterDescriptions.ToList(),
                OriginalApiDescription = apiDescription
            };

            var responseSymbolName = apiDescription.SupportedResponseTypes.FirstOrDefault();
            if (responseSymbolName is null)
            {
                _logger.LogError("Response type is null for url: {Url}", apiDescription.RelativePath);
                continue;
            }
            
            var apiHandlerClass = _compilation.GetTypeByMetadataName(responseSymbolName.Type?.FullName?.Split('+').First()!);
            currentEndpointInfo.HandlerClassSymbol = apiHandlerClass ?? throw new Exception("Handler class is null for url: " + apiDescription.RelativePath);

            var requestClass = apiHandlerClass.GetTypeMembers().FirstOrDefault(x => x.Name is "Command" or "Query");
            currentEndpointInfo.RequestSymbol = requestClass ?? throw new Exception("Request class is null for url: " + apiDescription.RelativePath);

            var requestObjectInfo = ParseObjectInfoWithValidators(apiHandlerClass, requestClass, true);
            currentEndpointInfo.Request = requestObjectInfo;
            
            var responseClass = _compilation.GetTypeByMetadataName(responseSymbolName.Type?.FullName!);
            currentEndpointInfo.ResponseSymbol = responseClass ?? throw new Exception("Response class is null for url: " + apiDescription.RelativePath);

            var responseObjectInfo = ParseObjectInfoWithValidators(apiHandlerClass, responseClass);
            currentEndpointInfo.Response = responseObjectInfo;
            
            endpointDescriptions.Add(currentEndpointInfo);
        }

        return endpointDescriptions;
    }

    private ObjectInfo ParseObjectInfoWithValidators(INamedTypeSymbol baseClass, INamedTypeSymbol parseableObject, bool hasValidators = false)
    {
        var validators = new List<ValidatorCollectionItem>();
        
        var objectInfo = new ObjectInfo()
        {
            Name = parseableObject.Name,
            OriginalSymbol = parseableObject,
        };

        if (hasValidators)
            validators = GetValidatorsFromAbstractValidator(baseClass);
        
        objectInfo.Members = GetMemberInfosFromObject(parseableObject, validators);
        
        return objectInfo;
    }

    private List<ValidatorCollectionItem> GetValidatorsFromAbstractValidator(INamedTypeSymbol baseClass)
    {
        var validators = new List<ValidatorCollectionItem>();
        var validatorClass = baseClass.GetTypeMembers().FirstOrDefault(x => x.BaseType?.Name == "AbstractValidator");
            
        if(validatorClass == null)
            throw new Exception($"Cannot find validator class for ${baseClass.Name}");
            
        var ctor = validatorClass.Constructors.First();
        var syntax = ctor.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).First() as ConstructorDeclarationSyntax;

        var semanticModel = _compilation.GetSemanticModel(syntax!.SyntaxTree);
        var operation = semanticModel.GetOperation(syntax.Body as SyntaxNode ?? syntax.ExpressionBody ?? throw new Exception("No constructor body"))!;

        foreach (var childOperation in operation.ChildOperations)
        {
            var validator = ProcessValidatorOperation(childOperation);
            validators.Add(validator);
        }

        return validators;
    }

    private ValidatorCollectionItem ProcessValidatorOperation(IOperation operation)
    {
        var walker = new InvocationOperationWalker();
        walker.Visit(operation);
                
        var invocations = walker.InvocationOperations;
        
        foreach (var invocation in invocations)
        {
            if (invocation.TargetMethod.MetadataName == "RuleFor")
            {
                var operationWalker = new PropertyReferenceOperationWalker();
                operationWalker.Visit(invocation);
        
                var completeProp = operationWalker.PropertyReferences.Select(x => x.Property.Name).Reverse().Aggregate((x, y) => x + "." + y);
                
                return new ValidatorCollectionItem
                {
                    PropertyPath = completeProp,
                    Validators = invocations
                        .Where(x => x?.TargetMethod.MetadataName != "RuleFor")
                        .ToList()
                };
            }
        }

        throw new Exception("No RuleFor invocation found in operation");
    }
    
    private List<MemberInfo> GetMemberInfosFromObject(INamedTypeSymbol parseableObject, List<ValidatorCollectionItem> validators, string propertyPath = "")
    {
        var memberInfos = new List<MemberInfo>();
        var currentPropertyPath = propertyPath;
        var members = parseableObject.GetMembers().OfType<IPropertySymbol>();

        foreach (var member in members)
        {
            if(member.Name == "EqualityContract")
                continue;
            
            var isList = member.Type.AllInterfaces.Any(x => x.Name == "IList");
            var propertyType = isList ? (member.Type as INamedTypeSymbol)?.TypeArguments.First()! : member.Type;
            
            var memberInfo = new MemberInfo()
            {
                Name = member.Name,
                Type = propertyType,
                IsList = isList,
                OriginalSymbol = member,
            };
            
            if(currentPropertyPath == "")
                currentPropertyPath = member.Name;
            else
                currentPropertyPath += "." + member.Name;
            
            var validator = validators.FirstOrDefault(x => x.PropertyPath == currentPropertyPath);
            if (validator != null)
            {
                var fluentValidators = GetFluentValidatorsFromValidationCollectionItem(validator);
                memberInfo.FluentValidators = fluentValidators;
            }
            
            if (propertyType is INamedTypeSymbol { TypeKind: TypeKind.Class } namedTypeSymbol)
            {
                var primitives = Constants.ZodTypeMapping.Keys.ToList();
                if (!primitives.Contains(namedTypeSymbol.Name))
                {
                    var children = GetMemberInfosFromObject(namedTypeSymbol, validators, currentPropertyPath);
                    memberInfo.Children = children;
                }
            }
            
            if(currentPropertyPath.Contains("." + member.Name))
                currentPropertyPath = currentPropertyPath.Replace("." + member.Name, "");
            else
                currentPropertyPath = currentPropertyPath.Replace(member.Name, "");
            
            memberInfos.Add(memberInfo);
        }
        
        return memberInfos;
    }

    private List<FluentValidator> GetFluentValidatorsFromValidationCollectionItem(ValidatorCollectionItem validatorCollectionItem)
    {
        var validators = new List<FluentValidator>();
        foreach (var validatorOperation in validatorCollectionItem.Validators)
        {
            var fluentValidator = new FluentValidator()
            {
                ValidatorName = validatorOperation.TargetMethod.MetadataName,
                Parameters = validatorOperation.Arguments.ToList()
                    .Where(x => x is { ArgumentKind: ArgumentKind.Explicit, Value.ConstantValue.HasValue: true })
                    .Select(x => new FluentValidatorParameter()
                        {
                            Name = x.Parameter?.MetadataName ?? "",
                            Type = x.Type!,
                            Value = x.Value.ConstantValue.ToString() ?? ""
                        })
                    .ToList(),
                OriginalInvocation = validatorOperation
            };
            
            validators.Add(fluentValidator);
        }
        
        return validators;
    }
}