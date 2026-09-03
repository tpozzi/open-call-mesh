using OpenCallMesh.Domain;
using OpenCallMesh.Protocol;
using System.Text.Json;
var json=JsonSerializer.Serialize(new RegisterAgent(new AgentIdentity("test","machine","instance","0.1.0",new HashSet<string>{"pcm"})),ProtocolJson.Options);
if(!json.Contains("register",StringComparison.Ordinal)) throw new Exception("Protocol serialization failed");
Console.WriteLine("Protocol tests passed");
