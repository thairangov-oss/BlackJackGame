# Background Cleanup Sequence Diagram

```mermaid
sequenceDiagram
participant Cleanup as GameCleanupHostedService
participant GameStore as GS

loop Every 5 Minutes
  Cleanup->>GS: CleanupOldGames(maxAge)
  GS->>GS: Remove completed or old games
  GS-->>Cleanup: Removed count
end
```
