﻿using OtpNet;
using Timespace.Api.Application.Features.Authentication.Login.Commands;
using Timespace.Api.Application.Features.Authentication.Login.Common;
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

    public static async Task ConfigureMfa()
    {
        var flow = await SendAsync(new CreateMfaSetupFlow.Command());

        var otp = new Totp(Base32Encoding.ToBytes(flow.Secret));
        var code = otp.ComputeTotp();

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