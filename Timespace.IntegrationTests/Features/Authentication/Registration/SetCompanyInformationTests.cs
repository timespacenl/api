﻿using FluentAssertions;
using Timespace.Api.Application.Features.Authentication.Registration.Commands;
using Timespace.Api.Application.Features.Authentication.Registration.Common.Entities;

namespace Timespace.IntegrationTests.Features.Authentication.Registration;

using static Helpers;

public class SetCompanyInformationTests : BaseTestFixture
{
    [Test]
    public async Task ShouldAdvanceStep()
    {
        // Arrange
        var result = await SetEmail();

        await SetPersonalInformation(result.FlowId);
        
        // Act
        var setCompanyInformationResult = await SetCompanyInformation(result.FlowId);
        
        var dbEntity = await FindAsync<RegistrationFlow>(result.FlowId);
        
        // Assert
        setCompanyInformationResult.Should().NotBeNull();
        setCompanyInformationResult.FlowId.Should().Be(result.FlowId);
        setCompanyInformationResult.NextStep.Should().Be(RegistrationFlowSteps.SetCredentials);
        dbEntity.Should().NotBeNull();
        dbEntity!.CompanyName.Should().NotBeNullOrEmpty();
        dbEntity.CompanyIndustry.Should().NotBeNullOrEmpty();
        dbEntity.CompanySize.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1000);
    }
}