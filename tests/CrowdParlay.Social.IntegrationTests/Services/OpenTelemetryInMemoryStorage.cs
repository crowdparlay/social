using System.Diagnostics;
using OpenTelemetry.Metrics;

namespace CrowdParlay.Social.IntegrationTests.Services;

public class OpenTelemetryInMemoryStorage
{
    public readonly List<Activity> Activities = new();
    public readonly List<Metric> Metrics = new();
}