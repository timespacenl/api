using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Timespace.Api.Application.Common.Attributes;
using Timespace.Api.Application.Features.Authentication.Registration.Common;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Persistence;

namespace Timespace.Api.Application.Features.Authentication.Registration.Commands;

public static class CreateRegistrationFlow {
    
    [AllowUnauthenticated]
    public record Command : IRequest<Response>
    {
        public string Email { get; init; } = null!;
        public string CaptchaToken { get; init; } = null!;
    }

    public record Response : IRegistrationFlowResponse
    {
        public Guid FlowId { get; set; }
        public string NextStep { get; set; } = null!;
        public Instant ExpiresAt { get; set; }
    }

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
        private readonly AuthenticationConfiguration _authConfiguration;
    
        public Handler(AppDbContext db, IClock clock, IOptions<AuthenticationConfiguration> authConfiguration)
        {
            _db = db;
            _clock = clock;
            _authConfiguration = authConfiguration.Value;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            request = request with { Email = request.Email.ToLower() };
            
            var existingCredential = await _db.IdentityIdentifiers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Identifier == request.Email, cancellationToken: cancellationToken);
            
            var existingFlow = await _db.RegistrationFlows
                .IgnoreQueryFilters()
                .Where(x => x.ExpiresAt > _clock.GetCurrentInstant())
                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken: cancellationToken);
            
            if (existingCredential != null || existingFlow != null)
                throw new DuplicateIdentifierException();
            
            var flow = new RegistrationFlow()
            {
                Email = request.Email.ToLower(),
                NextStep = RegistrationFlowSteps.SetPersonalInformation,
                ExpiresAt = _clock.GetCurrentInstant() + Duration.FromMinutes(_authConfiguration.RegistrationFlowTimeoutMinutes)
            };
            
            _db.RegistrationFlows.Add(flow);
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response()
            {
                FlowId = flow.Id,
                NextStep = flow.NextStep,
                ExpiresAt = flow.ExpiresAt
            };
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Email).EmailAddress().NotEmpty();
            RuleFor(x => x.CaptchaToken).NotEmpty();
        }
    }
}