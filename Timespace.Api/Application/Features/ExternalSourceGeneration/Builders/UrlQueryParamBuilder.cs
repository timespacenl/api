using System.Text;

namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Builders;

public class UrlQueryParamBuilder
{
    private readonly IList<KeyValuePair<string, string>> _params;
    
    public UrlQueryParamBuilder()
    {
        _params = new List<KeyValuePair<string, string>>();
    }

    public UrlQueryParamBuilder(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        _params = new List<KeyValuePair<string, string>>(parameters);
    }

    public void Add(string key, string value)
    {
        _params.Add(new KeyValuePair<string, string>(key, value));
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        bool first = true;
        for (var i = 0; i < _params.Count; i++)
        {
            var pair = _params[i];
            builder.Append(first ? '?' : '&');
            first = false;
            builder.Append(pair.Key);
            builder.Append('=');
            builder.Append(pair.Value);
        }

        return builder.ToString();
    }
}