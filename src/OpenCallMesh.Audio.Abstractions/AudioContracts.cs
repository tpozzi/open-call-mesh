namespace OpenCallMesh.Audio.Abstractions;

public sealed record AudioCapabilitySet(IReadOnlySet<string> Capabilities)
{
    public static AudioCapabilitySet Empty { get; } = new(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
}

public sealed record AudioCaptureMeter(
    long Frames,
    double Peak,
    double Rms,
    int SampleRate,
    int Channels,
    long DroppedFrames,
    TimeSpan Duration);

public interface IProcessAudioCapture : IAsyncDisposable
{
    Task StartAsync(int processId, CancellationToken cancellationToken = default);
    event EventHandler<AudioCaptureMeter>? MeterUpdated;
}
