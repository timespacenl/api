using Timespace.Api.Infrastructure.Persistence;
using Timespace.SourceGenerators;

namespace Timespace.Api.Application.Features.Modules.Employees.Queries;

[GenerateMediatr]
public static partial class GetEmployees
{
    public partial record Query(string Thing);

    public record Response(string Test);
    
    private static async Task<Response> Handle(Query query, AppDbContext db, CancellationToken cancellationToken)
    {
        return new Response("Test");
    }

    public partial class Validator
    {
        public Validator()
        {
            
        }
    }
}