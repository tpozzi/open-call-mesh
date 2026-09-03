using OpenCallMesh.Core;

namespace OpenCallMesh.Audio.Windows;

public sealed class WindowsAudioInventory
{
    public IReadOnlyList<AudioEndpointInfo> ListApplicationSessions() => Array.Empty<AudioEndpointInfo>();
    public string Status => OperatingSystem.IsWindows() ? "Windows audio inventory provider ready; native WASAPI discovery is Gate 1 follow-up." : "Windows provider unavailable on this host.";
}
