using System.Text.RegularExpressions;
using FluentValidation;
using MediatR;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Entities;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Commands;

public static class CreateMfaSetupFlow
{
    public record Command : IRequest<Response>;
    
    public record Response()
    {
        public string FlowId { get; init; } = null!;
        public string Secret { get; init; } = null!;
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
        private readonly IUsageContext _usageContext;
        
        public Handler(AppDbContext db, IUsageContext usageContext)
        {
            _db = db;
            _usageContext = usageContext;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var totpSecret = RandomStringGenerator.CreateSecureRandomString(32);
            var totpSecretFormatted = Regex.Replace(totpSecret, "[^a-zA-Z0-9]", "");

            var user = _usageContext.GetGuaranteedIdentity();
            
            var flow = new MfaSetupFlow()
            {
                Secret = totpSecretFormatted,
                IdentityId = user.Id
            };
            
            return new Response
            {
                
            };
        }
    }
    
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            
        }
    }
}