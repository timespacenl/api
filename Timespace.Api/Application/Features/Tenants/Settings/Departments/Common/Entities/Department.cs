﻿using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Tenants.Settings.Departments.Common.Entities;

public class Department : IEntity, ITenantEntity, ISoftDeletable
{
    public int Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public required Guid TenantId { get; set; }
    public Instant? DeletedAt { get; set; }
}
