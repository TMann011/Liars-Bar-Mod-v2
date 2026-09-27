# Liar's Bar Tactical Suite v4.4 (Inkwell Edition)

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011%20x64-blue.svg)](https://github.com/TMann011)
[![Runtime](https://img.shields.io/badge/Framework-MelonLoader%20v0.7.3%2B%20Open--Beta-green.svg)](https://github.com/LavaGang/MelonLoader)
[![Engine](https://img.shields.io/badge/Unity-2022.3.27f1%20IL2CPP-purple.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-gold.svg)](LICENSE)

An advanced, internal tactical introspection engine and gameplay modification suite for **Liar's Bar (Steam IL2CPP)**, featuring an **Inkwell Dark Glass UI**, real-time multi-mode card and dice ESP, cylinder manipulation radar, Steam matchmaking exploits, and host session tooling.

---

## 🎴 Key Capabilities

### 1. In-Game Visuals & Hand Telemetry
- **Draggable Tactical Hand HUD**: High-contrast, floating HUD overlay displaying player cards and live status.
- **Cross-Mode ESP Support**:
  - **Standard Cards (Deck)**: Real-time hand cards inspection (Ace, King, Queen, Jack, Devil).
  - **Chaos Deck**: Live card types & devil state tracking.
  - **Liar's Dice**: Real-time inspection of opponent dice under cups, plus alive dice summaries.
  - **Liar's Poker**: Opponent hole cards readout.
  - **Texas Hold'em**: Table community cards, hole card evaluation, and automatic best-hand solvers.
  - **Liar's Spin & Roulette**: Live slot reel tracking, bet amounts, and revolver covering states.
- **Last Played Card Tracker**: Displays the exact card and quantity last thrown by any opponent.

### 2. Revolver Cylinder & Weapon Rigging
- **Live Cylinder Radar**: Dynamic ASCII/glyph cylinder visualizer (`UiBuilder.GetRevolverRadar`) for local and all remote players.
- **Instant Safe Revolver**: Automatically offsets the bullet to guarantee survival on trigger pull.
- **Instant Deadly Revolver**: Instantly forces the bullet into the active chamber for lethal execution.
- **Cylinder & Bullet Stepping**: Interactive `B+1`, `B-1`, `Ch+1`, `Ch-1` controls.
- **Direct Chamber Selector**: Individual `[0]` through `[5]` chamber buttons for all players.
- **Mass Lethal Rigging**: Rig all opponent revolvers to deadly simultaneously.

### 3. Matchmaking & Steam Lobby Browser
- **Live Steam Lobby Browser**: Search worldwide Steam lobbies with game mode filtering.
- **Human-Readable Mode Titles**: Converts raw integer game modes (`0`–`7`) into clean names (`Standard Cards`, `Liar's Dice`, `Devil Cards`, `Texas Hold'em`, etc.).
- **Direct Lobby ID Connector**: Connect directly using 64-bit Steam Lobby IDs with paste support.
- **Steam Friends Session Sniffer**: One-click join on active friends playing Liar's Bar.
- **In-Progress Bypass**: Exploit to query and join active in-game matches.
- **Target Mode Matchmaking**: Queue directly for specific game types or host customized rooms.

### 4. Gameplay Rules & Host Authority
- **Bypass Card Placing Limit**: Play up to 5 cards per turn (bypasses 3-card restriction).
- **Always My Turn**: Play cards and interact without waiting for turn order.
- **Call Liar Anytime [F9]**: Trigger liar calls at any point during active rounds.
- **Round Card Manipulation**: Force the table round card to King, Queen, Ace, Jack, or Devil.
- **Force Host Migration [F10]**: Elect local client as room host via Mirror P2P protocol.
- **Networked Elimination**: Instantly eliminate target players via networked SyncVar/RPC methods.

### 5. Identity & Dynamic Cosmetics
- **RGB Rainbow Tag & Name**: Animated rainbow color cycling across local and network text meshes.
- **Preset Clan/Dev Tags**: `[DEV]`, `[PRO]`, `[VIP]`, `[ADMIN]`, `[STAFF]`, `[MOD]`, `[GOD]`.
- **Single-Dispatch Mirror Sync**: Broadcasts sanitized names over Mirror RPCs without frame-rate spam or connection drops.
- **Cosmetics Unlocker**: Instant access to all 8 characters and character skins.
- **Stats Editor**: Adjust Level, XP, and MMR across all game modes.

---

## ⌨️ Tactical Hotkeys

| Hotkey | Action | Scope |
| :--- | :--- | :--- |
| `[INSERT]` / `[F1]` | Toggle Main Tactical Glass Menu | All |
| `[L]` / `[F3]` | Toggle Floating Hotkey Reference HUD | All |
| `[P]` / `[F2]` | Export Live Game State to Desktop (`liarsbar_dump.txt`) | All |
| `[F4]` | Quick Safe Revolver (Loads 0 lethal chambers for self) | Match |
| `[F5]` | Quick Deadly Revolver (Forces live chamber on trigger) | Match |
| `[F6]` | Toggle God Mode (Zero bullets / Vignette clear / Shield) | Match |
| `[F7]` | Networked Elimination of Opponents | Host |
| `[F8]` | Force End Game Win (Direct winner handover) | Host |
| `[F9]` | Call Liar Anytime (Bypasses turn & game mode locks) | Match |
| `[F10]` | Force Host Migration (Seizes room authority as host) | Client |

---

## 🚀 Installation

1. Install **MelonLoader v0.7.3+ (Open-Beta)** for 64-bit Unity games into your Liar's Bar directory:
   - Official installer: [MelonLoader GitHub](https://github.com/LavaGang/MelonLoader)
2. Run the game once to allow MelonLoader to initialize and generate IL2CPP proxy assemblies in `MelonLoader/Il2CppAssemblies`.
3. Drop `LiarsBarMod.dll` into your `<GameDirectory>/Mods/` folder.
4. Launch Liar's Bar via Steam. Press `[INSERT]` to open the menu.

---

## 🛠️ Building From Source

Prerequisites:
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- MelonLoader v0.7.3+ generated dependencies

```bash
# Clone the repository
git clone https://github.com/TMann011/LiarsBarMod.git
cd LiarsBarMod

# Build Release binary (standard Steam installation)
dotnet build -c Release

# Or specify custom game directory
dotnet build -c Release -p:GamePath="D:\Games\Steam\steamapps\common\Liar's Bar"
```

The compiled binary will be located in `bin/Release/net6.0/LiarsBarMod.dll`.

---

## ☕ Support & Community

- **Developer**: Inkwell
- **GitHub**: [Inkwell](https://github.com/Inkwell)
- **Ko-fi**: [Support Inkwell on Ko-fi](https://ko-fi.com/Inkwell)

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).
