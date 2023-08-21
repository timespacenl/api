using Microsoft.CodeAnalysis.Operations;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Models;

public class ValidatorCollectionItem
{
    public string PropertyPath { get; set; } = null!;
    public List<IInvocationOperation> Validators { get; set; } = null!;
}