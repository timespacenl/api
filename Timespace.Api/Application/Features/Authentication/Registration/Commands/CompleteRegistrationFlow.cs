using Destructurama.Attributed;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;
using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Features.Authentication.Registration.Commands;

public static class CompleteRegistrationFlow {
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

    public record Response();
    
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
                IdentityId = identity.Id
            };
            
            _db.IdentityIdentifiers.Add(identifier);
            await _db.SaveChangesAsync(cancellationToken);

            var credential = new IdentityCredential()
            {
                IdentityId = identity.Id,
                CredentialType = request.Body.MagicLink ? CredentialTypes.MagicLink : CredentialTypes.Password,
                Configuration = request.Body.MagicLink ? "" : PasswordHasher.HashPasswordV3(request.Body.Password!)
            };
            
            identity.Credentials.Add(credential);
            await _db.SaveChangesAsync(cancellationToken);
            
            flow.NextStep = RegistrationFlowSteps.None;
            flow.CredentialType = credential.CredentialType;
            
            await _db.SaveChangesAsync(cancellationToken);

            return new Response();
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