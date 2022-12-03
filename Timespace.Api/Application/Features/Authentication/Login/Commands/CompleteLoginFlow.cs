using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timespace.Api.Application.Features.Authentication.Common.Exceptions;
using Timespace.Api.Application.Features.Authentication.Login.Exceptions;
using Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Extensions;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Features.Authentication.Login.Commands;

public static class CompleteLoginFlow {
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
    
    public record Response
    {
        public string SessionToken { get; init; } = null!;
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

            if (!flow.AllowedMethods.Contains(request.Body.CredentialType))
                throw new CredentialTypeNotConfiguredException();
            
            bool authenticated = request.Body.CredentialType switch
            {
                CredentialTypes.Password => await AuthenticatePasswordAsync(request.Body.CredentialValue, flow.IdentityId),
                _ => false
            };

            if(!authenticated)
                throw new IncorrectCredentialValueException();
            
            var identity = await _db.Identities.FirstOrDefaultAsync(x => x.Id == flow.IdentityId, cancellationToken);

            var session = new Session
            {
                IdentityId = flow.IdentityId,
                MfaRequired = identity!.RequiresMfa,
                SessionToken = RandomStringGenerator.CreateSecureRandomString(128)
            };

            flow.Completed = true;
            
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync(cancellationToken);

            return new Response
            {
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