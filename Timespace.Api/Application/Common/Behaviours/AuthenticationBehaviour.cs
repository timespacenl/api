using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Common.Exceptions;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Common.Behaviours;

public class AuthenticationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IAuthenticationTokenProvider _authenticationTokenProvider;
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly IUsageContext _usageContext;
    
    public AuthenticationBehaviour(IAuthenticationTokenProvider authenticationTokenProvider, AppDbContext db, IClock clock, IUsageContext usageContext)
    {
        _authenticationTokenProvider = authenticationTokenProvider;
        _db = db;
        _clock = clock;
        _usageContext = usageContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if(request.GetType().GetCustomAttribute<InternalAttribute>() != null)
            return await next();
        
        if (request.GetType().GetCustomAttribute<AllowUnauthenticatedAttribute>() != null)
            return await next();
        
        if (_authenticationTokenProvider.AuthenticationToken == null)
        {
            throw new UnauthenticatedException();
        }

        if (_authenticationTokenProvider.AuthenticationTokenType == AuthenticationTokenType.UserSession)
        {
            var session = await _db.Sessions
                .IgnoreQueryFilters()
                .Include(x => x.Identity)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SessionToken == _authenticationTokenProvider.AuthenticationToken,
                    cancellationToken);
            
            if (session == null)
                throw new SessionNotFoundException();
            
            if(session.ExpiresAt < _clock.GetCurrentInstant())
                throw new SessionExpiredException();
            
            _usageContext.IdentityId = session.IdentityId;
            _usageContext.TenantId = session.TenantId;
        }
        
        // TODO: Add api key authentication
        
        return await next();
    }
}