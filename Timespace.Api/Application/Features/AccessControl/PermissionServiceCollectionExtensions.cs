using System.Reflection;

namespace Timespace.Api.Application.Features.AccessControl;

public static class PermissionServiceCollectionExtensions
{
    public static void AddPermissionTree(this IServiceCollection services)
    {
        var allPermissions = DiscoverPermissions();
        var tenantScopedPermissions = DiscoverPermissions(PermissionScope.Tenant);
        var departmentScopedPermissions = DiscoverPermissions(PermissionScope.Department);

        var collections = new PermissionCollections()
        {
            All = allPermissions,
            AllTenantScoped = tenantScopedPermissions,
            AllDepartmentScoped = departmentScopedPermissions
        };

        services.AddSingleton(collections);
    }

    private static PermissionTree DiscoverPermissions(PermissionScope? scope = null)
    {
        var permissionTree = new PermissionTree();
        var permissionRootTypes = typeof(Application.Features.AccessControl.Permissions).GetNestedTypes();

        foreach (var rootType in permissionRootTypes)
        {
            var properties = rootType.GetFields();

            if (rootType.GetCustomAttribute<PermissionGroupAttribute>()?.Scope! != scope && scope != null)
            {
                continue;
            }
            
            var treeNode = new PermissionTreeNode()
            {
                GroupName = rootType.GetCustomAttribute<PermissionGroupAttribute>()?.GroupCode!,
                Permissions = properties.Select(x => (x.GetValue(null) as string)).ToList(),
                Children = new List<PermissionTreeNode>()
            };

            treeNode = DiscoverChildNodes(rootType, treeNode);
            permissionTree.Nodes.Add(treeNode);
        }

        return permissionTree;
    }

    private static PermissionTreeNode DiscoverChildNodes(Type rootType, PermissionTreeNode currentNode)
    {
        var childTypes = rootType.GetNestedTypes();
        var childNodes = new List<PermissionTreeNode>();
        
        if (childTypes.Length > 0)
        {
            foreach (var childType in childTypes)
            {
                var properties = childType.GetFields();

                var treeNode = new PermissionTreeNode()
                {
                    GroupName = childType.GetCustomAttribute<PermissionGroupAttribute>()?.GroupCode!,
                    Permissions = properties.Select(x => x.GetValue(null) as string).ToList(),
                    Children = new List<PermissionTreeNode>()
                };

                treeNode = DiscoverChildNodes(childType, treeNode);
                
                childNodes.Add(treeNode);
            }
        }
        
        currentNode.Children = childNodes;
        
        return currentNode;
    }
}