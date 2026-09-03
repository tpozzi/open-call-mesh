namespace OpenCallMesh.Diagnostics;

public sealed record HealthSnapshot(string Component, string State, DateTimeOffset At, IReadOnlyDictionary<string, double> Metrics)
{
    public static HealthSnapshot Online(string component) => new(component, "ONLINE", DateTimeOffset.UtcNow, new Dictionary<string, double>());
}
