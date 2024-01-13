using Xunit.Abstractions;

namespace Timespace.TypescriptGenerators.Tests;

public class XUnitLogger<T> : ILogger<T>, IDisposable
{
	private readonly ITestOutputHelper _output;

	public XUnitLogger(ITestOutputHelper output)
	{
		_output = output;
	}

	public void Dispose()
	{
	}
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
	{
		_output.WriteLine(state.ToString());
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return true;
	}

	public IDisposable BeginScope<TState>(TState state)
	{
		return this;
	}
}
