using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Features.Modules.Employees.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Modules.Employees.Queries;

public static class GetEmployee {
    public record Query : IRequest<Response>
    {
        [FromRoute(Name = "employeeId")]
        public Guid EmployeeId { get; init; }
    }
    
    public record Response()
    {
        public Guid EmployeeId { get; init; }
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Email { get; init; } = null!;
    }
    
    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly AppDbContext _db;
    
        public Handler(AppDbContext db)
        {
            _db = db;
        }
    
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var employeeIdentity = await _db.Identities.FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken: cancellationToken);

            if (employeeIdentity == null)
                throw new EmployeeNotFoundException();
            
            var identifier = await _db.IdentityIdentifiers.FirstOrDefaultAsync(x => x.IdentityId == employeeIdentity.Id && x.Primary == true, cancellationToken: cancellationToken);

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
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }
    }
}