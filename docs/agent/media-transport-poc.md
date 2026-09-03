# Agent media transport POC

The LAN POC uses a length-prefixed binary TCP stream on a media port separate
from the JSON control connection. Each frame preserves stream ID, origin agent,
origin endpoint, sequence, timestamp, and payload. Frames are bounded to 1 MiB
and audio is not logged or persisted.

The Agent B test receiver is started interactively with:

```text
OpenCallMesh.Cli.exe agent media-listen --port 17871 --duration 30
```

The synthetic sender is:

```text
OpenCallMesh.Cli.exe media send-tone --host <vm700-ip> --port 17871 --duration 10
```

This validates transport and the Agent B statistics sink. It is not yet a
real Windows Agent A test; the notebook must run the Agent A process-loopback
source before Telegram-to-VM700 media can be certified.
