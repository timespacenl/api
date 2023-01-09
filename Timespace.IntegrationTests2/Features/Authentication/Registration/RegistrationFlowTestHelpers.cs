using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Registration.Common;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

public static class RegistrationFlowTestHelpers
{
    private static int _callCounter = 0;
    
    public static async Task<IRegistrationFlowResponse> SetEmail(bool count = true)
    {
        if(count)
            _callCounter++;
        
        return await SendAsync(new CreateRegistrationFlow.Command
        {
            Email = $"duke{_callCounter}@steenbakkers.cc"
        });
    }

    public static async Task<IRegistrationFlowResponse> SetPersonalInformation(Guid flowId)
    {
        return await SendAsync(new SetPersonalInformation.Command
        {
            FlowId = flowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = null
            }
        });
    }

    public static async Task<IRegistrationFlowResponse> SetCompanyInformation(Guid flowId)
    {
        return await SendAsync(new SetCompanyInformation.Command
        {
            FlowId = flowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                Industry = "Something",
                CompanySize = 10
            }
        });
    }

    public static async Task CompleteRegistrationFlow(Guid flowId)
    {
        await SendAsync(new CompleteRegistrationFlow.Command
        {
            FlowId = flowId,
            Body = new CompleteRegistrationFlow.CommandBody
            {
                Password = "Test",
                AcceptTerms = true,
                MagicLink = false
            }
        });
    }
}