using Microsoft.Extensions.DependencyInjection;

namespace Timespace.IntegrationTests;

[Collection("SharedFixture")]
public abstract class IntegrationTest : IDisposable, IAsyncLifetime
{
    private readonly IServiceScope _serviceScope;
    
    protected SharedFixture SharedFixture { get; }

    protected IntegrationTest(SharedFixture sharedFixture)
    {
        SharedFixture = sharedFixture;
        _serviceScope = SharedFixture.Services.CreateScope();
    }

    public async Task InitializeAsync() 
    {
        await SharedFixture.ResetDatabase();
    }
    
    protected void GetService<T>(out T service)
        where T : notnull
        => service = _serviceScope.ServiceProvider.GetRequiredService<T>();

    public Task DisposeAsync() => Task.CompletedTask;
    
    public void Dispose()
    {
    }
}