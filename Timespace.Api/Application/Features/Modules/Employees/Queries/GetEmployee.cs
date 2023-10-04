using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Features.Modules.Employees.Exceptions;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.SourceGenerators;

namespace Timespace.Api.Application.Features.Modules.Employees.Queries;

[GenerateMediatr]
public static partial class GetEmployee {
    public partial record Query
    {
        [FromRoute(Name = "employeeId")]
        public Guid EmployeeId { get; init; }
    }
    
    public record Response
    {
        public Guid EmployeeId { get; init; }
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Email { get; init; } = null!;
    }
    
    private static async Task<Response> Handle(Query request, AppDbContext db, CancellationToken cancellationToken)
    {
        var employeeIdentity = await db.Identities.FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken: cancellationToken);

        if (employeeIdentity == null)
            throw new EmployeeNotFoundException();
            
        var identifier = await db.IdentityIdentifiers.FirstOrDefaultAsync(x => x.IdentityId == employeeIdentity.Id && x.Primary == true, cancellationToken: cancellationToken);

        if (identifier == null)
            throw new EmployeeNotFoundException(); // Should never happen as the user is created with an email identifier
            
        return new Response
        {
            EmployeeId = employeeIdentity.Id,
            FirstName = employeeIdentity.FirstName,
            LastName = employeeIdentity.LastName,
            Email = identifier.Identifier
        };
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }
    }
}