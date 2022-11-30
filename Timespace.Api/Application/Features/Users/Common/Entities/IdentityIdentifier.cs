using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Common.Entities;

public class IdentityIdentifier : IEntity, ISoftDeletable
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Instant? DeletedAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }
    
    public bool Verified { get; set; }
    public required string Identifier { get; set; }
    public Instant LastVerificationRequestSent { get; set; }
    public bool AllowLogin { get; set; }
}