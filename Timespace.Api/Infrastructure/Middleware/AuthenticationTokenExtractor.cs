using Microsoft.Extensions.Options;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Infrastructure.Middleware;

public class AuthenticationTokenExtractor : IMiddleware
{
    private readonly IAuthenticationTokenProvider _authenticationTokenProvider;
    private readonly AuthenticationConfiguration _authenticationConfiguration;

    public AuthenticationTokenExtractor(IAuthenticationTokenProvider authenticationTokenProvider,
        IOptions<AuthenticationConfiguration> authenticationConfiguration)
    {
        _authenticationTokenProvider = authenticationTokenProvider;
        _authenticationConfiguration = authenticationConfiguration.Value;
    }

    /// <summary>
    /// Session tokens can be passed in the Authorization header or in a cookie.
    /// Api tokens can only be passed in the X-Api-Key header.
    /// </summary>
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Cookies.TryGetValue(_authenticationConfiguration.SessionCookieName, out var token))
        {
            _authenticationTokenProvider.AuthenticationToken = token;
            _authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        }
        else if (context.Request.Headers.TryGetValue("Authorization", out var sessionToken))
        {
            _authenticationTokenProvider.AuthenticationToken = sessionToken.ToString();
            _authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.UserSession;
        }
        else if (context.Request.Headers.TryGetValue(_authenticationConfiguration.ApiKeyHeaderName, out var apiKey))
        {
            _authenticationTokenProvider.AuthenticationToken = apiKey.ToString();
            _authenticationTokenProvider.AuthenticationTokenType = AuthenticationTokenType.ApiKey;
        }
        else
        {
            _authenticationTokenProvider.AuthenticationToken = null;
            _authenticationTokenProvider.AuthenticationTokenType = null;
        }

        return next(context);
    }
}