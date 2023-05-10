using Microsoft.CodeAnalysis.Operations;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Walkers;

public class PropertyReferenceOperationWalker : OperationWalker
{
    public List<IPropertyReferenceOperation> PropertyReferences { get; } = new List<IPropertyReferenceOperation>();
    
    public override void VisitPropertyReference(IPropertyReferenceOperation operation)
    {
        PropertyReferences.Add(operation);
        base.VisitPropertyReference(operation);
    }
}