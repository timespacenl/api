using System.Reflection;
using MediatR;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly AppDbContext _db;
    private readonly IUsageContext _usageContext;
    private readonly IClock _clock;
    
    public AuthorizationBehaviour(AppDbContext db, IUsageContext usageContext, IClock clock)
    {
        _db = db;
        _usageContext = usageContext;
        _clock = clock;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Authorization was done at the start of the request
        if(request.GetType().GetCustomAttribute<InternalAttribute>() != null)
            return await next();
        
        if(_usageContext.Identity is null && _usageContext.Tenant is null)
            if (request.GetType().GetCustomAttribute<AllowUnauthenticatedAttribute>() == null)
                throw new UnauthenticatedException();

        var permissionAuthorizeAttribute = request.GetType().GetCustomAttribute<PermissionAuthorizeAttribute>();

        if (permissionAuthorizeAttribute is not null)
        {
            switch (permissionAuthorizeAttribute.Type)
            {
                case PermissionAuthorizeType.Or:
                    if(_usageContext.Permissions.Any(x => permissionAuthorizeAttribute.Permissions.Contains(x))) 
                        return await next();
                    break;
                case PermissionAuthorizeType.And:
                    if(permissionAuthorizeAttribute.Permissions.All(x => _usageContext.Permissions.Contains(x))) 
                        return await next();
                    break;
            }
            
            throw new ForbiddenException();
        }

        // Route doesn't require a specific permission, just an authenticated user
        return await next();
    }
}