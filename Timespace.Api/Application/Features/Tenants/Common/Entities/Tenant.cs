using Microsoft.AspNetCore.Identity;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Tenants.Common.Entities;

public partial class Tenant : IEntity, ISoftDeletable
{
    public int Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Instant? DeletedAt { get; set; }
    
    public List<ApplicationUser> Members { get; set; } = null!;
}
