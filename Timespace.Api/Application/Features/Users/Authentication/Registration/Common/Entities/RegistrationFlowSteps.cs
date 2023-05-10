namespace Timespace.Api.Application.Features.Users.Authentication.Registration.Common.Entities;

public class RegistrationFlowSteps
{
    public const string SetEmail = "email";
    public const string SetPersonalInformation = "personal_information";
    public const string SetCompanyInformation = "company_information";
    public const string CompleteRegistrationFlow = "complete";
    public const string None = "none";
    
    public static string[] All =
    {
        SetEmail,
        SetPersonalInformation,
        SetCompanyInformation,
        CompleteRegistrationFlow,
        None
    };
}