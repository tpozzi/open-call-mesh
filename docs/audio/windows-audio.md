# Windows audio discovery

`OpenCallMesh.Audio.Windows` uses NAudio.Wasapi 3.0.1 (MIT) as a thin wrapper around Windows Core Audio/MMDevice/WASAPI. It enumerates active render/capture endpoints and session metadata without installing a driver.

Audio APIs must be tested in the interactive Windows user session. An SSH service or QEMU Guest Agent process can run in a non-interactive session and may see no user audio endpoints even when Device Manager reports the HDA device correctly.

Commands:

```powershell
OpenCallMesh.Cli.exe audio list
OpenCallMesh.Cli.exe audio sessions
OpenCallMesh.Cli.exe telegram find
```

The current implementation reports devices and sessions but does not capture or save audio.
