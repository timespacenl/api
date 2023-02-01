using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Login.Common;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Login.Exceptions;
using Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Extensions;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Features.Authentication.Login.Commands;

public static class SetLoginFlowCredentials {
    
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
        public List<string> AllowedMethodsForNextStep { get; set; } = new();
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
        private readonly AuthenticationConfiguration _authConfiguration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public Handler(AppDbContext db, IClock clock, IOptions<AuthenticationConfiguration> authConfiguration, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
            _authConfiguration = authConfiguration.Value;
        }
        
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var flow = await _db.LoginFlows
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == request.FlowId, cancellationToken);
            
            if(flow == null)
                throw new FlowNotFoundException();
            
            if(flow.ExpiresAt < _clock.GetCurrentInstant())
                throw new FlowExpiredException();

            if(flow.NextStep != LoginFlowSteps.SetCredentials)
                throw new InvalidFlowStepException(flow.NextStep);

            if (!flow.AllowedMethodsForNextStep.Contains(request.Body.CredentialType))
                throw new CredentialTypeNotConfiguredException();
            
            bool authenticated = request.Body.CredentialType switch
            {
                CredentialTypes.Password => await AuthenticatePasswordAsync(request.Body.CredentialValue, flow.IdentityId),
                _ => false
            };

            if(!authenticated)
                throw new IncorrectCredentialValueException();
            
            var identity = await _db.Identities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == flow.IdentityId, cancellationToken);

            if (identity!.RequiresMfa)
            {
                var mfaMethods = await _db.IdentityCredentials
                    .IgnoreQueryFilters()
                    .Where(x => CredentialTypes.AllSecondFactor.Contains(x.CredentialType) &&
                                x.IdentityId == identity.Id)
                    .Select(x => x.CredentialType)
                    .ToListAsync(cancellationToken: cancellationToken);
                
                flow.NextStep = LoginFlowSteps.CompleteMfa;
                flow.AllowedMethodsForNextStep = mfaMethods;
                
                await _db.SaveChangesAsync(cancellationToken);
                
                return new Response
                {
                    FlowId = flow.Id,
                    NextStep = flow.NextStep,
                    ExpiresAt = flow.ExpiresAt,
                    SessionToken = null,
                    AllowedMethodsForNextStep = flow.AllowedMethodsForNextStep
                };
            }
            
            var session = new Session
            {
                IdentityId = flow.IdentityId,
                TenantId = flow.TenantId,
                SessionToken = RandomStringGenerator.CreateSecureRandomString(128),
                ExpiresAt = _clock.GetCurrentInstant().Plus(Duration.FromDays(_authConfiguration.SessionCookieExpirationDays))
            };

            flow.NextStep = LoginFlowSteps.None;
                
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync(cancellationToken);

            _httpContextAccessor.HttpContext!.Response.Cookies.Append(_authConfiguration.SessionCookieName, session.SessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = flow.RememberMe ? _clock.GetCurrentInstant().Plus(Duration.FromDays(_authConfiguration.SessionCookieExpirationDays)).ToDateTimeOffset() : null
            });
            
            return new Response
            {
                FlowId = flow.Id,
                ExpiresAt = flow.ExpiresAt,
                NextStep = flow.NextStep,
                SessionToken = session.SessionToken
            };
        }

        private async Task<bool> AuthenticatePasswordAsync(string password, Guid identityId)
        {
            var passwordCredential = await _db.IdentityCredentials
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                x => x.IdentityId == identityId && x.CredentialType == CredentialTypes.Password);
                
            if(passwordCredential == null)
                throw new CredentialTypeNotConfiguredException();

            return PasswordHasher.VerifyHashedPasswordV3(passwordCredential.Configuration,
                    password);
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.FlowId).NotEmpty();
            RuleFor(x => x.Body).NotNull();
            RuleFor(x => x.Body.CredentialType).NotEmpty().OneOf(CredentialTypes.AllFirstFactor).WithMessage("Credential type is not supported");
        }
    }
}