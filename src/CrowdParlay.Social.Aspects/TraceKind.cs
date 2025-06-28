using Metalama.Framework.Aspects;
using System.Diagnostics;

namespace CrowdParlay.Social.Aspects;

/// <summary>Compile-time version of <see cref="ActivityKind"/></summary>
[RunTimeOrCompileTime]
public enum TraceKind
{
    /// <summary>Internal operation within an application, as opposed to operations with remote parents or children. This is the default value.</summary>
    Internal,

    /// <summary>Requests incoming from external component.</summary>
    Server,

    /// <summary>Outgoing request to the external component.</summary>
    Client,

    /// <summary>Output provided to external components.</summary>
    Producer,

    /// <summary>Output received from an external component.</summary>
    Consumer
}