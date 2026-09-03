using System.Net;
using System.Text.Json;
using OpenCallMesh.Domain;
using OpenCallMesh.Protocol;
using OpenCallMesh.Transport;

namespace OpenCallMesh.Agent;

public sealed class AgentRuntime(AgentIdentity identity, string controllerHost, int controllerPort)
{
    public AgentConnectionState State { get; private set; } = AgentConnectionState.Disconnected;
    public async Task RegisterAsync(CancellationToken token)
    {
        var message = JsonSerializer.Serialize(new RegisterAgent(identity), ProtocolJson.Options);
        await LineTransport.SendAsync(controllerHost, controllerPort, message, token);
        State = AgentConnectionState.Connected;
    }
}
