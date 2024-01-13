namespace Timespace.TypescriptGenerators.Tests;

public static class SourceConstants
{
	public static readonly string AspNetAttributes =
		"""
     [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
     public class FromQueryAttribute :
       System.Attribute
     {
       public string? Name { get; set; }
     }

     [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
     public class FromRouteAttribute :
       System.Attribute
     {
       public string? Name { get; set; }
     }

     [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
     public class FromBodyAttribute :
       System.Attribute
     {
     }

     [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
     public class FromFormAttribute :
       System.Attribute
     {
       public string? Name { get; set; }
     }
     
     [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
     public class GenerateMediatrAttribute : System.Attribute
     {
     }
     """;

	public static readonly string SharedType =
		"""
    public record SharedType(string Prop1, string Prop2, System.Collections.Generic.List<SharedType>? SharedType2, System.Collections.Generic.List<SharedType?> SharedType3, NestedSharedType NestedSharedType, Instant? Test, LocalDate Test2);

    public record NestedSharedType(string Prop1, string Prop2);
    """;

	public static readonly string NodaTimeTypeMocks =
		"""
    namespace NodaTime {
      public struct Instant
      {
      }

      public struct LocalDate
      {
      }
    }
    """;

	public static readonly string PaginatedResult =
		"""
    public record PaginatedResult<T>(System.Collections.Generic.List<T> Items, int TotalCount, int Page, int PageSize);
    """;
}
