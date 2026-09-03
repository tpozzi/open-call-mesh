using System.Runtime.Versioning;
using OpenCallMesh.Audio.Abstractions;

namespace OpenCallMesh.Audio.Windows;

/// <summary>
/// Boundary for Microsoft's Application/Process Loopback API.
/// The native implementation is intentionally pending Windows integration
/// testing with an active Telegram call.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsProcessLoopbackCapture : IProcessAudioCapture
{
    public event EventHandler<AudioCaptureMeter>? MeterUpdated
    {
        add { }
        remove { }
    }

    public Task StartAsync(int processId, CancellationToken cancellationToken = default)
    {
        if (processId <= 0) throw new ArgumentOutOfRangeException(nameof(processId));
        throw new NotSupportedException(
            "Process loopback capture is not implemented yet. See docs/audio/process-loopback.md.");
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
