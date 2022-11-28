namespace Timespace.Api.Infrastructure.Errors;

public interface IBaseException
{
    public string Type { get; }
    public int StatusCode { get; }
    public string Title { get; }
    public string? Detail { get; }
    public Dictionary<string, object?> MapExtensions();
}