using System.Collections.Generic;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using System;

namespace NoNeedAbandonedSettlement
{
    public class WorldComponent_TileCooldownTracker : WorldComponent
    {
        private Dictionary<int, int> tileCooldownTicks = new Dictionary<int, int>();


        public bool IsOnCooldown(int tileId)
        {
            return tileCooldownTicks.ContainsKey(tileId);
        }

        public WorldComponent_TileCooldownTracker(World world) : base(world) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref tileCooldownTicks, "tileCooldownTicks", LookMode.Value, LookMode.Value);
        }

        public override void WorldComponentTick()
        {
            if (Find.TickManager.TicksGame % 60000 == 0) // every in-game day
            {
                List<int> toRemove = new List<int>();

                foreach (var kvp in tileCooldownTicks)
                {
                    if (Find.TickManager.TicksGame >= kvp.Value)
                    {
                        var obj = Find.WorldObjects.AllWorldObjects
                            .Find(o => o is AbandonedSettlement && o.Tile == kvp.Key);
                        if (obj != null)
                            Find.WorldObjects.Remove(obj);

                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (var tile in toRemove)
                    tileCooldownTicks.Remove(tile);
            }
        }

        public void RegisterCooldown(int tile)
        {
            int days = Mathf.Clamp(NNMod.Settings.cooldownDays, 0, 180);
            int end = Find.TickManager.TicksGame + days * 60000;
            int cur;
            if (!tileCooldownTicks.TryGetValue(tile, out cur) || end > cur)
                tileCooldownTicks[tile] = end;
        }

        public int? GetDaysRemaining(int tile)
        {
            if (tileCooldownTicks.TryGetValue(tile, out int expireTick))
            {
                int ticksRemaining = expireTick - Find.TickManager.TicksGame;
                return Mathf.Max(0, ticksRemaining / 60000);
            }
            return null;
        }
        public int GetTicksRemaining(int tile)
        {
            if (tileCooldownTicks.TryGetValue(tile, out int expireTick))
            {
                return Math.Max(0, expireTick - Find.TickManager.TicksGame);
            }
            return 0;
        }


    }
}
