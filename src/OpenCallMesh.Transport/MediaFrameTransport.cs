using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenCallMesh.Domain;

namespace OpenCallMesh.Transport;

/// <summary>Length-prefixed binary TCP media transport for the LAN POC.</summary>
public sealed class MediaFrameTransport
{
    private const int MaxFrameBytes = 1_048_576;
    public async Task RunServerAsync(IPEndPoint endpoint, Func<AudioFrame, Task> handler, CancellationToken token)
    {
        using var listener = new TcpListener(endpoint);
        listener.Start();
        while (!token.IsCancellationRequested)
        {
            using var client = await listener.AcceptTcpClientAsync(token);
            await ReadFramesAsync(client.GetStream(), handler, token);
        }
    }

    public static async Task SendAsync(string host, int port, AudioFrame frame, CancellationToken token)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port, token);
        await using var stream = client.GetStream();
        await WriteFrameAsync(stream, frame, token);
    }

    public static async Task SendManyAsync(string host, int port, IAsyncEnumerable<AudioFrame> frames, CancellationToken token)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port, token);
        await using var stream = client.GetStream();
        await foreach (var frame in frames.WithCancellation(token)) await WriteFrameAsync(stream, frame, token);
    }

    private static async Task WriteFrameAsync(NetworkStream stream, AudioFrame frame, CancellationToken token)
    {
        var body = Encode(frame);
        await stream.WriteAsync(BitConverter.GetBytes(body.Length), token);
        await stream.WriteAsync(body, token);
    }

    private static async Task ReadFramesAsync(NetworkStream stream, Func<AudioFrame, Task> handler, CancellationToken token)
    {
        var length = new byte[sizeof(int)];
        while (!token.IsCancellationRequested && await ReadExactAsync(stream, length, token))
        {
            var size = BitConverter.ToInt32(length);
            if (size <= 0 || size > MaxFrameBytes) throw new InvalidDataException($"Invalid media frame size: {size}.");
            var body = new byte[size];
            if (!await ReadExactAsync(stream, body, token)) break;
            await handler(Decode(body));
        }
    }

    private static async Task<bool> ReadExactAsync(Stream stream, byte[] buffer, CancellationToken token)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var count = await stream.ReadAsync(buffer.AsMemory(read), token);
            if (count == 0) return false;
            read += count;
        }
        return true;
    }

    private static byte[] Encode(AudioFrame frame)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write((byte)1);
        writer.Write(frame.StreamId); writer.Write(frame.OriginAgentId); writer.Write(frame.OriginEndpointId);
        writer.Write(frame.Sequence); writer.Write(frame.TimestampTicks); writer.Write(frame.Payload.Length); writer.Write(frame.Payload);
        return stream.ToArray();
    }

    private static AudioFrame Decode(byte[] body)
    {
        using var stream = new MemoryStream(body);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
        if (reader.ReadByte() != 1) throw new InvalidDataException("Unsupported media frame version.");
        var streamId = reader.ReadString(); var agentId = reader.ReadString(); var endpointId = reader.ReadString();
        var sequence = reader.ReadInt64(); var timestamp = reader.ReadInt64(); var payloadLength = reader.ReadInt32();
        if (payloadLength < 0 || payloadLength > MaxFrameBytes || payloadLength > stream.Length - stream.Position) throw new InvalidDataException("Invalid media payload length.");
        return new AudioFrame(streamId, agentId, endpointId, sequence, timestamp, reader.ReadBytes(payloadLength));
    }
}
