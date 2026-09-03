using OpenCallMesh.Audio.Windows;
using OpenCallMesh.Agent;
using OpenCallMesh.Controller;
using OpenCallMesh.Domain;

if (args is ["audio", "list"])
{
    if (!OperatingSystem.IsWindows()) { Console.WriteLine("Windows audio enumeration requires Windows."); return; }
    var inventory = new WindowsAudioInventory();
    Console.WriteLine(inventory.Status);
    foreach (var device in inventory.ListDevices()) Console.WriteLine($"DEVICE {device.Flow} {device.FriendlyName} [{device.State}] default={device.IsDefault} {device.SampleRate}Hz/{device.Channels}ch id={device.DeviceId}");
    foreach (var session in inventory.ListSessions()) Console.WriteLine($"SESSION pid={session.ProcessId} process={session.ProcessName} state={session.State} display={session.DisplayName} device={session.DeviceId}");
    return;
}
if (args is ["audio", "sessions"])
{
    if (!OperatingSystem.IsWindows()) { Console.WriteLine("Windows audio session enumeration requires Windows."); return; }
    var sessions = new WindowsAudioInventory().ListSessions();
    foreach (var session in sessions) Console.WriteLine($"pid={session.ProcessId} process={session.ProcessName} state={session.State} display={session.DisplayName} session={session.SessionIdentifier} instance={session.SessionInstanceIdentifier} device={session.DeviceId}");
    return;
}
if (args is ["telegram", "find"])
{
    if (!OperatingSystem.IsWindows()) { Console.WriteLine("Telegram process discovery requires Windows."); return; }
    var processes = new OpenCallMesh.Audio.Windows.TelegramProcessDetector().Find();
    if (processes.Count == 0) { Console.WriteLine("TELEGRAM_PROCESS_FOUND=NO"); return; }
    foreach (var process in processes) Console.WriteLine($"TELEGRAM_PROCESS_FOUND=YES pid={process.ProcessId} executable={process.ExecutableName} path={process.ExecutablePath ?? "N/A"} start={process.StartTime?.ToString("O") ?? "N/A"}");
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
Console.WriteLine("OpenCallMesh CLI\nCommands: audio list | audio sessions | telegram find | controller run | agent register [id] [controller-host]");
