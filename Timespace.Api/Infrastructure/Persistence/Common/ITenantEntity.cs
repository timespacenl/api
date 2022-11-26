using Timespace.Api.Application.Features.Tenants.Common.Entities;

namespace Timespace.Api.Infrastructure.Persistence.Common;

public interface ITenantEntity
{
    public Tenant Tenant { get; set; }
    public Guid TenantId { get; set; }
}