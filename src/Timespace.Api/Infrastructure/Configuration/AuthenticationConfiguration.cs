namespace Timespace.Api.Infrastructure.Configuration;

public class AuthenticationConfiguration
{
    public const string SectionName = "AuthenticationSettings";
    
    public int RegistrationFlowTimeoutMinutes { get; set; }
    public int LoginFlowTimeoutMinutes { get; set; }
    public int SessionCookieExpirationDays { get; set; }
    public string SessionCookieName { get; set; } = null!;
    public string ApiKeyHeaderName { get; set; } = null!;
    public int VerificationTokenTimeoutMinutes { get; set; }
}