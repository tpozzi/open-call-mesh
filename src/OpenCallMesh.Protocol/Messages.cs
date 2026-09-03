using System.Text.Json;
using OpenCallMesh.Domain;

namespace OpenCallMesh.Protocol;

public abstract record ControlMessage(string Type);
public sealed record RegisterAgent(AgentIdentity Identity) : ControlMessage("register");
public sealed record RegisterAccepted(string AgentId, DateTimeOffset ServerTime) : ControlMessage("register.accepted");
public sealed record Heartbeat(string AgentId, DateTimeOffset At) : ControlMessage("heartbeat");
public sealed record BridgeCommand(string BridgeId, string Command) : ControlMessage("bridge.command");

public static class ProtocolJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public static byte[] Serialize(ControlMessage message) => JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(), Options);
}
