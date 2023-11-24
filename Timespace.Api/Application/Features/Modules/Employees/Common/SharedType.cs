namespace Timespace.Api.Application.Features.Modules.Employees.Common;

public record SharedType(string Prop1, string Prop2, NestedSharedType NestedSharedType);

public record NestedSharedType(string Prop1, string Prop2, SharedType SharedType);
