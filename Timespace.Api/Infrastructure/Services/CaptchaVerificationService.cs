using Timespace.Api.Infrastructure.Configuration;

namespace Timespace.Api.Infrastructure.Services;

public interface ICaptchaVerificationService
{
    public Task<bool> VerifyAsync(string captchaResponse, string remoteIp);
}

public class CaptchaVerificationService : ICaptchaVerificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public CaptchaVerificationService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<bool> VerifyAsync(string captchaResponse, string remoteIp)
    {
        if(_configuration.GetValue<bool>(ConfigurationKeys.IntegrationTestingMode) || _environment.IsDevelopment())
            return true;

        return true;
    }
}