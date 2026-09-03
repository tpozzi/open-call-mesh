using OpenCallMesh.Domain;

namespace OpenCallMesh.Core;

public interface IAudioSource { IAsyncEnumerable<AudioFrame> ReadAsync(CancellationToken cancellationToken = default); }
public interface IAudioSink { ValueTask WriteAsync(AudioFrame frame, CancellationToken cancellationToken = default); }
public interface IAudioEncoder { byte[] Encode(ReadOnlySpan<byte> pcm); }
public interface IAudioDecoder { byte[] Decode(ReadOnlySpan<byte> encoded); }
public interface IApplicationAudioEndpointDetector
{
    IReadOnlyList<AudioEndpointInfo> Find(string processName);
}
public interface IVirtualAudioEndpoint { string Name { get; } }
public sealed record AudioEndpointInfo(string ProcessName, int ProcessId, string SessionId, string EndpointName, bool IsRender);

public sealed class PassthroughPcmCodec : IAudioEncoder, IAudioDecoder
{
    public byte[] Encode(ReadOnlySpan<byte> pcm) => pcm.ToArray();
    public byte[] Decode(ReadOnlySpan<byte> encoded) => encoded.ToArray();
}

public sealed class LoopGuard
{
    public bool ShouldForward(AudioFrame frame, string localAgentId, string localEndpointId) =>
        frame.OriginAgentId != localAgentId && frame.OriginEndpointId != localEndpointId;
}

public sealed class JitterBuffer
{
    private long _nextSequence;
    private readonly SortedDictionary<long, AudioFrame> _frames = new();
    public int Count => _frames.Count;
    public void Add(AudioFrame frame) { if (frame.Sequence >= _nextSequence && !_frames.ContainsKey(frame.Sequence)) _frames[frame.Sequence] = frame; }
    public bool TryRead(out AudioFrame? frame)
    {
        if (_frames.Remove(_nextSequence, out frame)) { _nextSequence++; return true; }
        frame = null; return false;
    }
}

public sealed class FakeAudioSource : IAudioSource
{
    private readonly IReadOnlyList<AudioFrame> _frames;
    public FakeAudioSource(IEnumerable<AudioFrame> frames) => _frames = frames.ToArray();
    public async IAsyncEnumerable<AudioFrame> ReadAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var frame in _frames) { cancellationToken.ThrowIfCancellationRequested(); yield return frame; await Task.Yield(); }
    }
}

public sealed class FakeAudioSink : IAudioSink
{
    private readonly List<AudioFrame> _frames = [];
    public IReadOnlyList<AudioFrame> Frames => _frames;
    public ValueTask WriteAsync(AudioFrame frame, CancellationToken cancellationToken = default) { cancellationToken.ThrowIfCancellationRequested(); _frames.Add(frame); return ValueTask.CompletedTask; }
}

public static class SyntheticRoute
{
    public static async Task<int> CopyAsync(IAudioSource source, IAudioSink sink, LoopGuard guard, string destinationAgentId, string destinationEndpointId, CancellationToken token = default)
    {
        var count = 0;
        await foreach (var frame in source.ReadAsync(token))
            if (guard.ShouldForward(frame, destinationAgentId, destinationEndpointId)) { await sink.WriteAsync(frame, token); count++; }
        return count;
    }
}
