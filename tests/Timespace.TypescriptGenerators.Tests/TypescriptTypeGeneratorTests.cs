using System.Reflection;
using Castle.Core.Logging;
using NSubstitute;

namespace Timespace.TypescriptGenerators.Tests;

public class TypescriptTypeGeneratorTests
{
	[Fact]
	public void Test1()
	{
		var source = $$"""
                      using NodaTime;
                      {{SourceConstants.AspNetAttributes}}
                      {{SourceConstants.SharedType}}
                      {{SourceConstants.NodaTimeTypeMocks}}
                      {{SourceConstants.PaginatedResult}}

                      namespace Test
                      {
                            public class TestClass
                            {
                                public System.Threading.Tasks.Task<GetTest.Response> Test([FromQuery] Command2 command)
                                {
                                    return true;
                                }
                            }
                            
                            [GenerateMediatr]
                            public static class GetTest {
                                public record Command(string Name, string Email, SharedType Query1, SharedType? Query2);
                                public record Response(PaginatedResult<UserDto> Result);
                            }
                      }
                      
                      public record UserDto
                      {
                          public string Name { get; init; } = null!;
                          public string Email { get; init; } = null!;
                      }
                      
                      public record Command2(
                          [property: FromQuery(Name = "name")] string Name,
                          [property: FromQuery(Name = "prop2")] string Prop2,
                          [property: FromRoute(Name = "employeeId")] int EmployeeId,
                          [property: FromBody] CommandBody Body
                          );
                      
                      public record CommandQuery(
                          [property: FromQuery(Name = "name")] string Name,
                          [property: FromQuery(Name = "prop2")] string Prop2
                          );
                      
                      public record CommandBody(
                          string Email,
                          string Password
                          TestEnum TestEnum
                          );
                      
                      public enum TestEnum
                      {
                          Test1 = TestEnum2.Test2,
                          Test2
                      }
                      
                      public enum TestEnum2
                      {
                          Test1,
                          Test2
                      }
                      """;

		var compilation = CreateCompilation(source);
		var logger = Substitute.For<ILogger>();
		var options = Substitute.For<IOptions<ExternalSourceGenerationSettings>>();

		// SUT
		var generator = new TypescriptMappingGenerator(compilation, logger, new(), new List<EndpointDescription>());

		var endpointDescription = new EndpointDescription
		{
			ActionName = "Test",
			ControllerTypeName = "Test.TestClass",
			Version = "v1",
			HttpMethod = "GET",
			RelativePath = "v1/employees/{employeeId}",
		};

		var endpoint = generator.TransformEndpointDescription(endpointDescription);
	}

	private static Compilation CreateCompilation(string source)
	{
		return CSharpCompilation.Create("compilation",
			new[]
			{
				CSharpSyntaxTree.ParseText(source),
			},
			new[]
			{
				MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
			},
			new CSharpCompilationOptions(OutputKind.ConsoleApplication));
	}
}
