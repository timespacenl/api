using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Common.Entities;

public partial class Identity : IEntity, ISoftDeletable, ITenantEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Instant? DeletedAt { get; set; }
    
    public Tenant Tenant { get; set; } = null!;
    public Guid TenantId { get; set; }
    
    public List<IdentityCredential> Credentials { get; set; } = new();
}