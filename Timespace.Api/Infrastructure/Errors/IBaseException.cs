namespace Timespace.Api.Infrastructure.Errors;

public interface IBaseException
{
    public string Type { get; }
    public int StatusCode { get; }
    public string Title { get; }
    public string? Detail { get; }
    public Dictionary<string, object?> Extensions { get; }
}

public class TestException : Exception, IBaseException
{
    public TestException(string extension)
    {
        Extensions["test"] = extension;
    }
    
    public string Type { get; } = "test-type";
    public int StatusCode { get; } = StatusCodes.Status403Forbidden;
    public string Title { get; } = "Test title";
    public string? Detail { get; } = null;
    public Dictionary<string, object?> Extensions { get; } = new();
}