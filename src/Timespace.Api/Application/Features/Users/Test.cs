using Timespace.Api.Application.Features.AccessControl;

namespace Timespace.Api.Application.Features.Users;

[PermissionGroup(Parent = typeof(SomeParentPermissionGroup))]
public class TestPermissions
{
    public string TestPermission1 { get; set; } = "test:permission1";
}

[Pipeline]
public class TestPipeline<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public void Before(TRequest request)
    {
        
    }
    
    public void After(TResponse request)
    {
        
    }
}

public class Behaviours : Attribute
{
    public Behaviours(params Type[] behaviourTypes)
    {
        BehaviourTypes = behaviourTypes;
    }
    
    public Type[] BehaviourTypes { get; set; }
}
