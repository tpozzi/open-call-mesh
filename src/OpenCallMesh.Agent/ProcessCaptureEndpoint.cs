using OpenCallMesh.Audio.Windows;
using OpenCallMesh.Core;
using System.Runtime.Versioning;

namespace OpenCallMesh.Agent;

public sealed record ProcessCaptureEndpointOptions(string ProcessName, string EndpointId = "process-audio");

public sealed record ProcessCaptureMetrics(
    string State,
    int ProcessId,
    long RawFramesCaptured,
    long CanonicalFramesProduced,
    long CaptureBytes,
    long DroppedRawFrames,
    long DroppedCanonicalFrames,
    double Peak,
    double Rms,
    int SourceSampleRate,
    int SourceChannels,
    int CanonicalSampleRate,
    int CanonicalChannels);

/// <summary>Agent-facing process capture endpoint. It uses a local statistics sink only.</summary>
[SupportedOSPlatform("windows10.0.19041.0")]
public sealed class ProcessCaptureEndpoint(ProcessCaptureEndpointOptions options)
{
    public async Task<ProcessCaptureMetrics> CaptureAsync(TimeSpan duration, CancellationToken token = default)
    {
        var process = new TelegramProcessDetector().Find().SingleOrDefault(p => string.Equals(p.ExecutableName, options.ProcessName + ".exe", StringComparison.OrdinalIgnoreCase));
        if (process is null) return new("WaitingForProcess", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, CanonicalAudioConverter.CanonicalSampleRate, CanonicalAudioConverter.CanonicalChannels);
        var result = await new NativeProcessLoopbackCapture().CaptureAsync(process.ProcessId, duration, token).ConfigureAwait(false);
        var canonicalFrames = result.Frames == 0 ? 0 : (long)Math.Round(result.Frames * (double)CanonicalAudioConverter.CanonicalSampleRate / Math.Max(result.SampleRate, 1));
        return new(result.Status == "PASS" ? "Running" : result.Status, process.ProcessId, result.Frames, canonicalFrames, result.Bytes, 0, 0, result.Peak, result.Rms, result.SampleRate, result.Channels, CanonicalAudioConverter.CanonicalSampleRate, CanonicalAudioConverter.CanonicalChannels);
    }
}
