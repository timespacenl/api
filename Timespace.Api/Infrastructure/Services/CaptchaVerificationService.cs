using Timespace.Api.Infrastructure.Configuration;

namespace Timespace.Api.Infrastructure.Services;

public class CaptchaVerificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CaptchaVerificationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    
    public async Task<bool> Verify(string token)
    {
        if(_configuration.GetValue<bool>(ConfigurationKeys.IntegrationTestingMode))
            return true;

        return true;
    }
}