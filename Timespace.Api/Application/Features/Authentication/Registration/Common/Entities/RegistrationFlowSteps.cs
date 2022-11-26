namespace Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;

public class RegistrationFlowSteps
{
    public const string Email = "email";
    public const string PersonalInformation = "personal_information";
    public const string CompanyInformation = "company_information";
    public const string Credentials = "credentials";
    
    public static string[] All = new[]
    {
        Email,
        PersonalInformation,
        CompanyInformation,
        Credentials
    };
}