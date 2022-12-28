using System.Reflection;
using FluentAssertions;
using Timespace.Api.Infrastructure.AccessControl;

namespace Timespace.UnitTests.Permissions;

public class NoConflictingPermissionsScopesTests
{
    private PermissionScope _currentScope;
    
    [Test]
    public async Task NoConflictingPermissionScopes()
    {
        var rootTypes = typeof(Api.Infrastructure.AccessControl.Permissions).GetNestedTypes();

        foreach (var rootType in rootTypes)
        {
            rootType.Should().BeDecoratedWith<PermissionGroupAttribute>("Permission group is missing from {0}", rootType.Name);
            var permissionScope = rootType.GetCustomAttribute<PermissionGroupAttribute>()!.Scope;
            
            _currentScope = permissionScope;
            
            var children = DiscoverChildren(rootType);
        }
    }

    private bool DiscoverChildren(Type currentType)
    {
        var nestedTypes = currentType.GetNestedTypes();

        foreach (var nestedType in nestedTypes)
        {
            nestedType.Should().BeDecoratedWith<PermissionGroupAttribute>();
            var permissionScope = nestedType.GetCustomAttribute<PermissionGroupAttribute>()!.Scope;
            
            permissionScope.Should().Be(_currentScope, "Permission scope is different from parent {0}", currentType.Name);

            var children = DiscoverChildren(nestedType);
        }
        
        return true;
    }
}