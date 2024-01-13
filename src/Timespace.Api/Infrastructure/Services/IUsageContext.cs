using Timespace.Api.Infrastructure.Persistence.Entities.Users;

namespace Timespace.Api.Infrastructure.Services;

public interface IUsageContext
{
	public ApplicationUser CurrentUser { get; }
	public Guid CurrentTenantId { get; }
}
