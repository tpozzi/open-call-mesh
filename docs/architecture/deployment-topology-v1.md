# Deployment topology v1

```text
User Notebook                         Proxmox VM700
Telegram A                            Telegram B
Agent A + Controller  <--- LAN --->  Agent B

VM701 = reserve/test endpoint (future Agent C)
```

The Controller must not own or terminate Telegram. Closing it must not close Telegram; the Agent is the local audio/process integration boundary.
