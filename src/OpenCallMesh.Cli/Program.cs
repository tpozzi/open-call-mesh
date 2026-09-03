using OpenCallMesh.Audio.Windows;
using OpenCallMesh.Agent;
using OpenCallMesh.Controller;
using OpenCallMesh.Domain;

if (args is ["audio", "list"])
{
    Console.WriteLine(new WindowsAudioInventory().Status);
    return;
}
if (args is ["controller", "run"])
{
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
    Console.WriteLine("Controller listening on 0.0.0.0:17870");
    await new ControllerRuntime().RunAsync(17870, cts.Token);
    return;
}
if (args.Length >= 2 && args[0] == "agent" && args[1] == "register")
{
    var id = args.Length > 2 ? args[2] : Environment.MachineName.ToLowerInvariant();
    var host = args.Length > 3 ? args[3] : "127.0.0.1";
    var identity = new AgentIdentity(id, Environment.MachineName, Guid.NewGuid().ToString("N"), "0.1.0", new HashSet<string> { "pcm" });
    await new AgentRuntime(identity, host, 17870).RegisterAsync(CancellationToken.None);
    Console.WriteLine($"Agent registered: {id}");
    return;
}
Console.WriteLine("OpenCallMesh CLI\nCommands: audio list | controller run | agent register [id] [controller-host]");
