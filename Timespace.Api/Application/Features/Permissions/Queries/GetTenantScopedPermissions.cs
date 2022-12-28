using FluentValidation;
using MediatR;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Permissions.Queries;

public static class GetTenantScopedPermissions {
    public record Query : IRequest<Response>
    {
        
    }
    
    public record Response()
    {
        List<ResponsePermissionGroup> PermissionGroups { get; init; } = null!;
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
            return new Response
            {
            
            };
        }
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            
        }
    }
}