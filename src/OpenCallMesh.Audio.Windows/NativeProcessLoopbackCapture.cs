using System.Runtime.Versioning;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace OpenCallMesh.Audio.Windows;

/// <summary>Process loopback backed by NAudio's ActivateAudioInterfaceAsync implementation.</summary>
[SupportedOSPlatform("windows10.0.19041.0")]
public sealed class NativeProcessLoopbackCapture
{
    public async Task<ProcessLoopbackCaptureResult> CaptureAsync(int processId, TimeSpan duration, CancellationToken token = default)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows is required.");
        if (processId <= 0) throw new ArgumentOutOfRangeException(nameof(processId));
        if (duration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration));

        var started = DateTime.UtcNow;
        long frames = 0, bytes = 0, silentFrames = 0;
        double peak = 0, sumSquares = 0;
        WasapiRecorder? recorder = null;
        try
        {
            recorder = await new WasapiRecorderBuilder()
                .WithProcessLoopback((uint)processId, ProcessLoopbackMode.IncludeTargetProcessTree)
                .WithSharedMode().WithEventSync().WithBufferLength(100).BuildAsync().ConfigureAwait(false);
            var format = recorder.WaveFormat;
            recorder.DataAvailable += (buffer, flags, _, _) =>
            {
                var frameCount = buffer.Length / Math.Max(format.BlockAlign, 1);
                frames += frameCount;
                bytes += buffer.Length;
                if ((flags & AudioClientBufferFlags.Silent) != 0) { silentFrames += frameCount; return; }
                Measure(buffer, format, ref peak, ref sumSquares);
            };
            recorder.StartRecording();
            await Task.Delay(duration, token).ConfigureAwait(false);
            recorder.StopRecording();
            var seconds = Math.Max((DateTime.UtcNow - started).TotalSeconds, 0.001);
            var samples = Math.Max((frames - silentFrames) * format.Channels, 1);
            return new ProcessLoopbackCaptureResult("PASS", 0, 0, 0, frames, bytes, silentFrames, peak, Math.Sqrt(sumSquares / samples), seconds, format.SampleRate, format.Channels, format.BitsPerSample);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            try { recorder?.StopRecording(); } catch { }
            return new ProcessLoopbackCaptureResult("CANCELLED", 0, 0, 0, frames, bytes, silentFrames, peak, 0, (DateTime.UtcNow - started).TotalSeconds, 0, 0, 0);
        }
        catch (Exception ex)
        {
            try { recorder?.StopRecording(); } catch { }
            return ProcessLoopbackCaptureResult.Failed(ex.GetType().Name + ":" + ex.Message, ex.HResult, frames, bytes, silentFrames);
        }
        finally { recorder?.Dispose(); }
    }

    private static void Measure(ReadOnlySpan<byte> buffer, WaveFormat format, ref double peak, ref double sumSquares)
    {
        if (format.BitsPerSample == 32 && format.Encoding == WaveFormatEncoding.IeeeFloat)
        {
            foreach (var sample in System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(buffer))
            {
                var value = Math.Clamp(sample, -1f, 1f);
                peak = Math.Max(peak, Math.Abs(value));
                sumSquares += value * value;
            }
            return;
        }
        if (format.BitsPerSample != 16) return;
        foreach (var sample in System.Runtime.InteropServices.MemoryMarshal.Cast<byte, short>(buffer))
        {
            var value = sample / 32768.0;
            peak = Math.Max(peak, Math.Abs(value));
            sumSquares += value * value;
        }
    }
}

public sealed record ProcessLoopbackCaptureResult(string Status, int ApiHresult, int ActivationHresult, int FailureHresult, long Frames, long Bytes, long SilentFrames, double Peak, double Rms, double DurationSeconds, int SampleRate, int Channels, int BitsPerSample)
{
    public static ProcessLoopbackCaptureResult Failed(string reason, int hresult, long frames, long bytes, long silent) => new("FAIL:" + reason, hresult, 0, hresult, frames, bytes, silent, 0, 0, 0, 0, 0, 0);
}
