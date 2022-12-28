namespace Timespace.Api.Application.Features.Permissions.Queries;

public record ResponsePermissionGroup(
    string GroupName,
    List<string> Permissions,
    ResponsePermissionGroup? Child
);