using System.Collections.Generic;

namespace Timespace.SourceGenerators;

public record struct MediatrGeneratable
{
    public string WrapperClassName { get; init; }
    public string Fqns { get; init; }
    public string RequestTypeName { get; init; }
    public string ResponseTypeName { get; init; }
    public EquatableArray<(string TypeName, string ParameterName)> Dependencies { get; init; }
    
    public MediatrGeneratable(string wrapperClassName, string fqns, List<(string, string)> handleMethodParameters, string requestTypeName, string responseTypeName)
    {
        WrapperClassName = wrapperClassName;
        Fqns = fqns;
        Dependencies = new EquatableArray<(string, string)>(handleMethodParameters.ToArray());
        RequestTypeName = requestTypeName;
        ResponseTypeName = responseTypeName;
    }
}