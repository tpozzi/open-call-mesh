using System.Net;
using OpenCallMesh.Domain;
using OpenCallMesh.Transport;

namespace OpenCallMesh.Agent;

public sealed record AgentMediaMetrics(long Frames, long Bytes, long NonZeroFrames, double Peak, double Rms);

/// <summary>Agent media-plane receiver with an in-memory statistics sink.</summary>
public sealed class AgentMediaReceiver
{
    public async Task<AgentMediaMetrics> ListenAsync(int port, TimeSpan duration, CancellationToken token = default)
    {
        long frames = 0, bytes = 0, nonZero = 0;
        double peak = 0, sumSquares = 0;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(duration);
        try
        {
            await new MediaFrameTransport().RunServerAsync(new IPEndPoint(IPAddress.Any, port), frame =>
            {
                frames++; bytes += frame.Payload.Length;
                var samples = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(frame.Payload.AsSpan());
                var framePeak = 0.0; foreach (var sample in samples) { var value = Math.Abs(sample); framePeak = Math.Max(framePeak, value); sumSquares += value * value; }
                peak = Math.Max(peak, framePeak); if (framePeak > 0.0001) nonZero++;
                return Task.CompletedTask;
            }, timeout.Token);
        }
        catch (OperationCanceledException) { }
        var sampleCount = Math.Max(bytes / sizeof(float), 1);
        return new AgentMediaMetrics(frames, bytes, nonZero, peak, Math.Sqrt(sumSquares / sampleCount));
    }
}
