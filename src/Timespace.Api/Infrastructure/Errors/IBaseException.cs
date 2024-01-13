namespace Timespace.Api.Infrastructure.Errors;

internal interface IBaseException
{
    public string Type { get; }
    public int StatusCode { get; }
    public string Title { get; }
    public string? Detail { get; }
    public Dictionary<string, object?> MapExtensions();
}
