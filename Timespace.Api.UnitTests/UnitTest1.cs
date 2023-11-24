using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.NewTypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.Modules.Employees.Common;
using Timespace.Api.Application.Features.Modules.Employees.Queries;

namespace Timespace.Api.UnitTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var members = (typeof(GetEmployee.Query)).GetGeneratableMembersFromType("SharedType");
        
        Assert.True(true);
    }
}