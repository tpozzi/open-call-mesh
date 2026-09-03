# ADR-0007: Windows process loopback capture

Status: proposed

## Context

OpenCallMesh must capture Telegram's rendered audio without capturing Chrome,
system notifications, or unrelated applications. Normal WASAPI device loopback
captures the complete render mix and is therefore insufficient.

## Decision

Target Microsoft's process loopback activation path:
`ActivateAudioInterfaceAsync` with `VIRTUAL_AUDIO_DEVICE_PROCESS_LOOPBACK` and
`AUDIOCLIENT_ACTIVATION_PARAMS`, selecting the Telegram process tree. The
implementation will sit behind `IProcessAudioCapture` and feed a bounded PCM
buffer; callbacks will not perform network I/O, logging, or disk writes.

The current release only provides the interface boundary and an explicit
Windows stub. It does not claim process capture until a Windows integration
test proves non-zero Telegram audio and excludes a concurrent non-Telegram test
tone.

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
