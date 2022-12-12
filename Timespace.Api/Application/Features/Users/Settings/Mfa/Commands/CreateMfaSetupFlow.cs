using System.Web;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OtpNet;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Entities;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Exceptions;
using Timespace.Api.Infrastructure.Configuration;
using Timespace.Api.Infrastructure.Persistence;
using Timespace.Api.Infrastructure.Services;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Commands;

public static class CreateMfaSetupFlow
{
    public record Command : IRequest<Response>;
    
    public record Response()
    {
        public Guid FlowId { get; init; }
        public Instant ExpiresAt { get; init; }
        public string Secret { get; init; } = null!;
        public string QrCodeUrl { get; init; } = null!;
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly AppDbContext _db;
        private readonly IUsageContext _usageContext;
        private readonly IClock _clock;
        private readonly UserSettingsConfiguration _userSettingsConfiguration;
        
        public Handler(AppDbContext db, IUsageContext usageContext, IClock clock, IOptions<UserSettingsConfiguration> userSettingsConfiguration)
        {
            _db = db;
            _usageContext = usageContext;
            _clock = clock;
            _userSettingsConfiguration = userSettingsConfiguration.Value;
        }
    
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var key = KeyGeneration.GenerateRandomKey(20);

            var user = _usageContext.GetGuaranteedIdentity();

            var existingTotpCredentials = await _db.IdentityCredentials.Where(x => x.IdentityId == user.Id && x.CredentialType == CredentialTypes.Totp).ToListAsync(cancellationToken);

            if (existingTotpCredentials.Any())
                throw new MfaAlreadySetUpException();
            
            var identifier = await _db.IdentityIdentifiers.OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(x => x.IdentityId == user.Id, cancellationToken);

            var flow = new MfaSetupFlow()
            {
                Secret = Base32Encoding.ToString(key),
                IdentityId = user.Id,
                ExpiresAt = _clock.GetCurrentInstant().Plus(Duration.FromMinutes(_userSettingsConfiguration.MfaSetupFlowExpirationInMinutes))
            };

            _db.MfaSetupFlows.Add(flow);
            await _db.SaveChangesAsync(cancellationToken);
            
            return new Response
            {
                FlowId = flow.Id,
                ExpiresAt = flow.ExpiresAt,
                Secret = flow.Secret,
                QrCodeUrl = $"otpauth://totp/{HttpUtility.UrlEncode($"Timespace:{identifier!.Identifier}")}?secret={flow.Secret}&issuer=Timespace&period=30&digits=6"
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