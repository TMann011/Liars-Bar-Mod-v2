using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace LiarsBarMod.Features.Poker
{
    public static class PokerController
    {
        // Card type cycle order for up/down arrows
        public static readonly int[] CardTypeCycle = { 1, 2, 3, 4, -1 }; // K, Q, A, J, Devil

        public static List<int> GetCardsForPoker(PokerGamePlay pg)
        {
            var res = new List<int>();
            if (pg == null) return res;

            try
            {
                if (pg.CardTypes != null && pg.CardTypes.Count > 0)
                {
                    for (int i = 0; i < pg.CardTypes.Count; i++) res.Add(pg.CardTypes[i]);
                    return res;
                }

                if (pg.Cards != null && pg.Cards.Count > 0)
                {
                    for (int i = 0; i < pg.Cards.Count; i++)
                    {
                        var c = pg.Cards[i];
                        if (c != null)
                        {
                            var pc = c.GetComponent<PokerCard>();
                            if (pc != null) res.Add(pc.cardtype);
                        }
                    }
                }
            }
            catch { }

            return res;
        }

        /// <summary>
        /// Sets a single poker card slot. Updates local card object, all PokerCard components,
        /// and CardTypes SyncList, then broadcasts authoritative RandomCards/SetCardsCmd.
        /// </summary>
        public static void SetPokerCard(PokerGamePlay pg, int slot, int type)
        {
            if (pg == null || slot < 0 || slot > 1) return;

            try
            {
                // Update the CardTypes SyncList entry
                if (pg.CardTypes != null)
                {
                    while (pg.CardTypes.Count <= slot) pg.CardTypes.Add(type);
                    pg.CardTypes[slot] = type;
                }

                // Update the local PokerCard component + visual mesh
                if (pg.Cards != null && slot < pg.Cards.Count)
                {
                    var c = pg.Cards[slot];
                    if (c != null)
                    {
                        var pc = c.GetComponent<PokerCard>();
                        if (pc != null)
                        {
                            pc.cardtype = type;
                            try { pc.SetCard(); } catch { }
                        }
                    }
                }

                // Also update any PokerCard components in pg hierarchy or CardsParent
                var allPokerCards = pg.GetComponentsInChildren<PokerCard>(true);
                if (allPokerCards != null && slot < allPokerCards.Length && allPokerCards[slot] != null)
                {
                    allPokerCards[slot].cardtype = type;
                    try { allPokerCards[slot].SetCard(); } catch { }
                }
                if (pg.CardsParent != null)
                {
                    var parentCards = pg.CardsParent.GetComponentsInChildren<PokerCard>(true);
                    if (parentCards != null && slot < parentCards.Length && parentCards[slot] != null)
                    {
                        parentCards[slot].cardtype = type;
                        try { parentCards[slot].SetCard(); } catch { }
                    }
                }

                // Update the network-synced card value
                pg.CardValue = type;
                pg.NetworkCardValue = type;
                pg.currentcard = slot;

                // Sync to server and peers:
                // RandomCards is the authoritative command setting hole cards
                if (pg.CardTypes != null && pg.CardTypes.Count >= 2)
                {
                    int c0 = pg.CardTypes[0];
                    int c1 = pg.CardTypes[1];
                    try { pg.RandomCards(c0, c1); } catch { }
                    try { pg.CmdShowCardsFirst(0, c0, c1); } catch { }
                }
                try { pg.SetCardsCmd(); } catch { }
                try { pg.SetCardsRpc(); } catch { }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Warning($"SetPokerCard error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cycles a card slot to the next type in the cycle order.
        /// direction: +1 = up, -1 = down
        /// </summary>
        public static void CyclePokerCard(PokerGamePlay pg, int slot, int direction)
        {
            if (pg == null) return;
            var cards = GetCardsForPoker(pg);
            int current = slot < cards.Count ? cards[slot] : 1;
            int idx = Array.IndexOf(CardTypeCycle, current);
            if (idx < 0) idx = 0;
            int next = (idx + direction + CardTypeCycle.Length) % CardTypeCycle.Length;
            SetPokerCard(pg, slot, CardTypeCycle[next]);
        }

        public static void ApplyBestPokerHand(PokerGamePlay pg)
        {
            if (pg == null) return;
            SetPokerCard(pg, 0, 3); // Ace
            SetPokerCard(pg, 1, 3); // Ace
        }

        /// <summary>
        /// God mode: removes all live bullets from the cylinder so pulling trigger is always safe.
        /// Avoids clearing the list entirely (which can crash); instead sets all entries to -1 (empty).
        /// </summary>
        public static void EnableGodMode(PokerGamePlay pg)
        {
            if (pg == null) return;
            try
            {
                if (pg.fullbullets != null)
                {
                    // Overwrite every bullet position with a non-death value
                    for (int i = 0; i < pg.fullbullets.Count; i++)
                    {
                        pg.fullbullets[i] = -1;
                    }
                }
                // Also move CurrentBullet past the end so it never matches
                pg.CurrentBullet = 999;
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Warning($"GodMode error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the death bullet to be the current one — next pull kills.
        /// </summary>
        public static void ForceDeadlyGun(PokerGamePlay pg)
        {
            if (pg == null) return;
            try
            {
                if (pg.fullbullets != null)
                {
                    // Clear and set exactly one live bullet at the current position
                    for (int i = 0; i < pg.fullbullets.Count; i++)
                        pg.fullbullets[i] = -1;

                    int cur = pg.CurrentBullet;
                    if (cur >= 0 && cur < pg.fullbullets.Count)
                        pg.fullbullets[cur] = cur;
                    else if (pg.fullbullets.Count > 0)
                        pg.fullbullets[0] = 0;
                }
            }
            catch { }
        }

        /// <summary>
        /// Sets the CurrentBullet index (which chamber the revolver is on).
        /// </summary>
        public static void SetCurrentBullet(PokerGamePlay pg, int bullet)
        {
            if (pg == null) return;
            pg.CurrentBullet = bullet;
        }

        /// <summary>
        /// Sets a specific chamber index as the death bullet.
        /// All other chambers become empty.
        /// </summary>
        public static void SetDeathBullet(PokerGamePlay pg, int chamberIndex)
        {
            if (pg == null) return;
            try
            {
                if (pg.fullbullets != null)
                {
                    for (int i = 0; i < pg.fullbullets.Count; i++)
                        pg.fullbullets[i] = -1;

                    if (chamberIndex >= 0 && chamberIndex < pg.fullbullets.Count)
                        pg.fullbullets[chamberIndex] = chamberIndex;
                }
            }
            catch { }
        }

        public static void WinRound(PokerGamePlay pg)
        {
            if (pg == null) return;
            try
            {
                EnableGodMode(pg);
                SetPokerCard(pg, 0, 1); // King
                SetPokerCard(pg, 1, 1); // King
            }
            catch { }
        }

        /// <summary>
        /// Returns the fullbullets list as readable ints for display.
        /// </summary>
        public static List<int> GetBullets(PokerGamePlay pg)
        {
            var res = new List<int>();
            if (pg?.fullbullets == null) return res;
            try
            {
                for (int i = 0; i < pg.fullbullets.Count; i++)
                    res.Add(pg.fullbullets[i]);
            }
            catch { }
            return res;
        }
    }
}
