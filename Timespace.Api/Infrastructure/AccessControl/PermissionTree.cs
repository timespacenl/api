namespace Timespace.Api.Infrastructure.AccessControl;

public class PermissionTreeNode
{
    public string GroupName { get; set; } = null!;
    public List<string?> Permissions { get; set; } = null!;
    public List<PermissionTreeNode> Children { get; set; } = null!;
}

public class PermissionTree
{
    public List<PermissionTreeNode> Nodes { get; set; } = new();
}