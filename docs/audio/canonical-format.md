# Canonical audio format

The OpenCallMesh canonical format is 48,000 Hz, mono, float32 PCM. The current
Windows process-loopback test observed 44,100 Hz, stereo, 32-bit IEEE float.
Conversion occurs at the capture boundary using stereo averaging followed by
linear interpolation resampling. The converter preserves duration within one
output frame and is isolated in `OpenCallMesh.Core`; network transport does not
perform format conversion.

The resampler is adequate for the current diagnostic POC. A higher-quality
band-limited resampler can replace it later without changing the audio
contracts.
