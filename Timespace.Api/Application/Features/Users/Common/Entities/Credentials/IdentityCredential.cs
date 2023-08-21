using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Common.Entities.Credentials;

public class IdentityCredential : IEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }
    
    public Tenant Tenant { get; set; } = null!;
    public required Guid TenantId { get; set; }
    
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

public interface ICredential
{
    public string CredentialType { get; }
}

public class PasswordCredential : ICredential
{
    public string CredentialType => "password";
    public string Password { get; set; } = null!;
}