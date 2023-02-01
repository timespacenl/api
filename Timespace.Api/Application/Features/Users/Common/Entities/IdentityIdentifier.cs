using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Common.Entities;

public class IdentityIdentifier : IEntity, ISoftDeletable, ITenantEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Instant? DeletedAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }
    
    public Tenant Tenant { get; set; } = null!;
    public required Guid TenantId { get; set; }
    
    public required bool Primary { get; set; }
    
    public bool Verified { get; set; }
    public required string Identifier { get; set; }
    public Instant LastVerificationRequestSent { get; set; }
    public bool AllowLogin { get; set; }
}

public class IdentityIdentifierEntityTypeConfiguration : IEntityTypeConfiguration<IdentityIdentifier> {
    public void Configure(EntityTypeBuilder<IdentityIdentifier> builder)
    {
        builder.HasIndex(x => x.Identifier).IsUnique();
    }
}