using MediatR;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

static class RegistrationFlowTestHelpers
{
    public static async Task<CompleteRegistrationFlow.Response> RegisterUserAsync(ISender sender, string email)
    {
        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email
        });

        await sender.Send(new SetPersonalInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetPersonalInformation.CommandBody
            {
                FirstName = "Test",
                LastName = "Test",
                PhoneNumber = "06 12345678"
            }
        });

        await sender.Send(new SetCompanyInformation.Command
        {
            FlowId = flow.FlowId,
            Body = new SetCompanyInformation.CommandBody
            {
                CompanyName = "Test",
                CompanySize = 1,
                Industry = "Test"
            }
        });

        return await sender.Send(new CompleteRegistrationFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteRegistrationFlow.CommandBody
            {
                AcceptTerms = true,
                MagicLink = false,
                Password = "Test1234!"
            }
        });
    }
}