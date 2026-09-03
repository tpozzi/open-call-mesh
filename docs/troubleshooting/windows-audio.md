# Windows audio troubleshooting

- If Device Manager shows HDA but `audio list` is empty over SSH, run it in the logged-in desktop session.
- If Telegram is found but no session exists, start/keep a voice chat active and rerun `audio sessions`.
- If process capture is denied, verify the application is running in the same interactive user session and check the Windows audio services.
- No third-party audio driver is required for discovery; virtual microphone injection is a separate future decision.
