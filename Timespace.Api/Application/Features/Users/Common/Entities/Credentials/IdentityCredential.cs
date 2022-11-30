using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Common.Entities.Credentials;

public class IdentityCredential : IEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }
    
    public required string CredentialType { get; set; }
    public required string Configuration { get; set; }
}

public class IdentityCredentialTypeConfiguration : IEntityTypeConfiguration<IdentityCredential>
{
    public void Configure(EntityTypeBuilder<IdentityCredential> builder)
    {
        builder.HasIndex(x => new {x.IdentityId, x.CredentialType}).IsUnique();
    }
}