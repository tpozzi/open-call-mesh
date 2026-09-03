# Architecture

OpenCallMesh is a modular monolith plus distributed Agents. The Controller owns registration, bridge commands and health; Agents own local endpoint discovery and audio I/O. Control plane messages are JSON lines only for the bootstrap. Media is intentionally a separate abstraction and must not be sent as JSON frames.

Canonical model: `VoiceEndpoint`, `AudioSource`, `AudioSink`, `AudioRoute`, `Bridge`, `Agent`, `Controller`, and `Connector`. The initial codec is passthrough PCM (48 kHz mono 16-bit is the target format); Opus and QUIC/RTP-like media remain replaceable implementations.
