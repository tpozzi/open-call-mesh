namespace OpenCallMesh.Core;

public static class CanonicalAudioConverter
{
    public const int CanonicalSampleRate = 48_000;
    public const int CanonicalChannels = 1;

    /// <summary>Downmixes interleaved stereo float32 and resamples with linear interpolation.</summary>
    public static float[] StereoFloat32ToMono48K(ReadOnlySpan<float> stereo, int sourceSampleRate)
    {
        if (sourceSampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(sourceSampleRate));
        if ((stereo.Length & 1) != 0) throw new ArgumentException("Stereo input must contain pairs of samples.", nameof(stereo));
        var sourceFrames = stereo.Length / 2;
        if (sourceFrames == 0) return [];
        var outputFrames = checked((int)Math.Round(sourceFrames * (double)CanonicalSampleRate / sourceSampleRate, MidpointRounding.AwayFromZero));
        var mono = new float[sourceFrames];
        for (var i = 0; i < sourceFrames; i++) mono[i] = (stereo[i * 2] + stereo[i * 2 + 1]) * 0.5f;
        if (sourceSampleRate == CanonicalSampleRate) return mono;

        var output = new float[outputFrames];
        var ratio = sourceSampleRate / (double)CanonicalSampleRate;
        for (var i = 0; i < output.Length; i++)
        {
            var position = i * ratio;
            var left = Math.Min((int)position, mono.Length - 1);
            var right = Math.Min(left + 1, mono.Length - 1);
            var fraction = position - left;
            output[i] = (float)(mono[left] + (mono[right] - mono[left]) * fraction);
        }
        return output;
    }
}
