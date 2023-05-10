﻿using Destructurama.Attributed;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Application.Features.Users.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Users.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Application.Features.Users.Authentication.Sessions.Common.Entities;
using Timespace.Api.Application.Features.Users.Authentication.Verification;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;
using Constants = Timespace.Api.Infrastructure.Constants;

namespace Timespace.Api.Application.Features.Users.Authentication.Registration.Commands;

// [GenerateTsTypes]
public static class CompleteRegistrationFlow {
    
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
        [LogMasked] 
        public string? Password { get; init; }
        public bool AcceptTerms { get; init; }
        public bool MagicLink { get; init; }
    }

    public record Response
    {
        public string SessionToken { get; init; } = null!;
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationConfiguration _authConfiguration;
        
        public Handler(AppDbContext db, IClock clock, IHttpContextAccessor httpContextAccessor, IOptions<AuthenticationConfiguration> authConfiguration)
        {
            _db = db;
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
            _authConfiguration = authConfiguration.Value;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var flow = _db.RegistrationFlows.FirstOrDefault(x => x.Id == request.FlowId);
            if (flow == null)
                throw new FlowNotFoundException();

            if (flow.ExpiresAt < _clock.GetCurrentInstant())
                throw new FlowExpiredException();

            if(flow.NextStep != RegistrationFlowSteps.CompleteRegistrationFlow)
                throw new InvalidFlowStepException(flow.NextStep);
            
            if(request.Body.Password is not null && request.Body.MagicLink)
                throw new CredentialMismatchException();
            
            if(request.Body.Password is null && !request.Body.MagicLink)
                throw new MissingCredentialException();

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var tenant = new Tenant()
                {
                    CompanyName = flow.CompanyName!,
                    CompanyIndustry = flow.CompanyIndustry!,
                    CompanySize = flow.CompanySize!.Value,
                };

                _db.Tenants.Add(tenant);
                await _db.SaveChangesAsync(cancellationToken);
                
                var identity = new Identity()
                {
                    FirstName = flow.FirstName!,
                    LastName = flow.LastName!,
                    PhoneNumber = flow.PhoneNumber,
                    TenantId = tenant.Id
                };
                
                _db.Identities.Add(identity);
                await _db.SaveChangesAsync(cancellationToken);

                var identifier = new IdentityIdentifier()
                {
                    Identifier = flow.Email,
                    Verified = false,
                    AllowLogin = true,
                    IdentityId = identity.Id,
                    TenantId = tenant.Id,
                    Primary = true
                };
                
                _db.IdentityIdentifiers.Add(identifier);

                var credential = new IdentityCredential()
                {
                    IdentityId = identity.Id,
                    TenantId = tenant.Id,
                    CredentialType = request.Body.MagicLink ? CredentialTypes.MagicLink : CredentialTypes.Password,
                    Configuration = request.Body.MagicLink ? "" : PasswordHasher.HashPasswordV3(request.Body.Password!)
                };
                
                identity.Credentials.Add(credential);

                await _db.SaveChangesAsync(cancellationToken);
                
                var verification = new Verification.Verification
                {
                    VerificationTokenType = VerificationType.Email,
                    VerificationToken = RandomStringGenerator.CreateUrlSafeRandomString(Constants.VerificationTokenSize),
                    VerifiableIdentityIdentifierId = identifier.Id,
                    ExpiresAt = _clock.GetCurrentInstant()
                        .Plus(Duration.FromMinutes(_authConfiguration.VerificationTokenTimeoutMinutes))
                };

                _db.Verifications.Add(verification);
                
                flow.NextStep = RegistrationFlowSteps.None;
                flow.CredentialType = credential.CredentialType;
                
                await _db.SaveChangesAsync(cancellationToken);
                
                var session = new Session
                {
                    IdentityId = identity.Id,
                    TenantId = tenant.Id,
                    SessionToken = RandomStringGenerator.CreateUrlSafeRandomString(128),
                    ExpiresAt = _clock.GetCurrentInstant().Plus(Duration.FromDays(_authConfiguration.SessionCookieExpirationDays))
                };

                flow.NextStep = LoginFlowSteps.None;
                
                _db.Sessions.Add(session);
                await _db.SaveChangesAsync(cancellationToken);
                
                await transaction.CommitAsync(cancellationToken);
                
                _httpContextAccessor.HttpContext!.Response.Cookies.Append(_authConfiguration.SessionCookieName, session.SessionToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = null
                });
            
                return new Response
                {
                    SessionToken = session.SessionToken
                };
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.FlowId).NotEmpty();
            RuleFor(x => x.Body).NotNull();
            RuleFor(x => x.Body.Password).MaximumLength(100);
            RuleFor(x => x.Body.AcceptTerms).Equal(true);
            RuleFor(x => x.Body.MagicLink).NotNull();
        }
    }
}