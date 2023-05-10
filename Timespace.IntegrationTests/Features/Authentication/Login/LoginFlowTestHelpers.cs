using MediatR;
using NodaTime;
using OtpNet;
using Timespace.Api.Application.Features.Users.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Commands;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

public static class LoginFlowTestHelpers
{
    public static async Task<CompleteRegistrationFlow.Response> RegisterUserAsync(ISender sender, string email, string password = "Test1234!")
    {
        var flow = await sender.Send(new CreateRegistrationFlow.Command
        {
            Email = email,
            CaptchaToken = "irrelevant"
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
                Password = password
            }
        });
    }

    public static async Task<string> ConfigureMfa(ISender sender, IClock clock)
    {
        var flow = await sender.Send(new CreateMfaSetupFlow.Command());

        var otp = new Totp(Base32Encoding.ToBytes(flow.Secret));
        var code = otp.ComputeTotp(clock.GetCurrentInstant().ToDateTimeUtc());

        await sender.Send(new CompleteMfaSetupFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteMfaSetupFlow.CommandBody
            {
                TotpCode = code
            }
        });

        return flow.Secret;
    }
}