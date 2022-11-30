using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Common.Entities;

public class Identity : IEntity, ISoftDeletable, ITenantEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public Instant DeletedAt { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid TenantId { get; set; }
}