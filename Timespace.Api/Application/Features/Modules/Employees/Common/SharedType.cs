namespace Timespace.Api.Application.Features.Modules.Employees.Common;

public record SharedType(string Prop1, string Prop2, List<SharedType>? SharedType2, List<SharedType?> SharedType3, NestedSharedType NestedSharedType, Instant? Test, LocalDate Test2);

public record NestedSharedType(string Prop1, string Prop2);

public record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
