using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppSteamworks;
using MelonLoader;
using UnityEngine;

namespace LiarsBarMod.Features.Matchmaking
{
    public static class LobbyJoiner
    {
        public static int SelectedModeIndex = 0;
        public static string DirectLobbyIdInput = "";
        public static bool ShowInGameMatches = true;
        public static int FilterModeIndex = -1; // -1 = All Modes

        public static readonly string[] ModeNames = {
            "Standard Cards (Deck)",
            "Liar's Dice",
            "Devil Cards (Chaos Deck)",
            "Liar's Poker",
            "Velvet Room",
            "Texas Hold'em",
            "Liar's Spin",
            "Arena",
            "Russian Roulette"
        };

        public static string ResolveLobbyDataMode(CSteamID id)
        {
            try
            {
                string mode = SteamMatchmaking.GetLobbyData(id, "LobbyType");
                if (string.IsNullOrEmpty(mode)) mode = SteamMatchmaking.GetLobbyData(id, "lobbytype");
                if (string.IsNullOrEmpty(mode)) mode = SteamMatchmaking.GetLobbyData(id, "GameMode");
                if (string.IsNullOrEmpty(mode)) mode = SteamMatchmaking.GetLobbyData(id, "gameMode");
                if (string.IsNullOrEmpty(mode)) mode = SteamMatchmaking.GetLobbyData(id, "gamemode");
                if (string.IsNullOrEmpty(mode)) mode = SteamMatchmaking.GetLobbyData(id, "Mode");
                if (string.IsNullOrEmpty(mode)) mode = SteamMatchmaking.GetLobbyData(id, "DeckMode");
                if (string.IsNullOrEmpty(mode)) mode = SteamMatchmaking.GetLobbyData(id, "gamemodetext");
                return FormatModeName(mode);
            }
            catch
            {
                return "Standard Cards (Deck)";
            }
        }

        public static string FormatModeName(string rawMode)
        {
            if (string.IsNullOrEmpty(rawMode)) return "Standard Cards (Deck)";
            string trimmed = rawMode.Trim();
            if (int.TryParse(trimmed, out int mIdx))
            {
                if (mIdx >= 0 && mIdx < ModeNames.Length) return ModeNames[mIdx];
                return $"Mode #{mIdx}";
            }
            string lower = trimmed.ToLowerInvariant();
            if (lower.Contains("devil") || lower.Contains("chaos")) return "Devil Cards (Chaos Deck)";
            if (lower.Contains("dice")) return "Liar's Dice";
            if (lower.Contains("poker")) return "Liar's Poker";
            if (lower.Contains("velvet")) return "Velvet Room";
            if (lower.Contains("texas")) return "Texas Hold'em";
            if (lower.Contains("spin")) return "Liar's Spin";
            if (lower.Contains("arena")) return "Arena";
            if (lower.Contains("roulette")) return "Russian Roulette";
            if (lower.Contains("deck") || lower.Contains("card") || lower.Contains("standard")) return "Standard Cards (Deck)";
            return trimmed;
        }

        public struct LobbyInfo
        {
            public CSteamID Id;
            public ulong RawId;
            public string Name;
            public string Mode;
            public int Members;
            public int MaxMembers;
            public bool InProgress;
        }

        public struct FriendSession
        {
            public string FriendName;
            public CSteamID FriendId;
            public CSteamID LobbyId;
            public string Mode;
            public bool HasLobby;
        }

        public static readonly List<LobbyInfo> CachedLobbies = new List<LobbyInfo>();
        public static readonly List<FriendSession> CachedFriends = new List<FriendSession>();
        public static string StatusMessage = "Ready. Select a game mode or search lobbies.";
        private static float _lastAutoPoll = 0f;

        public static void PollUpdate()
        {
            if (Time.time - _lastAutoPoll > 1.5f)
            {
                _lastAutoPoll = Time.time;
                UpdateLobbyCache();
            }
        }

        public static string GetModeForLobby(CSteamID lobbyId)
        {
            try
            {
                for (int i = 0; i < CachedLobbies.Count; i++)
                {
                    if (CachedLobbies[i].Id.m_SteamID == lobbyId.m_SteamID && !string.IsNullOrEmpty(CachedLobbies[i].Mode))
                        return CachedLobbies[i].Mode;
                }
                for (int i = 0; i < CachedFriends.Count; i++)
                {
                    if (CachedFriends[i].LobbyId.m_SteamID == lobbyId.m_SteamID && !string.IsNullOrEmpty(CachedFriends[i].Mode))
                        return CachedFriends[i].Mode;
                }
                return ResolveLobbyDataMode(lobbyId);
            }
            catch { }
            return "Liar's Bar";
        }

