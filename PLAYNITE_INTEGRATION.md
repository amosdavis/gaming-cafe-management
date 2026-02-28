# Playnite Integration Strategy

## Overview
Playnite is integrated via two approaches:
1. **Direct Process Launch** (MVP) — Simple command-line launching via process management
2. **Playnite SDK Plugin** (Phase 2) — Full integration via C# plugin for metadata access and advanced features

## Phase 1: MVP Integration (Direct Process Launch)

### Scope
- Launch games by name via Playnite command-line
- Monitor Playnite process state
- Run Playnite in fullscreen kiosk mode
- Track active game sessions

### Implementation
**PlayniteIntegrationService** provides:
- `IsPlayniteRunning()` — Check if Playnite is active
- `LaunchGameAsync(gameName)` — Launch game via Playnite
- `StartPlayniteKioskAsync()` — Start Playnite in fullscreen mode
- `GetAvailableGamesAsync()` — Placeholder for Phase 2

### Limitations
- No access to Playnite's game library without SDK
- Game discovery requires manual UI interaction
- Session tracking via process monitoring only

## Phase 2: Full SDK Integration (Future)

### Playnite SDK Details
- **Target Framework:** .NET Framework 4.6.2 (separate plugin project)
- **NuGet Package:** PlayniteSDK v6.15+
- **Plugin Types:** GenericPlugin, LibraryPlugin, MetadataPlugin

### Planned Features
1. **Game Library Import**
   - Auto-discover games from Steam, Epic, GOG, Origin
   - Sync game metadata to GameCafe database
   - Update playtime statistics

2. **Event Hooks**
   - OnGameStarted — Trigger session creation
   - OnGameStopped — Trigger session end & billing
   - OnSessionChange — Update station status

3. **Metadata Enrichment**
   - Genre categorization for billing rates
   - Cover art/media for kiosk UI
   - Developer/publisher information

4. **Playnite Plugin Architecture**
   ```csharp
   public class GameCafePlaynitePlugin : GenericPlugin
   {
       public GameCafePlaynitePlugin(IPlayniteAPI api) : base(api) { }
       
       public override void OnGameStarted(OnGameStartedEventArgs args) 
       {
           // Notify GameCafe Station Agent
           _stationAgent.OnGameLaunched(args.Game.Name, args.Game.Id);
       }
       
       public override void OnGameStopped(OnGameStoppedEventArgs args)
       {
           // Notify GameCafe Station Agent for session end
           _stationAgent.OnGameStopped(args.Game.Id);
       }
   }
   ```

### Integration Flow (Phase 2)
1. Station Agent runs Playnite + GameCafe plugin
2. Player selects game in Playnite
3. Plugin detects launch, sends message to Station Agent
4. Agent creates session in local DB
5. Game runs with session timer active
6. Plugin detects game exit
7. Agent ends session, calculates billing, persists to server

## Configuration Files

### Playnite Installation
- **Default Path:** `C:\Program Files\Playnite\`
- **Data Path:** `%APPDATA%\Playnite\`
- **Library Location:** Configurable (Steam, Epic, GOG, Origin, local)

### GameCafe Playnite Configuration
```json
{
  "playnite": {
    "installPath": "C:\\Program Files\\Playnite\\",
    "fullscreenMode": true,
    "autoStartOnStationBoot": true,
    "enableMetadataSync": false,
    "pluginEnabled": false
  }
}
```

## Testing Strategy

### MVP Validation
```csharp
[TestFixture]
public class PlayniteIntegrationTests
{
    [Test]
    public async Task LaunchGame_WithPlayniteRunning_ReturnsTrue()
    {
        var service = new PlayniteIntegrationService();
        var result = await service.LaunchGameAsync("Elden Ring");
        Assert.IsTrue(result);
    }
    
    [Test]
    public void IsPlayniteRunning_WhenPlayniteActive_ReturnsTrue()
    {
        var service = new PlayniteIntegrationService();
        var running = service.IsPlayniteRunning();
        Assert.IsTrue(running);
    }
}
```

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Playnite API changes | Plugin incompatibility | Monitor Playnite releases, use semantic versioning for plugin |
| Game launch failures | Session tracking errors | Add fallback direct .exe launch, manual override |
| Metadata unavailable (MVP) | Limited billing categories | Use hardcoded mappings, allow manual game config |
| Playnite not installed | Station inoperable | Installer checks, clear documentation |

## References
- [Playnite API Docs](https://api.playnite.link/docs/tutorials/extensions/plugins.html)
- [PlayniteSDK NuGet](https://www.nuget.org/packages/PlayniteSDK/)
- [Playnite GitHub](https://github.com/JosefNemec/Playnite)
