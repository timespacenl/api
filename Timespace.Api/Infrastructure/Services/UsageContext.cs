namespace Timespace.Api.Infrastructure.Services;

public interface IUsageContext
{
    /// <summary>
    /// This property will only be populated if the user authenticated using a session token. This will not be populated when the user authenticated with an api key.
    /// </summary>
    public Guid? IdentityId { get; set; }
    public Guid? TenantId { get; set; }
    public List<string> Permissions { get; set; }
}

public class UsageContext : IUsageContext
{
    public Guid? IdentityId { get; set; }
    public Guid? TenantId { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public static class UsageContextExtensions
{
    public static Guid GetGuaranteedIdentityId(this IUsageContext usageContext)
    {
        return usageContext.IdentityId ?? throw new InvalidOperationException("Current user is not authenticated");
    }
    
    public static Guid GetGuaranteedTenantId(this IUsageContext usageContext)
    {
        if(usageContext.TenantId == null)
            throw new InvalidOperationException("Current user is not authenticated");

        return usageContext.TenantId.Value;
    }
}