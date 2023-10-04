using FluentValidation;
using MediatR;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.SourceGenerators;

namespace Timespace.Api.Application.Features.Modules.Employees.Queries;

[GenerateMediatr]
public static partial class GetEmployees
{
    public partial record Query;

    public record Response(string Test);
    
    private static async Task<Response> Handle(Query request, CancellationToken cancellationToken)
    {
        return new Response("Test");
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            
        }
    }
}