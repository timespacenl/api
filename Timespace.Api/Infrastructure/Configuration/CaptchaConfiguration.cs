namespace Timespace.Api.Infrastructure.Configuration;

public class CaptchaConfiguration
{
    public const string SectionName = "CaptchaSettings";
    
    public string Secret { get; set; } = null!;
}