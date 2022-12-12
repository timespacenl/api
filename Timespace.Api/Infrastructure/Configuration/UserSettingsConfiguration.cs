namespace Timespace.Api.Infrastructure.Configuration;

public class UserSettingsConfiguration
{
    public const string SectionName = "UserSettings";
    
    public int MfaSetupFlowExpirationInMinutes { get; set; }
}