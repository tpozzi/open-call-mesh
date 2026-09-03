#pragma warning disable CA1416
using System.Diagnostics;
using OpenCallMesh.Audio.Windows;
using OpenCallMesh.Agent;
using OpenCallMesh.Controller;
using OpenCallMesh.Domain;
using OpenCallMesh.Transport;

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
if (args.Length >= 2 && args[0] == "agent" && args[1] == "capture-process")
{
    if (!OperatingSystem.IsWindows()) { Console.WriteLine("Windows Agent process capture requires Windows."); return; }
    var duration = TimeSpan.FromSeconds(30);
    for (var i = 2; i < args.Length; i++)
        if (args[i] == "--duration" && i + 1 < args.Length && double.TryParse(args[++i], out var seconds)) duration = TimeSpan.FromSeconds(seconds);
    var metrics = await new ProcessCaptureEndpoint(new("Telegram")).CaptureAsync(duration);
    Console.WriteLine($"AGENT_CAPTURE_STATE={metrics.State}\nTELEGRAM_PROCESS_RESOLVED={(metrics.ProcessId > 0 ? "PASS" : "FAIL")}\nPROCESS_CAPTURE_IN_AGENT={(metrics.State == "Running" ? "PASS" : "FAIL")}\nRawFramesCaptured={metrics.RawFramesCaptured}\nCanonicalFramesProduced={metrics.CanonicalFramesProduced}\nCaptureBytes={metrics.CaptureBytes}\nPeak={metrics.Peak:F6}\nRMS={metrics.Rms:F6}\nSourceFormat={metrics.SourceSampleRate}Hz/{metrics.SourceChannels}ch\nCanonicalFormat={metrics.CanonicalSampleRate}Hz/{metrics.CanonicalChannels}ch");
    return;
}
if (args.Length >= 2 && args[0] == "media" && args[1] == "listen")
{
    var port = 17871;
    var duration = 30;
    for (var i = 2; i < args.Length; i++)
    {
        if (args[i] == "--port" && i + 1 < args.Length) _ = int.TryParse(args[++i], out port);
        else if (args[i] == "--duration" && i + 1 < args.Length) _ = int.TryParse(args[++i], out duration);
    }
    long frames = 0, bytes = 0;
    using var mediaCts = new CancellationTokenSource(TimeSpan.FromSeconds(duration));
    try { await new MediaFrameTransport().RunServerAsync(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port), frame => { frames++; bytes += frame.Payload.Length; return Task.CompletedTask; }, mediaCts.Token); } catch (OperationCanceledException) { }
    Console.WriteLine($"MEDIA_LISTEN_FRAMES={frames}\nMEDIA_LISTEN_BYTES={bytes}\nMEDIA_LISTEN_DURATION_SECONDS={duration}");
    return;
}
if (args.Length >= 2 && args[0] == "agent" && args[1] == "media-listen")
{
    var port = 17871; var duration = 30;
    for (var i = 2; i < args.Length; i++)
    {
        if (args[i] == "--port" && i + 1 < args.Length) _ = int.TryParse(args[++i], out port);
        else if (args[i] == "--duration" && i + 1 < args.Length) _ = int.TryParse(args[++i], out duration);
    }
    var metrics = await new AgentMediaReceiver().ListenAsync(port, TimeSpan.FromSeconds(duration));
    Console.WriteLine($"AGENT_MEDIA_RECEIVER=PASS\nMEDIA_FRAMES_RECEIVED={metrics.Frames}\nMEDIA_BYTES_RECEIVED={metrics.Bytes}\nAGENT_B_AUDIO_LEVEL_NONZERO={(metrics.NonZeroFrames > 0 ? "YES" : "NO")}\nAGENT_B_PEAK={metrics.Peak:F6}\nAGENT_B_RMS={metrics.Rms:F6}");
    return;
}
if (args.Length >= 2 && args[0] == "media" && args[1] == "send-tone")
{
    var host = "127.0.0.1"; var port = 17871; var duration = 10; const int rate = 48000; const int frameSamples = 960;
    for (var i = 2; i < args.Length; i++)
    {
        if (args[i] == "--host" && i + 1 < args.Length) host = args[++i];
        else if (args[i] == "--port" && i + 1 < args.Length) _ = int.TryParse(args[++i], out port);
        else if (args[i] == "--duration" && i + 1 < args.Length) _ = int.TryParse(args[++i], out duration);
    }
    async IAsyncEnumerable<AudioFrame> Tone()
    {
        var total = rate * duration / frameSamples;
        for (var n = 0; n < total; n++)
        {
            var samples = new float[frameSamples];
            for (var i = 0; i < samples.Length; i++) samples[i] = 0.1f * MathF.Sin(2 * MathF.PI * 1000 * (n * frameSamples + i) / rate);
            var payload = new byte[samples.Length * sizeof(float)];
            Buffer.BlockCopy(samples, 0, payload, 0, payload.Length);
            yield return new AudioFrame("synthetic-tone", "media-cli", "test-sink", n, Stopwatch.GetTimestamp(), payload);
            await Task.Delay(20);
        }
    }
    Console.WriteLine($"TEST_TONE_PROCESS_STARTED=YES\nTEST_TONE_FREQUENCY=1000\nTEST_TONE_DURATION_SECONDS={duration}");
    await MediaFrameTransport.SendManyAsync(host, port, Tone(), CancellationToken.None);
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
Console.WriteLine("OpenCallMesh CLI\nCommands: audio list | audio sessions | audio capture-process --process Telegram [--duration 30] | agent capture-process [--duration 30] | agent media-listen [--port 17871] | media listen [--port 17871] | media send-tone --host <host> [--duration 10] | telegram find | controller run | agent register [id] [controller-host]");
