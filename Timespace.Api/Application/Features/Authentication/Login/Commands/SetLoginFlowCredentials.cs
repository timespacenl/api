using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Login.Common;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Login.Exceptions;
using Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Extensions;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Features.Authentication.Login.Commands;

public static class SetLoginFlowCredentials {
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
        
        public Handler(AppDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
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
                CredentialTypes.Password => await AuthenticatePasswordAsync(request.Body.CredentialValue, flow.IdentityId),
                _ => false
            };

            if(!authenticated)
                throw new IncorrectCredentialValueException();
            
            var identity = await _db.Identities.FirstOrDefaultAsync(x => x.Id == flow.IdentityId, cancellationToken);

            if (identity!.RequiresMfa)
            {
                var mfaMethods = await _db.IdentityCredentials
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
                    NextStepAllowedMethods = flow.AllowedMethodsForNextStep
                };
            }

            var session = new Session
            {
                IdentityId = flow.IdentityId,
                SessionToken = RandomStringGenerator.CreateSecureRandomString(128)
            };

            flow.NextStep = LoginFlowSteps.None;
                
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync(cancellationToken);

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
            var passwordCredential = await _db.IdentityCredentials.FirstOrDefaultAsync(
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