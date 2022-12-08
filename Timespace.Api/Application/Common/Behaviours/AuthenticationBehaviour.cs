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
        if (_authenticationTokenProvider.AuthenticationToken == null)
        {
            if (request.GetType().GetCustomAttribute<AllowUnauthenticatedAttribute>() != null)
                return await next();

            throw new UnauthenticatedException();
        }

        if (_authenticationTokenProvider.AuthenticationTokenType == AuthenticationTokenType.UserSession)
        {
            var session = await _db.Sessions
                .Include(x => x.Identity)
                .ThenInclude(x => x.Tenant)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SessionToken == _authenticationTokenProvider.AuthenticationToken,
                    cancellationToken);
            
            if (session == null)
                throw new SessionNotFoundException();
            
            if(session.ExpiresAt < _clock.GetCurrentInstant())
                throw new SessionExpiredException();
            
            _usageContext.Identity = session.Identity;
            _usageContext.Tenant = session.Identity.Tenant;
        }
        
        // TODO: Add api key authentication
        
        return await next();
    }
}