        public static void JoinLobby(CSteamID lobbyId)
        {
            try
            {
                if (lobbyId.m_SteamID == 0) return;
                string modeName = GetModeForLobby(lobbyId);
                StatusMessage = $"Connecting to {modeName}...";
                MelonLogger.Msg($"[+] LobbyJoiner: Connecting to {modeName} (Steam Lobby {lobbyId.m_SteamID})");

                var sl = SteamLobby.Instance ?? UnityEngine.Object.FindObjectOfType<SteamLobby>();
                var nm = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>() ?? (CustomNetworkManager.singleton != null ? CustomNetworkManager.singleton.TryCast<CustomNetworkManager>() : null);

                // 1. Leave any existing lobby cleanly first to allow new connection
                try
                {
                    if (sl != null && sl.CurrentLobbyID != 0 && sl.CurrentLobbyID != lobbyId.m_SteamID)
                    {
                        SteamMatchmaking.LeaveLobby(new CSteamID(sl.CurrentLobbyID));
                        sl.CurrentLobbyID = 0;
                    }
                    if (nm != null && nm.isNetworkActive)
                    {
                        try { nm.StopClient(); } catch { }
                        try { nm.StopHost(); } catch { }
                    }
                }
                catch { }

                // 2. Unlock SteamLobby join lock and setup target
                if (sl != null)
                {
                    sl.JoinLocked = false;
                    if (sl.manager == null && nm != null) sl.manager = nm;
                    if (sl.LobbyText != null) sl.LobbyText.text = lobbyId.m_SteamID.ToString();
                    if (sl.lobbyname != null) sl.lobbyname.text = lobbyId.m_SteamID.ToString();
                }

                // 3. Update in-game connecting screen - show mode instead of raw number!
                var lm = UnityEngine.Object.FindObjectOfType<LobbyListManager>();
                if (lm != null)
                {
                    if (lm.ConnectingScreen != null) lm.ConnectingScreen.SetActive(true);
                    if (lm.ConnectingScreenInfo != null) lm.ConnectingScreenInfo.text = $"Connecting to {modeName}...";
                    if (lm.LobbyIdInput != null) lm.LobbyIdInput.text = lobbyId.m_SteamID.ToString();
                }

                // 4. Dispatch Join: Both SteamLobby engine methods to ensure private/friends-only lobbies work
                bool engineJoined = false;
                if (sl != null)
                {
                    try
                    {
                        sl.JoinLocked = false;
                        sl.JoinLobby(lobbyId);
                        engineJoined = true;
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[-] sl.JoinLobby threw {ex.GetType().Name} ({ex.Message}), attempting JoinLobbyWithId...");
                    }
                    try
                    {
                        sl.JoinLobbyWithId();
                        engineJoined = true;
                    }
                    catch { }
                }

                if (!engineJoined)
                {
                    MelonLogger.Msg($"[+] LobbyJoiner: Dispatching direct SteamMatchmaking.JoinLobby({lobbyId.m_SteamID})");
                    SteamMatchmaking.JoinLobby(lobbyId);
                }

                StatusMessage = $"Connecting to {modeName}...";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Join error: {ex.Message}";
                MelonLogger.Error($"[-] JoinLobby error: {ex.Message}");
            }
        }

