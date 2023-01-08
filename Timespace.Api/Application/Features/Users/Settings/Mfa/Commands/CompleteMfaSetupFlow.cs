using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Commands;

public static class CompleteMfaSetupFlow {
    public record Command : IRequest<Response>
    {
        [FromRoute(Name = "flowId")]
        public Guid FlowId { get; init; }
        
        [FromBody]
        public CommandBody Body { get; init; } = null!;
    }

    public record CommandBody
    {
        public string TotpCode { get; init; } = null!;
    }
    
    public record Response()
    {
        public bool Success { get; init; }
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
            var flow = await _db.MfaSetupFlows.FirstOrDefaultAsync(x => x.Id == request.FlowId, cancellationToken);

            if (flow == null)
                throw new MfaSetupFlowNotFoundException();
            
            if(flow.ExpiresAt < _clock.GetCurrentInstant())
                throw new MfaSetupFlowExpiredException();

            var existingTotpCredentials = await _db.IdentityCredentials.Where(x => x.IdentityId == flow.IdentityId && x.CredentialType == CredentialTypes.Totp).ToListAsync(cancellationToken);

            if (existingTotpCredentials.Any())
                throw new MfaAlreadySetUpException();
            
            var totp = new Totp(Base32Encoding.ToBytes(flow.Secret));
            var verificationWindow = new VerificationWindow(1, 1);
            if (totp.VerifyTotp(_clock.GetCurrentInstant().ToDateTimeUtc(), request.Body.TotpCode, out _, verificationWindow))
            {
                var credential = new IdentityCredential()
                {
                    IdentityId = flow.IdentityId,
                    TenantId = flow.TenantId,
                    CredentialType = CredentialTypes.Totp,
                    Configuration = flow.Secret,
                };

                var identity = await _db.Identities.FirstOrDefaultAsync(x => x.Id == flow.IdentityId, cancellationToken);

                _db.IdentityCredentials.Add(credential);
                identity!.RequiresMfa = true;
                
                await _db.SaveChangesAsync(cancellationToken);

                return new Response
                {
                    Success = true
                };
            }

            throw new VerificationTotpInvalidException();
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.FlowId).NotEmpty();
            RuleFor(x => x.Body).NotNull();
            RuleFor(x => x.Body.TotpCode).NotEmpty();
        }
    }
}