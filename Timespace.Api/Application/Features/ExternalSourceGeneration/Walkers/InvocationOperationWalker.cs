using Microsoft.CodeAnalysis.Operations;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Walkers;

public class InvocationOperationWalker : OperationWalker
{
    public List<IInvocationOperation> InvocationOperations { get; } = new List<IInvocationOperation>();
    
    public override void VisitInvocation(IInvocationOperation operation)
    {
        InvocationOperations.Add(operation);
        base.VisitInvocation(operation);
    }
}