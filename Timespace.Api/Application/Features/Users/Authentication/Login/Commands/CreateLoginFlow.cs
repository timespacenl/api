using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Users.Authentication.Login.Common;
using Timespace.Api.Application.Features.Users.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Users.Authentication.Login.Exceptions;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Users.Authentication.Login.Commands;

public static class CreateLoginFlow {
    
    [AllowUnauthenticated]
    public record Command : IRequest<Response>
    {
        public string Email { get; init; } = null!;
        public bool RememberMe { get; init; }
    }

    public record Response : ILoginFlowResponse
    {
        public Guid FlowId { get; set; }
        public Instant ExpiresAt { get; set; }
        public string NextStep { get; set; } = null!;
        public string? SessionToken { get; set; }
        public List<string> AllowedMethodsForNextStep { get; set; } = new();
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
        private readonly AuthenticationConfiguration _authconfiguration;
        
        public Handler(AppDbContext db, IClock clock, IOptions<AuthenticationConfiguration> authconfiguration)
        {
            _db = db;
            _clock = clock;
            _authconfiguration = authconfiguration.Value;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var identityIdentifier = await _db.IdentityIdentifiers
                .IgnoreQueryFilters()
                .Include(x => x.Identity)
                .FirstOrDefaultAsync(x => x.Identifier == request.Email.ToLower(), cancellationToken: cancellationToken);

            if (identityIdentifier == null || identityIdentifier.AllowLogin == false)
                throw new IdentifierNotFoundException();
            
            var credentialMethods = await _db.IdentityCredentials.Where(x => x.IdentityId == identityIdentifier.IdentityId && CredentialTypes.AllFirstFactor.Contains(x.CredentialType))
                .IgnoreQueryFilters()
                .Select(x => x.CredentialType)
                .ToListAsync(cancellationToken);

            var flow = new LoginFlow
            {
                IdentityId = identityIdentifier.IdentityId,
                TenantId = identityIdentifier.Identity.TenantId,
                ExpiresAt = _clock.GetCurrentInstant().Plus(Duration.FromMinutes(_authconfiguration.LoginFlowTimeoutMinutes)),
                AllowedMethodsForNextStep = credentialMethods,
                NextStep = LoginFlowSteps.SetCredentials
            };

            _db.LoginFlows.Add(flow);
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response
            {
                FlowId = flow.Id,
                ExpiresAt = flow.ExpiresAt,
                NextStep = flow.NextStep,
                AllowedMethodsForNextStep = flow.AllowedMethodsForNextStep,
                SessionToken = null
            };
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Email).EmailAddress().NotEmpty().EmailAddress();
        }
    }
}