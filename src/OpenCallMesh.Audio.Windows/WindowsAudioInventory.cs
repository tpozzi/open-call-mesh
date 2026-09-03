using OpenCallMesh.Core;
using NAudio.CoreAudioApi;
using System.Runtime.Versioning;

namespace OpenCallMesh.Audio.Windows;

[SupportedOSPlatform("windows")]
public sealed class WindowsAudioInventory
{
    public IReadOnlyList<WindowsAudioDevice> ListDevices()
    {
        if (!OperatingSystem.IsWindows()) return [];
        using var enumerator = new MMDeviceEnumerator();
        var defaults = new Dictionary<DataFlow, string?>
        {
            [DataFlow.Render] = TryDefault(enumerator, DataFlow.Render),
            [DataFlow.Capture] = TryDefault(enumerator, DataFlow.Capture)
        };
        return enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active)
            .Select(d => { using var client = d.CreateAudioClient(); return new WindowsAudioDevice(d.ID, d.FriendlyName, d.DataFlow.ToString(), d.State.ToString(), defaults[d.DataFlow] == d.ID, client.MixFormat.SampleRate, client.MixFormat.Channels); })
            .ToArray();
    }

    public IReadOnlyList<WindowsAudioSession> ListSessions()
    {
        if (!OperatingSystem.IsWindows()) return [];
        using var enumerator = new MMDeviceEnumerator();
        var sessions = new List<WindowsAudioSession>();
        foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
        {
            using var collection = device.AudioSessionManager.Sessions;
            for (var i = 0; i < collection.Count; i++)
            {
                using var session = collection[i];
                var pid = unchecked((int)session.GetProcessID);
                var processName = pid > 0 ? TryProcessName(pid) : "System";
                sessions.Add(new WindowsAudioSession(pid, processName, session.DisplayName, session.GetSessionIdentifier, session.GetSessionInstanceIdentifier, session.State.ToString(), device.ID));
            }
        }
        return sessions.DistinctBy(s => (s.DeviceId, s.SessionInstanceIdentifier)).ToArray();
    }

    public string Status => OperatingSystem.IsWindows() ? "Windows WASAPI provider ready." : "Windows provider unavailable on this host.";

    private static string? TryDefault(MMDeviceEnumerator enumerator, DataFlow flow) { try { using var device = enumerator.GetDefaultAudioEndpoint(flow, Role.Multimedia); return device.ID; } catch { return null; } }
    private static string TryProcessName(int pid) { try { return System.Diagnostics.Process.GetProcessById(pid).ProcessName + ".exe"; } catch { return "Unknown"; } }
}

public sealed record WindowsAudioDevice(string DeviceId, string FriendlyName, string Flow, string State, bool IsDefault, int SampleRate, int Channels);
public sealed record WindowsAudioSession(int ProcessId, string ProcessName, string DisplayName, string SessionIdentifier, string SessionInstanceIdentifier, string State, string DeviceId);
