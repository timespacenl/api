namespace Timespace.TypescriptGenerators.Helpers;

/// <summary>
///     A compilation object for the Timespace.Api project is passed in through dependency injection
/// </summary>
public interface IExternalSourceGenerator
{
	public void Execute();
}
