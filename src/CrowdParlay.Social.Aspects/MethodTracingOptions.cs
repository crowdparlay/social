using Metalama.Framework.Aspects;

namespace CrowdParlay.Social.Aspects;

[CompileTime]
internal class MethodTracingOptions
{
    public string? ActivityName;
    public TraceKind? TraceKind;
    public readonly HashSet<string> IgnoredParameters = new();

    public void Fallback(MethodTracingOptions other)
    {
        ActivityName ??= other.ActivityName;
        TraceKind ??= other.TraceKind;
        IgnoredParameters.UnionWith(other.IgnoredParameters);
    }
}