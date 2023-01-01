using Timespace.Api.Application.Features.Authentication.Registration.Commands;

namespace Timespace.IntegrationTests.Testing;

public partial class Testing
{
    public static async Task<string?> CreateUserWithPassword(string email = Constants.DefaultUserEmail, string password = Constants.DefaultUserPassword)
    {
        var flow = await SendAsync(new CreateRegistrationFlow.Command
        {
            Email = email
        });
        
        await SendAsync(new SetPersonalInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = null
            }
        });
        
        await SendAsync(new SetCompanyInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                Industry = "Something",
                CompanySize = 10
            }
        });
        
        var flowResponse = await SendAsync(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody
            {
                Password = password,
                AcceptTerms = true,
                MagicLink = false
            }
        });

        return flowResponse.SessionToken;
    }

    public static void SetUserAuthToken(string? token)
    {
        _currentSessionToken = token;
    }

    public static string? GetUserAuthToken()
    {
        return _currentSessionToken;
    }
}