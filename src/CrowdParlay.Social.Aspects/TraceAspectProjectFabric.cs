using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

namespace CrowdParlay.Social.Aspects;

public class TraceAspectProjectFabric : TransitiveProjectFabric
{
    public override void AmendProject(IProjectAmender amender)
    {
        var implicitlyTracedMethods = amender
            .SelectDeclarationsWithAttribute<TraceMethodsAttribute>()
            .OfType<INamedType>()
            .SelectMany(type =>
            {
                var typeAttribute = type.Attributes.GetConstructedAttributesOfType<TraceMethodsAttribute>().Single();
                return type.Methods.Where(method =>
                    method.HasImplementation
                    && typeAttribute.Accessibility.Contains(method.Accessibility)
                    && !method.Attributes.OfAttributeType(typeof(TraceAttribute)).Any()
                    && !method.Attributes.OfAttributeType(typeof(TraceIgnoreAttribute)).Any());
            });

        implicitlyTracedMethods.AddAspect(method =>
        {
            var declaringTypeAttribute = method.DeclaringType.Attributes.GetConstructedAttributesOfType<TraceMethodsAttribute>().Single();
            return new TraceAttribute(declaringTypeAttribute.MethodTracingOptions);
        });
    }
}