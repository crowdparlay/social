using System.Diagnostics.CodeAnalysis;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace CrowdParlay.Social.Aspects;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
[AttributeUsage(AttributeTargets.Class), CompileTime]
public class TraceMethodsAttribute : Attribute
{
    internal readonly Accessibility[] Accessibility = [Metalama.Framework.Code.Accessibility.Public];
    internal readonly MethodTracingOptions MethodTracingOptions = new();

    public TraceMethodsAttribute() { }
    public TraceMethodsAttribute(TraceKind traceKind) => MethodTracingOptions.TraceKind = traceKind;
    public TraceMethodsAttribute(params Accessibility[] accessibility) => Accessibility = accessibility;
    
    public TraceMethodsAttribute(TraceKind traceKind, params Accessibility[] accessibility)
    {
        if (!accessibility.Any())
            throw new ArgumentException("At least one accessibility must be specified.", nameof(accessibility));

        MethodTracingOptions.TraceKind = traceKind;
        Accessibility = accessibility;
    }
}