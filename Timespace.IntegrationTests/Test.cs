using Microsoft.Extensions.Configuration;

namespace Timespace.IntegrationTests;

public class Test : IntegrationTest
{
    [Fact]
    public async Task Test1()
    {
        GetService(out IConfiguration configuration);
        var connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public Test(SharedFixture sharedFixture) : base(sharedFixture)
    {
    }
}