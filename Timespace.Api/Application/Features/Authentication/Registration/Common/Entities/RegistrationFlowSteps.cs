namespace Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;

public class RegistrationFlowSteps
{
    public const string SetEmail = "email";
    public const string SetPersonalInformation = "personal_information";
    public const string SetCompanyInformation = "company_information";
    public const string SetCredentials = "credentials";
    public const string None = "none";
    
    public static string[] All =
    {
        SetEmail,
        SetPersonalInformation,
        SetCompanyInformation,
        SetCredentials,
        None
    };
}