# Telegram positive capture and PC-to-VM700 test

The next end-to-end test runs Agent A interactively on the notebook while the
user's Telegram call has remote audio. Agent B runs interactively on VM700 with
`agent media-listen`. The test must report non-zero source and sink energy,
frame counts, timestamps, and drops. No Telegram audio is sent to the VM700
Telegram client in this phase.
