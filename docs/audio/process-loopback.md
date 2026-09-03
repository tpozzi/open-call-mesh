# Process loopback

The current bootstrap does not claim process capture. The code exposes
`IProcessAudioCapture` and an explicit Windows stub; the native implementation
is the next gate.

The target is Microsoft's Application/Process Loopback Capture API using
`ActivateAudioInterfaceAsync` and `AUDIOCLIENT_ACTIVATION_PARAMS`. It captures
the selected process tree without binding to one physical endpoint. The API
requires Windows 10 build 20348 or later and returns silence when the target
process has no audio-rendering stream.

The implementation must use a bounded PCM channel between the audio callback
and worker. It must prove Telegram-only isolation while another application
emits a known test tone. No Stereo Mix, VoiceMeeter, VB-Cable, or other driver
is used.
