using System.Net;
using System.Text.Json;
using OpenCallMesh.Domain;
using OpenCallMesh.Protocol;
using OpenCallMesh.Transport;

namespace OpenCallMesh.Controller;

public sealed class ControllerRuntime
{
    private readonly Dictionary<string, AgentIdentity> _agents = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<AgentIdentity> Agents => _agents.Values;
    public void Register(AgentIdentity identity) => _agents[identity.AgentId] = identity;
    public async Task RunAsync(int port, CancellationToken token)
    {
        var transport = new LineTransport();
        await transport.RunServerAsync(new IPEndPoint(IPAddress.Any, port), HandleAsync, token);
    }
    private Task HandleAsync(string line)
    {
        try
        {
            using var doc = JsonDocument.Parse(line);
            if (doc.RootElement.GetProperty("type").GetString() == "register")
            {
                var registration = JsonSerializer.Deserialize<RegisterAgent>(line, ProtocolJson.Options);
                if (registration is not null) _agents[registration.Identity.AgentId] = registration.Identity;
            }
        }
        catch (JsonException) { }
        return Task.CompletedTask;
    }
}
