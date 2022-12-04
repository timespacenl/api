namespace Timespace.Api.Application.Features.Authentication.Login.Common.Entities;

public class LoginFlowSteps
{
    public const string SetCredentials = "set_credentials";
    public const string CompleteMfa = "complete_mfa";
    public const string None = "none";
    
    public static string[] All =
    {
        SetCredentials,
        CompleteMfa,
        None
    };
}