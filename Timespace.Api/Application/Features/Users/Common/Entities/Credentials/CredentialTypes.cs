namespace Timespace.Api.Application.Features.Users.Common.Entities.Credentials;

public class CredentialTypes
{
    public static readonly string Password = "password";
    public static readonly string Totp = "totp";
    public static readonly string LookupSecret = "lookup_secret";
    public static readonly string MagicLink = "magic_link";
    // public static readonly string Google = "google";
    // public static readonly string Microsoft = "microsoft";
    
    public static string[] All { get; } = { Password, /*Google, Microsoft,*/ Totp, LookupSecret, MagicLink };
    public static string[] AllFirstFactor { get; } = { Password, /*Google, Microsoft,*/ MagicLink };
    public static string[] AllSecondFactor { get; } = { Totp, LookupSecret };
}