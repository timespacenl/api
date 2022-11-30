﻿using FluentAssertions;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Exceptions;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

using static RegistrationFlowTestHelpers;

public class FlowNotFoundTests : BaseTestFixture
{
    [Test]
    public async Task SetPersonalInformation_ShouldThrow_WithInvalidFlowId()
    {
        await FluentActions.Invoking(() => SetPersonalInformation(Guid.NewGuid()))
            .Should()
            .ThrowAsync<FlowNotFoundException>();
    }
    
    [Test]
    public async Task SetCompanyInformation_ShouldThrow_WithInvalidFlowId()
    {
        await FluentActions.Invoking(() => SetCompanyInformation(Guid.NewGuid()))
            .Should()
            .ThrowAsync<FlowNotFoundException>();
    }
    
    [Test]
    public async Task SetCredentials_ShouldThrow_WithInvalidFlowId()
    {
        await FluentActions.Invoking(() => SetCredentials(Guid.NewGuid()))
            .Should()
            .ThrowAsync<FlowNotFoundException>();
    }
}