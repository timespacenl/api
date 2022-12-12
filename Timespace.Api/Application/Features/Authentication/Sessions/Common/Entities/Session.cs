using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Authentication.Sessions.Common.Entities;

public class Session : IEntity
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public required Guid IdentityId { get; set; }
    
    public required string SessionToken { get; set; }
    public required Instant ExpiresAt { get; set; }
}