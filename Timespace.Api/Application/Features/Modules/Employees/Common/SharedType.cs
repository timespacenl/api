namespace Timespace.Api.Application.Features.Modules.Employees.Common;

public record SharedType(string Prop1, string Prop2, NestedSharedType NestedSharedType, Test TestEnum, Instant? Test, LocalDate Test2);

public enum Test { Test1, Test2 }

public record NestedSharedType(string Prop1, string Prop2);

public record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
