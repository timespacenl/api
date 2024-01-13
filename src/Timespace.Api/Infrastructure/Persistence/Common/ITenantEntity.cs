namespace Timespace.Api.Infrastructure.Persistence.Common;

public interface ITenantEntity
{
	//public Tenant Tenant { get; set; }
	/// <remarks>
	///     Make this required in the implementation
	/// </remarks>
	public Guid TenantId { get; set; }
}
