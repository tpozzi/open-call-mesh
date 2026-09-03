using System.Diagnostics;
using System.Runtime.Versioning;

namespace OpenCallMesh.Audio.Windows;

[SupportedOSPlatform("windows")]
public sealed class TelegramProcessDetector
{
    public IReadOnlyList<TelegramProcessInfo> Find()
    {
        if (!OperatingSystem.IsWindows()) return [];
        return Process.GetProcessesByName("Telegram").Select(p =>
        {
            try { return new TelegramProcessInfo(p.Id, p.ProcessName + ".exe", p.MainModule?.FileName, p.StartTime); }
            catch { return new TelegramProcessInfo(p.Id, p.ProcessName + ".exe", null, null); }
            finally { p.Dispose(); }
        }).ToArray();
    }
}

public sealed record TelegramProcessInfo(int ProcessId, string ExecutableName, string? ExecutablePath, DateTime? StartTime);
