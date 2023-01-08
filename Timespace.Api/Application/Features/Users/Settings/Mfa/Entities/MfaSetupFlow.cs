using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Settings.Mfa.Entities;

public class MfaSetupFlow : IEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public required Instant ExpiresAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public required Guid IdentityId { get; set; }
    
    public Tenant Tenant { get; set; } = null!;
    public required Guid TenantId { get; set; }
    
    public required string Secret { get; set; }
}