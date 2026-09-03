namespace OpenCallMesh.Audio.Abstractions;

public sealed record AudioCapabilitySet(IReadOnlySet<string> Capabilities)
{
    public static AudioCapabilitySet Empty { get; } = new(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
}
