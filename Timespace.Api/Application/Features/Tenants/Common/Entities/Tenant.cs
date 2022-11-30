using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Tenants.Common.Entities;

public class Tenant : IEntity, ISoftDeletable
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Instant? DeletedAt { get; set; }
}