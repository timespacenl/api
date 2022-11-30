using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Common.Entities;

public class IdentityLookupSecret : IEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public required Guid IdentityId { get; set; }
    
    [Column(TypeName = "jsonb")]
    public required List<LookupSecret> Secrets { get; set; }
}

public class LookupSecret
{
    public string Secret { get; set; } = null!;
    public bool IsUsed { get; set; }
}

public class IdentityLookupSecretEntityTypeConfiguration : IEntityTypeConfiguration<IdentityLookupSecret>
{
    public void Configure(EntityTypeBuilder<IdentityLookupSecret> builder)
    {
        builder.HasIndex(x => x.IdentityId).IsUnique();
    }
}

