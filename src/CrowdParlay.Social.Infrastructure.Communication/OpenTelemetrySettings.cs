using System.ComponentModel.DataAnnotations;

namespace CrowdParlay.Social.Infrastructure.Communication;

public record OpenTelemetrySettings
{
    [Required] public required string ServiceName { get; init; }
    [Required] public required string OtlpEndpoint { get; init; }
    [Required] public required string MeterName { get; init; }
    [Required] public required string AdditionalTraceSources { get; init; }
}