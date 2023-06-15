using Microsoft.CodeAnalysis.Operations;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Models;

public class ValidatorCollectionItem
{
    public string PropertyPath { get; set; } = null!;
    public List<IInvocationOperation> Validators { get; set; } = null!;
}