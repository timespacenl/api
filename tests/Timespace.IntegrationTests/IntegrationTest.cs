using Microsoft.Extensions.DependencyInjection;

namespace Timespace.IntegrationTests;

[Collection("SharedFixture")]
public abstract class IntegrationTest : IDisposable, IAsyncLifetime
{
	private readonly IServiceScope _serviceScope;

	protected IntegrationTest(SharedFixture sharedFixture)
	{
		SharedFixture = sharedFixture;
		_serviceScope = SharedFixture.Services.CreateScope();
	}

	protected SharedFixture SharedFixture { get; }

	public async Task InitializeAsync()
	{
		await SharedFixture.ResetDatabase();
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
	}

	public void Dispose()
	{
	}

	protected void GetService<T>(out T service)
		where T : notnull
	{
		service = _serviceScope.ServiceProvider.GetRequiredService<T>();
	}
}
