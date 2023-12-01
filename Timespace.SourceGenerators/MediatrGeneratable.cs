using System.Collections.Generic;
using Timespace.SourceGenerators.Helpers;

namespace Timespace.SourceGenerators;

public readonly record struct MediatrGeneratable
{
    public string WrapperClassName { get; init; }
    public string Fqns { get; init; }
    public string RequestTypeName { get; init; }
    public string RequestTypeNameShort { get; init; }
    public string ResponseTypeName { get; init; }
    public EquatableArray<(string TypeName, string ParameterName)> Dependencies { get; init; }
    public bool HasDependencies => Dependencies.Count > 0;
    
    public MediatrGeneratable(string wrapperClassName, string fqns, List<(string, string)> handleMethodParameters, string requestTypeName, string responseTypeName, string requestTypeNameShort)
    {
        WrapperClassName = wrapperClassName;
        Fqns = fqns;
        Dependencies = new EquatableArray<(string, string)>(handleMethodParameters.ToArray());
        RequestTypeName = requestTypeName;
        RequestTypeNameShort = requestTypeNameShort;
        ResponseTypeName = responseTypeName;
    }
}