using FluentAssertions;

namespace Timespace.UnitTests.Permissions;

public class NoConflictingPermissionValuesTests
{
    private List<string> _permissionValues = new();

    [Test]
    public void NoConflictingPermissionValues()
    {
        var rootTypes = typeof(Api.Infrastructure.AccessControl.Permissions).GetNestedTypes();

        foreach (var rootType in rootTypes)
        {
            DiscoverChildren(rootType);
        }
        
        var query = _permissionValues.GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(y => new { Element = y.Key, Counter = y.Count() })
            .ToList();
        
        query.Should().BeEmpty("Permission keys must be unique: {0}", string.Join(", ", query.Select(x => x.Element)));
    }
    
    private void DiscoverChildren(Type currentType)
    {
        var nestedTypes = currentType.GetNestedTypes();

        foreach (var nestedType in nestedTypes)
        {
            var fields = nestedType.GetFields();
            var values = fields.Select(x => x.GetValue(null)?.ToString()).ToList();
            
            _permissionValues.AddRange(values!);

            DiscoverChildren(nestedType);
        }
    }
}