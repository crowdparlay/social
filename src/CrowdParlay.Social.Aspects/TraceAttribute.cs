using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace CrowdParlay.Social.Aspects;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
[AttributeUsage(AttributeTargets.Method), CompileTime]
public sealed class TraceAttribute : OverrideMethodAspect
{
    internal MethodTracingOptions Options = new();

    public TraceAttribute() { }
    internal TraceAttribute(MethodTracingOptions options) => Options = options;
    public TraceAttribute(string activityName) => Options.ActivityName = activityName;
    public TraceAttribute(TraceKind traceKind) => Options.TraceKind = traceKind;

    public TraceAttribute(string activityName, TraceKind traceKind)
    {
        Options.ActivityName = activityName;
        Options.TraceKind = traceKind;
    }

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        var typeAttribute = builder.Target.DeclaringType.Attributes.GetConstructedAttributesOfType<TraceMethodsAttribute>().SingleOrDefault();
        if (typeAttribute is not null)
            Options.Fallback(typeAttribute.MethodTracingOptions);

        var ignoredParameters = builder.Target.Parameters
            .Where(parameter => parameter.Attributes.OfAttributeType(typeof(TraceIgnoreAttribute)).Any())
            .Select(parameter => parameter.Name);

        Options.IgnoredParameters.UnionWith(ignoredParameters);
        base.BuildAspect(builder);
    }

    public override dynamic? OverrideMethod()
    {
        var activitySource = new ActivitySource(meta.Target.Method.DeclaringType.FullName);
        var activity = activitySource.CreateActivity(
            Options.ActivityName ?? meta.Target.Method.Name,
            (ActivityKind?)Options.TraceKind ?? ActivityKind.Internal)!;

        var parametersForSerialization = meta.Target.Parameters.Where(parameter => !Options.IgnoredParameters.Contains(parameter.Name));
        var options = new JsonSerializerOptions { MaxDepth = 3 };
        foreach (var parameter in parametersForSerialization)
        {
            activity.AddTag($"parameters.{parameter.Name}.presence","YES" );
            try
            {
                var serializedValue = JsonSerializer.Serialize(parameter.Value, options);
                activity.AddTag($"parameters.{parameter.Name}", serializedValue);
            }
            catch
            {
                // Parameters of unserializable types (e.g. CancellationToken having nested IntPtr property) are ignored
            }
        }

        using (activity.Start())
        {
            try
            {
                return meta.Proceed();
            }
            catch (Exception exception)
            {
                activity.AddException(exception);
                throw;
            }
        }
    }
}