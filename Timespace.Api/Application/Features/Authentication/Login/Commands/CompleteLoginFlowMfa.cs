using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OtpNet;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Login.Common;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Login.Exceptions;
using Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Features.Authentication.Login.Commands;

public static class CompleteLoginFlowMfa {
    
    [AllowUnauthenticated]
    public record Command : IRequest<Response>
    {
        [FromRoute(Name = "flowId")]
        public Guid FlowId { get; init; }
        
        [FromBody]
        public CommandBody Body { get; init; } = null!;
    }

    public record CommandBody
    {
        public string CredentialType { get; init; } = null!;
        public string CredentialValue { get; init; } = null!;
    }
    
    public record Response : ILoginFlowResponse
    {
        public Guid FlowId { get; set; }
        public string NextStep { get; set; } = null!;
        public Instant ExpiresAt { get; set; }
        public string? SessionToken { get; set; }
        public List<string> NextStepAllowedMethods { get; set; } = new();
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationConfiguration _authenticationConfiguration;
        
        public Handler(AppDbContext db, IClock clock, IHttpContextAccessor httpContextAccessor, IOptions<AuthenticationConfiguration> authenticationConfiguration)
        {
            _db = db;
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
            _authenticationConfiguration = authenticationConfiguration.Value;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var flow = await _db.LoginFlows.FirstOrDefaultAsync(x => x.Id == request.FlowId, cancellationToken);
            
            if(flow == null)
                throw new FlowNotFoundException();
            
            if(flow.ExpiresAt < _clock.GetCurrentInstant())
                throw new FlowExpiredException();

            if (!flow.AllowedMethodsForNextStep.Contains(request.Body.CredentialType))
                throw new CredentialTypeNotConfiguredException();
            
            bool authenticated = request.Body.CredentialType switch
            {
                CredentialTypes.Password => await AuthenticateTotpAsync(request.Body.CredentialValue, flow.IdentityId),
                _ => false
            };
            
            if(!authenticated)
                throw new IncorrectCredentialValueException();

            var session = new Session
            {
                IdentityId = flow.IdentityId,
                SessionToken = RandomStringGenerator.CreateSecureRandomString(128),
                ExpiresAt = _clock.GetCurrentInstant().Plus(Duration.FromDays(_authenticationConfiguration.SessionCookieExpirationDays))
            };

            flow.NextStep = LoginFlowSteps.None;
                
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync(cancellationToken);

            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_authenticationConfiguration.SessionCookieName, session.SessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = flow.RememberMe ? _clock.GetCurrentInstant().Plus(Duration.FromDays(_authenticationConfiguration.SessionCookieExpirationDays)).ToDateTimeOffset() : null
            });
            
            return new Response
            {
                FlowId = flow.Id,
                ExpiresAt = flow.ExpiresAt,
                NextStep = flow.NextStep,
                SessionToken = session.SessionToken
            };
        }

        private async Task<bool> AuthenticateTotpAsync(string totp, Guid identityId)
        {
            var identityCredential = await _db.IdentityCredentials.FirstOrDefaultAsync(
                x => x.IdentityId == identityId && x.CredentialType == CredentialTypes.Totp, CancellationToken.None);

            if (identityCredential == null)
                throw new CredentialTypeNotConfiguredException();

            var totpMgr = new Totp(Base32Encoding.ToBytes(identityCredential.Configuration));
            
            return totpMgr.VerifyTotp(totp, out _, new VerificationWindow(1, 1));
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            
        }
    }
}