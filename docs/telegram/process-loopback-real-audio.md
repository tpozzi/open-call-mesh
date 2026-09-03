# Telegram process-loopback real-audio test

Run this in the interactive VM700 user session:

```text
OpenCallMesh.Cli.exe audio capture-process --process Telegram --duration 30
```

The command resolves Telegram dynamically and captures only the target process
tree using Windows process loopback. It reports frame flow independently from
audio energy and never writes a recording.

The current VM700 run produced 1,323,000 frames in 30.1 seconds at 44.1 kHz,
stereo, 32-bit IEEE float. Peak and RMS were zero because no remote participant
was speaking. This certifies activation and frame flow, but not positive remote
voice energy or process isolation.
