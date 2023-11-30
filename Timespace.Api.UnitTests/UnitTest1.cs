using Microsoft.JSInterop.Implementation;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.Extensions;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Generators.TypescriptApiClientGenerator.TsGenerators;
using Timespace.Api.Application.Features.ExternalSourceGeneration.Types;
using Timespace.Api.Application.Features.Modules.Employees.Common;
using Timespace.Api.Application.Features.Modules.Employees.Queries;

namespace Timespace.Api.UnitTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var props = typeof(PaginatedResult<GetEmployee.Response>).GetProperties();
        var members = (typeof(PaginatedResult<GetEmployee.Response>)).GetGeneratableMembersFromType("SharedType", returnMemberOfMembers: true);
        var obj = new GeneratableObject()
        {
            Name = "GetEmployeeResponse",
            Members = members,
            ObjectType = typeof(PaginatedResult<GetEmployee.Response>)
        };
        
        ITypescriptSourceBuilder responseBuilder = new TypescriptInterfaceSourceBuilder().Initialize(obj.Name);
        
        var responseInterfaces = ApiClientSourceBuilder<>.GenerateFromGeneratableObject(obj, responseBuilder, new());
        
        Assert.True(true);
    }
}