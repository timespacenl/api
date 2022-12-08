using Timespace.Api.Application.Features.Tenants.Common.Entities;
using Timespace.Api.Application.Features.Users.Common.Entities;

namespace Timespace.Api.Infrastructure.Services;

public interface IUsageContext
{
    /// <summary>
    /// This property will only be populated if the user authenticated using a session token. This will not be populated when the user authenticated with an api key.
    /// </summary>
    Identity? Identity { get; set; }
    public Tenant? Tenant { get; set; }
    public List<string> Permissions { get; set; }
}

public class UsageContext : IUsageContext
{
    public Identity? Identity { get; set; }
    public Tenant? Tenant { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public static class UsageContextExtensions
{
    public static Identity GetGuaranteedIdentity(this IUsageContext usageContext)
    {
        return usageContext.Identity ?? throw new InvalidOperationException("Current user is not authenticated");
    }
    
    public static Tenant GetGuaranteedTenant(this IUsageContext usageContext)
    {
        return usageContext.Tenant ?? throw new InvalidOperationException("Current user is not authenticated");
    }
}