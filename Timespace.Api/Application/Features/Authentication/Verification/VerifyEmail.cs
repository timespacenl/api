using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Authentication.Verification.Exceptions;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Verification;

public static class VerifyEmail {
    [AllowUnauthenticated]
    public record Command : IRequest<Response>
    {
        [FromQuery]
        public string VerificationToken { get; init; } = null!;
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
            var verification = await _db.Verifications
                .IgnoreQueryFilters()
                .Include(x => x.VerifiableIdentityIdentifier)
                .FirstOrDefaultAsync(x => x.VerificationToken == request.VerificationToken, cancellationToken);

            if (verification == null)
                throw new VerificationTokenNotFoundException();
            
            if(verification.ExpiresAt < _clock.GetCurrentInstant())
                throw new VerificationTokenExpiredException();

            verification.VerifiableIdentityIdentifier.Verified = true;

            _db.Verifications.Remove(verification);
            
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response
            {
                Success = true
            };
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.VerificationToken).NotEmpty();
        }
    }
}