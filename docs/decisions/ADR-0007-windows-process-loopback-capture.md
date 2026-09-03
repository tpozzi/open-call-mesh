# ADR-0007: Windows process loopback capture

Status: accepted

## Context

OpenCallMesh must capture Telegram's rendered audio without capturing Chrome,
system notifications, or unrelated applications. Normal WASAPI device loopback
captures the complete render mix and is therefore insufficient.

## Decision

Target Microsoft's process loopback activation path through NAudio 3.0.1:
`ActivateAudioInterfaceAsync` with `AUDIOCLIENT_PROCESS_LOOPBACK_PARAMS`,
selecting the Telegram process tree. The implementation sits behind the
Windows audio component and feeds in-memory statistics; callbacks do not
perform network I/O, logging, or disk writes.

The current release has passed runtime activation and frame-flow testing on
Windows 11 build 26100. Non-zero Telegram audio and exclusion of a concurrent
non-Telegram test tone remain separate manual evidence gates.

## Compatibility

Microsoft documents process loopback for Windows 10 build 20348 or later. The
target Windows 11 24H2 VMs meet the OS-family requirement, but Telegram call
and session behavior still requires testing in an interactive user session.

## Consequences

- No Stereo Mix or third-party virtual audio driver is required for capture.
- Native interop will require a Windows-specific implementation, including the
  asynchronous activation callback and WASAPI capture client.
- A Telegram process with no active render stream produces silence.
- Injection into Telegram's microphone remains a separate unresolved problem.
