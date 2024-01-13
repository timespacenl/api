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
        [property: FromRoute(Name = "employeeId")] int EmployeeId,
        [property: FromQuery(Name = "name")] string Name,
        [property: FromQuery(Name = "prop2")] string Prop2
        );
    
    public record Command2(
        [property: FromQuery] CommandQuery Query,
        [property: FromForm] CommandBody Body
        );

    public record CommandQuery(
        [property: FromQuery(Name = "name")] string Name,
        [property: FromQuery(Name = "prop2")] string Prop2
        );
    
    public record CommandBody(
        [property: FromQuery(Name = "name")] string Email,
        string Password,
        Test Test,
        SharedType SharedType,
        List<SharedType> SharedTypes,
        List<IFormFile> Files,
        Dictionary<string, SharedType> SharedTypesDictionary
        );
    
    public record Command3(
        [property: FromQuery(Name = "name")] string Name,
        [property: FromQuery(Name = "prop2")] string Prop2,
        [property: FromBody] CommandBody Body
        );


    public record Response(
        string Name,
        string Email,
        SharedType SharedType,
        List<SharedType> SharedTypes,
        Dictionary<string, SharedType> SharedTypesDictionary
        );

    public enum Test
    {
        Test1,
        Test2
    }

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
