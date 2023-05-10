using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Walkers;

public class InvocationExpressionSyntaxWalker : CSharpSyntaxWalker
{
    public ICollection<InvocationExpressionSyntax> Invocations { get; } = new List<InvocationExpressionSyntax>();

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        Invocations.Add(node);
        base.VisitInvocationExpression(node);
    }
}