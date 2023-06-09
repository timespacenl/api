using FluentValidation;
using MediatR;
using Timespace.Api.Application.Features.AccessControl;
using Timespace.Api.Infrastructure.AccessControl;

namespace Timespace.Api.Application.Features.Users.Permissions.Queries;

public static class GetTenantScopedPermissions {
    public record Query : IRequest<PermissionTree>
    {
        
    }

    public class Handler : IRequestHandler<Query, PermissionTree>
    {
        private readonly PermissionCollections _permissionCollections;
        
        public Handler(PermissionCollections permissionCollections)
        {
            _permissionCollections = permissionCollections;
        }
    
        public async Task<PermissionTree> Handle(Query request, CancellationToken cancellationToken)
        {
            return _permissionCollections.AllTenantScoped;
        }
    }
    
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            
        }
    }
}