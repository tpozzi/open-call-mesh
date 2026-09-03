#pragma warning disable CA1416
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
if (args.Length >= 2 && args[0] == "audio" && args[1] == "capture-process")
{
    if (!OperatingSystem.IsWindows()) { Console.WriteLine("Windows process-loopback capture requires Windows."); return; }
    var duration = TimeSpan.FromSeconds(30);
    var pid = 0;
    for (var i = 2; i < args.Length; i++)
    {
        if (args[i] == "--duration" && i + 1 < args.Length && double.TryParse(args[++i], out var seconds)) duration = TimeSpan.FromSeconds(seconds);
        else if (args[i] == "--pid" && i + 1 < args.Length) _ = int.TryParse(args[++i], out pid);
        else if (args[i] == "--process" && i + 1 < args.Length && string.Equals(args[++i], "Telegram", StringComparison.OrdinalIgnoreCase))
            pid = new TelegramProcessDetector().Find().SingleOrDefault()?.ProcessId ?? 0;
    }
    if (pid <= 0) { Console.WriteLine("PROCESS_LOOPBACK_ACTIVATION=FAIL"); Console.WriteLine("FAILURE_STAGE=ProcessResolution"); return; }
    Console.WriteLine($"TargetProcess=Telegram.exe\nTargetPid={pid}\nWindowsBuild={Environment.OSVersion.Version.Build}\nTargetProcessTreeMode=INCLUDE");
    var result = await new NativeProcessLoopbackCapture().CaptureAsync(pid, duration);
    Console.WriteLine($"PROCESS_LOOPBACK_ACTIVATION={(result.Status == "PASS" ? "PASS" : "FAIL")}\nAPI_CALL_HRESULT=0x{result.ApiHresult:X8}\nACTIVATION_RESULT_HRESULT=0x{result.ActivationHresult:X8}\nFAILURE_HRESULT=0x{result.FailureHresult:X8}\nCAPTURE_STATE={result.Status}\nDuration={result.DurationSeconds:F1}\nFrames={result.Frames}\nBytes={result.Bytes}\nSilentFrames={result.SilentFrames}\nPeak={result.Peak:F6}\nRMS={result.Rms:F6}\nFormat={result.SampleRate}Hz/{result.Channels}ch/{result.BitsPerSample}bit");
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
Console.WriteLine("OpenCallMesh CLI\nCommands: audio list | audio sessions | audio capture-process --process Telegram [--duration 30] | telegram find | controller run | agent register [id] [controller-host]");