        public static void QuickJoinMode(int modeIndex)
        {
            try
            {
                var sl = SteamLobby.Instance ?? UnityEngine.Object.FindObjectOfType<SteamLobby>();
                var nm = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>() ?? (CustomNetworkManager.singleton != null ? CustomNetworkManager.singleton.TryCast<CustomNetworkManager>() : null);

                // Leave existing lobby if currently in one
                try
                {
                    if (sl != null && sl.CurrentLobbyID != 0)
                    {
                        SteamMatchmaking.LeaveLobby(new CSteamID(sl.CurrentLobbyID));
                    }
                    if (nm != null)
                    {
                        try { nm.StopClient(); } catch { }
                        try { nm.StopHost(); } catch { }
                    }
                }
                catch { }

                if (sl != null)
                {
                    sl.JoinLocked = false;
                    sl.ShowInGameLobbies = ShowInGameMatches;
                    if (sl.lobbytype != null) sl.lobbytype.value = modeIndex;
                }

                if (nm != null)
                {
                    try { nm.Mode = (CustomNetworkManager.GameMode)modeIndex; } catch { }
                    try { nm.DeckMode = (modeIndex == 2 ? 1 : 0); } catch { }
                    try { nm.isVelvetRoom = (modeIndex == 4); } catch { }
                }

                if (sl != null)
                {
                    StatusMessage = $"Matchmaking queued for: {ModeNames[modeIndex]}";
                    MelonLogger.Msg($"[+] LobbyJoiner: Starting Matchmaking for {ModeNames[modeIndex]}");
                    sl.StartMatchMaking();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Matchmaking error: {ex.Message}";
            }
        }

        public static void CancelMatchmaking()
        {
            try
            {
                var sl = SteamLobby.Instance ?? UnityEngine.Object.FindObjectOfType<SteamLobby>();
                if (sl != null)
                {
                    sl.StopMatchMaking();
                    StatusMessage = "Matchmaking canceled.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Cancel error: {ex.Message}";
            }
        }

        public static void HostLobbyForMode(int modeIndex)
        {
            try
            {
                var sl = SteamLobby.Instance ?? UnityEngine.Object.FindObjectOfType<SteamLobby>();
                var nm = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>() ?? (CustomNetworkManager.singleton != null ? CustomNetworkManager.singleton.TryCast<CustomNetworkManager>() : null);

                if (sl != null && sl.lobbytype != null)
                {
                    sl.lobbytype.value = modeIndex;
                }

                if (nm != null)
                {
                    try { nm.Mode = (CustomNetworkManager.GameMode)modeIndex; } catch { }
                    try { nm.DeckMode = (modeIndex == 2 ? 1 : 0); } catch { }
                    try { nm.isVelvetRoom = (modeIndex == 4); } catch { }
                }

                if (sl != null)
                {
                    StatusMessage = $"Creating host lobby for {ModeNames[modeIndex]}...";
                    sl.HostLobby();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Host error: {ex.Message}";
            }
        }

        public static void RefreshLobbies()
        {
            try
            {
                StatusMessage = "Querying Steam matchmaking network...";
                var sl = SteamLobby.Instance ?? UnityEngine.Object.FindObjectOfType<SteamLobby>();
                if (sl != null)
                {
                    sl.ShowInGameLobbies = ShowInGameMatches;
                    sl.GetLobbiesList();
                }

                SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
                SteamMatchmaking.AddRequestLobbyListResultCountFilter(64);
                SteamMatchmaking.RequestLobbyList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Refresh error: {ex.Message}";
            }
        }

        public static void UpdateLobbyCache()
        {
            try
            {
                var sl = SteamLobby.Instance ?? UnityEngine.Object.FindObjectOfType<SteamLobby>();
                if (sl != null && sl.lobbiesIDs != null && sl.lobbiesIDs.Count > 0)
                {
                    CachedLobbies.Clear();
                    for (int i = 0; i < sl.lobbiesIDs.Count; i++)
                    {
                        var id = sl.lobbiesIDs[i];
                        if (id.m_SteamID == 0) continue;

                        string name = SteamMatchmaking.GetLobbyData(id, "name");
                        if (string.IsNullOrEmpty(name)) name = $"Lobby #{id.m_SteamID}";

                        string mode = ResolveLobbyDataMode(id);

                        int members = SteamMatchmaking.GetNumLobbyMembers(id);
                        int max = SteamMatchmaking.GetLobbyMemberLimit(id);
                        if (max <= 0) max = 4;

                        string inGameStr = SteamMatchmaking.GetLobbyData(id, "inGame");
                        bool inProgress = (inGameStr == "1" || inGameStr.Equals("true", StringComparison.OrdinalIgnoreCase));

                        CachedLobbies.Add(new LobbyInfo
                        {
                            Id = id,
                            RawId = id.m_SteamID,
                            Name = name,
                            Mode = mode,
                            Members = members,
                            MaxMembers = max,
                            InProgress = inProgress
                        });
                    }
                    StatusMessage = $"Discovered {CachedLobbies.Count} live lobbies.";
                }
            }
            catch { }
        }

        public static void RefreshFriends()
        {
            try
            {
                CachedFriends.Clear();
                int count = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
                for (int i = 0; i < count; i++)
                {
                    var fId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                    var gameInfo = new FriendGameInfo_t();
                    if (SteamFriends.GetFriendGamePlayed(fId, out gameInfo))
                    {
                        string friendName = SteamFriends.GetFriendPersonaName(fId);
                        bool hasLobby = (gameInfo.m_steamIDLobby.m_SteamID != 0);
                        string friendMode = "";
                        if (hasLobby)
                        {
                            friendMode = ResolveLobbyDataMode(gameInfo.m_steamIDLobby);
                        }
                        CachedFriends.Add(new FriendSession
                        {
                            FriendName = friendName,
                            FriendId = fId,
                            LobbyId = gameInfo.m_steamIDLobby,
                            Mode = friendMode,
                            HasLobby = hasLobby
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[-] RefreshFriends error: {ex.Message}");
            }
        }
    }
}
