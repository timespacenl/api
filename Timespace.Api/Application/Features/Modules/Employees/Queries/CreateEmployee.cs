using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Modules.Employees.Common;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.SourceGenerators;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace Timespace.Api.Application.Features.Modules.Employees.Queries;

[GenerateMediatr]
public static partial class CreateEmployee
{
    public partial record Command(
        [property: FromQuery(Name = "name")] string Name,
        [property: FromQuery(Name = "prop2")] string Prop2
        );
    
    public record Command2(
        [property: FromQuery] CommandQuery Query,
        [property: FromBody] CommandBody Body
        );

    public record CommandQuery(
        [property: FromQuery(Name = "name")] string Name,
        [property: FromQuery(Name = "prop2")] string Prop2
        );
    
    public record CommandBody(
        [property: FromQuery(Name = "name")] string Email,
        string Password,
        SharedType SharedType,
        List<SharedType> SharedTypes,
        Dictionary<string, SharedType> SharedTypesDictionary
        );
    
    public record Command3(
        [property: FromQuery(Name = "name")] string Name,
        [property: FromQuery(Name = "prop2")] string Prop2,
        [property: FromForm] CommandBody Body
        );


    public record Response(
        string Name,
        string Email,
        SharedType SharedType,
        List<SharedType> SharedTypes,
        Dictionary<string, SharedType> SharedTypesDictionary
        );

    private static async Task<PaginatedResult<Response>> Handle(Command request, AppDbContext db, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        return new(new(), 0, 0, 0);
    }

    public partial class Validator
    {
        public Validator()
        {

        }
    }
}
