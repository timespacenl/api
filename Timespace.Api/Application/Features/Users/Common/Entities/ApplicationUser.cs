using Microsoft.AspNetCore.Identity;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Users.Common.Entities;

public class ApplicationUser : IdentityUser<int>, ISoftDeletable
{
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Instant? DeletedAt { get; set; }
    
    // public List<Tenant> Tenants { get; set; } = null!;
}
