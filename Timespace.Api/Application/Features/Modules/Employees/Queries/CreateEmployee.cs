using Timespace.Api.Infrastructure.Persistence;
using Timespace.SourceGenerators;

namespace Timespace.Api.Application.Features.Modules.Employees.Queries;

[GenerateMediatr]
public static partial class CreateEmployee
{
    public partial record Command();

    public record Response();

    private static async Task<Response> Handle(Command request, AppDbContext db, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        return new Response();
    }

    public partial class Validator
    {
        public Validator()
        {

        }
    }
}