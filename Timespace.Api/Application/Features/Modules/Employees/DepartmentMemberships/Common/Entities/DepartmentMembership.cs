using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Application.Features.Tenants.Settings.Departments.Common.Entities;
using Timespace.Api.Application.Features.Tenants.Settings.Roles.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities;
using Timespace.Api.Infrastructure.Persistence.Common;

namespace Timespace.Api.Application.Features.Modules.Employees.DepartmentMemberships.Common.Entities;

public class DepartmentMembership : IEntity, ITenantEntity, ISoftDeletable
{
    public Guid Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant UpdatedAt { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public required Guid TenantId { get; set; }
    public Instant? DeletedAt { get; set; }
    
    public Department Department { get; set; } = null!;
    public Guid DepartmentId { get; set; }
    
    public Identity Identity { get; set; } = null!;
    public Guid IdentityId { get; set; }
    
    public List<DepartmentRole> Roles { get; set; } = null!;
}