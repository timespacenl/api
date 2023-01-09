using OtpNet;
using Timespace.Api.Application.Features.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Authentication.Login.Common;
using Timespace.Api.Application.Features.Users.Common.Entities.Credentials;
using Timespace.Api.Application.Features.Users.Settings.Mfa.Commands;

namespace Timespace.IntegrationTests.Features.Authentication.Login;

public static class LoginFlowTestHelpers
{
    public static async Task<ILoginFlowResponse> CreateLoginFlow(string email)
    {
        return await SendAsync(new CreateLoginFlow.Command
        {
            Email = email
        });
    }

    public static async Task<ILoginFlowResponse> SetCredentialsForFlow(Guid flowId, string credentialType = CredentialTypes.Password, string credentialValue = Constants.DefaultUserPassword)
    {
        return await SendAsync(new SetLoginFlowCredentials.Command
        {
            FlowId = flowId,
            Body = new SetLoginFlowCredentials.CommandBody
            {
                CredentialType = credentialType,
                CredentialValue = credentialValue
            }
        });
    }

    public static async Task<ILoginFlowResponse> CompleteTotpMfaForFlow(Guid flowId, string totp)
    {
        return await SendAsync(new CompleteLoginFlowMfa.Command
        {
            FlowId = flowId,
            Body = new CompleteLoginFlowMfa.CommandBody
            {
                CredentialType = CredentialTypes.Totp,
                CredentialValue = totp
            }
        });
    }

    public static async Task ConfigureMfa()
    {
        var flow = await SendAsync(new CreateMfaSetupFlow.Command());

        CurrentMfaSecret = flow.Secret;

        var clock = GetClock();
        
        var otp = new Totp(Base32Encoding.ToBytes(flow.Secret));
        var code = otp.ComputeTotp(clock.GetCurrentInstant().ToDateTimeUtc());

        await SendAsync(new CompleteMfaSetupFlow.Command
        {
            FlowId = flow.FlowId,
            Body = new CompleteMfaSetupFlow.CommandBody
            {
                TotpCode = code
            }
        });
    }
}