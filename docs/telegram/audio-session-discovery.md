# Telegram audio-session discovery

`TelegramProcessDetector` finds `Telegram.exe` dynamically by process name; it never hardcodes a PID. Audio-session correlation requires Telegram to be actively rendering audio, normally while connected to a voice chat. An open idle Telegram process is not sufficient evidence of an active audio session.
