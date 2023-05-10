using Timespace.Api.Infrastructure.Errors;

namespace Timespace.Api.Application.Features.Users.Authentication.Login.Exceptions;

public class TotpInvalidException : Exception, IBaseException
{
    public TotpInvalidException(int triesRemaining)
    {
        TriesRemaining = triesRemaining;
    }

    public string Type => "totp_invalid";
    public int StatusCode => StatusCodes.Status400BadRequest;
    public string Title => "Totp is invalid";
    public string? Detail => null;
    private int TriesRemaining { get; }
    public Dictionary<string, object?> MapExtensions() => new()
    {
        { "triesRemaining", TriesRemaining }
    };
}