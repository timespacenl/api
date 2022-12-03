using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Features.Authentication.Login.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Login.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Login.Commands;

public static class CreateLoginFlow {
    public record Command : IRequest<Response>
    {
        public string Email { get; init; } = null!;
    }

    public record Response
    {
        public Guid FlowId { get; init; }
        public Instant ExpiresAt { get; init; }
        public List<string> Methods { get; init; } = null!;
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

            if (identityIdentifier == null)
                throw new IdentifierNotFoundException();
            
            var credentialMethods = await _db.IdentityCredentials.Where(x => x.IdentityId == identityIdentifier.IdentityId)
                .Select(x => x.CredentialType)
                .ToListAsync(cancellationToken);
            
            var flow = new LoginFlow
            {
                IdentityId = identityIdentifier.IdentityId,
                ExpiresAt = _clock.GetCurrentInstant().Plus(Duration.FromMinutes(5)),
                AllowedMethods = credentialMethods
            };

            _db.LoginFlows.Add(flow);
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response
            {
                FlowId = flow.Id,
                ExpiresAt = flow.ExpiresAt,
                Methods = flow.AllowedMethods
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