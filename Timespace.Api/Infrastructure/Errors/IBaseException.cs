namespace Timespace.Api.Infrastructure.Errors;

public interface IBaseException
{
    public string Type => "internal-server-error";
    public int StatusCode => StatusCodes.Status500InternalServerError;
    public string Title => "Internal Server Error";
    public string? Detail => null;
    public Dictionary<string, object?> Extensions => new();
}

public class TestException : Exception, IBaseException
{
    public TestException(string extension)
    {
        Extensions["test"] = extension;
    }
    
    public string Type => "test-type";
    public int StatusCode => StatusCodes.Status403Forbidden;
    public string Title => "Test title";
    public string? Detail => null;
    public Dictionary<string, object?> Extensions { get; } = new();
}