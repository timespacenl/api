﻿using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Tenants.Settings.Roles.Common.Entities;

public class TenantRole : IEntity, ITenantEntity, ISoftDeletable
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid TenantId { get; set; }
    public Instant? DeletedAt { get; set; }
    
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Locked { get; set; }
    public List<string> Permissions { get; set; } = new();
    public int Position { get; set; }
}