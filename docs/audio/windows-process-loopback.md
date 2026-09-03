# Windows process loopback

OpenCallMesh uses NAudio 3.0.1's `WasapiRecorderBuilder.WithProcessLoopback`
path. NAudio invokes the Windows `ActivateAudioInterfaceAsync` API with
`AUDIOCLIENT_PROCESS_LOOPBACK_PARAMS`, targeting the Telegram PID and including
its child-process tree.

The CLI command is:

```text
OpenCallMesh.Cli.exe audio capture-process --process Telegram --duration 30
OpenCallMesh.Cli.exe audio capture-process --pid 1234 --duration 30
```

The capture does not require an `IAudioSessionManager2` session match. Session
enumeration remains diagnostic. The capture reports frame flow independently
from audio energy, so a silent call can produce frames with `Peak=0` and
`RMS=0`.

The current statistics sink supports the observed 32-bit IEEE-float format and
16-bit PCM. It does not persist audio. The callback only updates bounded,
in-memory statistics; network transport and microphone injection are separate
future components.

## VM700 evidence

On Windows 11 build 26100, targeting the runtime-resolved Telegram PID:

- activation: PASS;
- capture: PASS;
- 10.1 seconds / 441,000 frames;
- 44.1 kHz, 2 channels, 32-bit;
- Peak/RMS: zero, consistent with a silent call;
- no third-party audio driver installed.
