using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Features.Authentication.Login.Common;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Login.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Login.Commands;

public static class CreateLoginFlow {
    public record Command : IRequest<Response>
    {
        public string Email { get; init; } = null!;
    }

    public record Response : ILoginFlowResponse
    {
        public Guid FlowId { get; set; }
        public Instant ExpiresAt { get; set; }
        public string NextStep { get; set; } = null!;
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
            var identityIdentifier = await _db.IdentityIdentifiers
                .FirstOrDefaultAsync(x => x.Identifier == request.Email, cancellationToken: cancellationToken);

            if (identityIdentifier == null || identityIdentifier.AllowLogin == false)
                throw new IdentifierNotFoundException();
            
            var credentialMethods = await _db.IdentityCredentials.Where(x => x.IdentityId == identityIdentifier.IdentityId)
                .Select(x => x.CredentialType)
                .ToListAsync(cancellationToken);
            
            var flow = new LoginFlow
            {
                IdentityId = identityIdentifier.IdentityId,
                ExpiresAt = _clock.GetCurrentInstant().Plus(Duration.FromMinutes(5)),
                AllowedMethodsForNextStep = credentialMethods,
                NextStep = LoginFlowSteps.SetCredentials
            };

            _db.LoginFlows.Add(flow);
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response
            {
                FlowId = flow.Id,
                ExpiresAt = flow.ExpiresAt,
                NextStepAllowedMethods = flow.AllowedMethodsForNextStep,
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