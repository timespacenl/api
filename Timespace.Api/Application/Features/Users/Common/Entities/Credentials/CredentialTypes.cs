namespace Timespace.Api.Application.Features.Users.Common.Entities.Credentials;

public class CredentialTypes
{
    public const string Password = "password";
    public const string Totp = "totp";
    public const string LookupSecret = "lookup_secret";
    public const string MagicLink = "magic_link";
    // public static readonly string Google = "google";
    // public static readonly string Microsoft = "microsoft";
    
    public static string[] All { get; } = { Password, /*Google, Microsoft,*/ Totp, LookupSecret, MagicLink };
    public static string[] AllFirstFactor { get; } = { Password, /*Google, Microsoft,*/ MagicLink };
    public static string[] AllSecondFactor { get; } = { Totp, LookupSecret };
}

public enum CredentialTypesEnum
{
    Password = 1,
    Totp = 2,
    LookupSecret = 3,
    MagicLink = 4,
    // Google = 5,
    // Microsoft = 6
}

public enum FirstFactorCredentialTypes
{
    Password = CredentialTypesEnum.Password,
    // Google = CredentialTypesEnum.Google,
    // Microsoft = CredentialTypesEnum.Microsoft,
    MagicLink = CredentialTypesEnum.MagicLink
}

public enum SecondFactorCredentialTypes
{
    Totp = CredentialTypesEnum.Totp,
    LookupSecret = CredentialTypesEnum.LookupSecret
}