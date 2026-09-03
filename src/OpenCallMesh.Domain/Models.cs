namespace OpenCallMesh.Domain;

public sealed record AgentIdentity(string AgentId, string MachineName, string InstanceId, string Version, IReadOnlySet<string> Capabilities);
public sealed record AudioFormat(int SampleRate = 48_000, int Channels = 1, int BitsPerSample = 16);
public sealed record AudioFrame(string StreamId, string OriginAgentId, string OriginEndpointId, long Sequence, long TimestampTicks, byte[] Payload);
public sealed record BridgeDefinition(string Id, string Name, IReadOnlyList<string> Endpoints);
public enum AgentConnectionState { Connected, Degraded, Reconnecting, Disconnected }
