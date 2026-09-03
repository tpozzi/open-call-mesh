using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OpenCallMesh.Transport;

public sealed class LineTransport
{
    public async Task RunServerAsync(IPEndPoint endpoint, Func<string, Task> handler, CancellationToken token)
    {
        using var listener = new TcpListener(endpoint);
        listener.Start();
        while (!token.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(token);
            _ = HandleAsync(client, handler, token);
        }
    }

    public static async Task SendAsync(string host, int port, string line, CancellationToken token)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port, token);
        await using var stream = client.GetStream();
        var data = Encoding.UTF8.GetBytes(line + "\n");
        await stream.WriteAsync(data, token);
    }

    private static async Task HandleAsync(TcpClient client, Func<string, Task> handler, CancellationToken token)
    {
        await using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (!token.IsCancellationRequested && await reader.ReadLineAsync(token) is { } line) await handler(line);
        client.Dispose();
    }
}
