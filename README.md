# OpenCallMesh

Open-source voice-room bridging for communication platforms. Telegram is the first endpoint connector; this project is independent and is not affiliated with, endorsed by, or sponsored by Telegram.

The first topology is a Controller and Agent A on the user's notebook, with Agent B and Telegram Desktop on Proxmox VM700. VM701 is reserved for tests. Audio recording, transcription, cloud processing and Telegram authentication are off by default.

## Bootstrap

```bash
dotnet build OpenCallMesh.sln
dotnet run --project src/OpenCallMesh.Cli -- audio list
dotnet run --project tests/OpenCallMesh.Core.Tests
dotnet run --project tests/OpenCallMesh.Protocol.Tests
dotnet run --project tests/OpenCallMesh.Transport.Tests
```

The current release is a deterministic fake-agent/control-plane bootstrap. Real WASAPI application-loopback discovery is the next gate and must be validated on Windows before deploying an Agent to VM700.

## Privacy and safety

Operators must have permission to bridge the rooms and comply with applicable law and community rules. No Telegram session files, OTPs, phone numbers, API credentials or audio recordings belong in this repository.
