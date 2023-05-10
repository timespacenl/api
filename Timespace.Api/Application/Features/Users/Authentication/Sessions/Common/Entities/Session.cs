using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Authentication.Sessions.Common.Entities;

public class Session : IEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public required Guid IdentityId { get; set; }
    
    public Tenant Tenant { get; set; } = null!;
    public required Guid TenantId { get; set; }
    
    public required string SessionToken { get; set; }
    public required Instant ExpiresAt { get; set; }
}