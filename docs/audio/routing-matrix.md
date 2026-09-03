# Audio routing matrix

| Signal | Telegram A | Bridge to B | Return to A |
|---|---:|---:|---:|
| Local microphone A | yes | via designed Telegram remote mix | no |
| Remote room A audio | n/a | yes | n/a |
| Remote room B audio | n/a | n/a | yes |
| Audio injected from B | no recapture | no retransmit to origin | no |

Digital loop exclusion is separate from acoustic echo cancellation. The recapture behavior of Windows application loopback must be measured before real Telegram audio is enabled.
