using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Authentication.Verification;

public class Verification : IEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Instant ExpiresAt { get; set; }

    public Guid VerifiableIdentityIdentifierId { get; set; }
    public IdentityIdentifier VerifiableIdentityIdentifier { get; set; } = null!;

    public string VerificationTokenType { get; set; } = null!;
    public string VerificationToken { get; set; } = null!;
}

public class VerificationEntityTypeConfiguration : IEntityTypeConfiguration<Verification> {
    public void Configure(EntityTypeBuilder<Verification> builder)
    {
        builder.HasIndex(x => x.VerificationToken).IsUnique();
    }
}

public class VerificationType
{
    public const string Email = "email";
}