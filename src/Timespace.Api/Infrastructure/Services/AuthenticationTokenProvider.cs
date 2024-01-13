namespace Timespace.Api.Infrastructure.Services;

public interface IAuthenticationTokenProvider
{
    public string? AuthenticationToken { get; set;  }
    public AuthenticationTokenType? AuthenticationTokenType { get; set; }
}

public enum AuthenticationTokenType
{
    ApiKey,
    UserSession
}

public class AuthenticationTokenProvider : IAuthenticationTokenProvider
{
    public string? AuthenticationToken { get; set; }
    public AuthenticationTokenType? AuthenticationTokenType { get; set; }
}