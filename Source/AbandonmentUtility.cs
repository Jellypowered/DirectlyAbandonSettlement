using System;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace NoNeedAbandonedSettlement
{
    public static class AbandonmentUtility
    {
        private static bool UnsafeToTouchWorld()
        {
            return Scribe.mode != LoadSaveMode.Inactive || Current.Game == null || Find.World == null;
        }

        public static void FinalizeAbandon(MapParent settlement)
        {
            if (settlement == null || UnsafeToTouchWorld()) return;

            int tile = settlement.Tile;

            try
            {
                // Remove the active settlement if it still exists
                if (settlement.Spawned && !settlement.Destroyed)
                    settlement.Destroy();

                // Auto-remove: handle based on cooldown respect setting
                if (NNMod.Settings.autoRemoveImmediately)
                {
                    if (!NNMod.Settings.autoRemoveRespectCooldown || NNMod.Settings.cooldownDays == 0)
                    {
                        // Remove immediately without cooldown
                        var leftover = Find.WorldObjects.WorldObjectAt<AbandonedSettlement>(tile);
                        if (leftover != null && leftover.Spawned && !leftover.Destroyed)
                            leftover.Destroy();
                        
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        if (Prefs.DevMode) Log.Message("[DAS] FinalizeAbandon with auto-remove (immediate) on tile " + tile);
                        return;
                    }
                    else
                    {
                        // Respect cooldown - create abandoned tile and let WorldComponent auto-remove it after cooldown
                        if (Prefs.DevMode) Log.Message("[DAS] FinalizeAbandon with auto-remove (respecting cooldown) on tile " + tile);
                        // Fall through to cooldown logic below
                    }
                }

                if (NNMod.Settings.cooldownDays > 0)
                {
                    // If another mod already placed their own object, don't add ours; just register cooldown.
                    var existingVanilla = Find.WorldObjects.WorldObjectAt<AbandonedSettlement>(tile);
                    bool anyAtTile = Find.WorldObjects.AnyWorldObjectAt(tile);

                    if (existingVanilla == null && !anyAtTile)
                    {
                        var abandoned = (AbandonedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedSettlement);
                        abandoned.Tile = tile;
                        Find.WorldObjects.Add(abandoned);
                    }

                    var tracker = Find.World.GetComponent<WorldComponent_TileCooldownTracker>();
                    if (tracker != null) tracker.RegisterCooldown(tile);
                }
                else
                {
                    // Cooldown disabled: remove vanilla abandoned marker if present
                    var leftover = Find.WorldObjects.WorldObjectAt<AbandonedSettlement>(tile);
                    if (leftover != null && leftover.Spawned && !leftover.Destroyed)
                        leftover.Destroy();
                }

                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                if (Prefs.DevMode) Log.Message("[DAS] FinalizeAbandon OK on tile " + tile + " (cooldownDays=" + NNMod.Settings.cooldownDays + ")");
            }
            catch (Exception e)
            {
                Log.ErrorOnce("[DAS] FinalizeAbandon failed on tile " + tile + ": " + e, tile.GetHashCode());
            }
        }

        public static void FinalizeRemoveAbandonedTile(AbandonedSettlement abandonedSettlement)
        {
            if (abandonedSettlement == null || UnsafeToTouchWorld()) return;

            int tile = abandonedSettlement.Tile;

            try
            {
                if (NNMod.Settings.cooldownDays > 0)
                {
                    var tracker = Find.World.GetComponent<WorldComponent_TileCooldownTracker>();
                    if (tracker != null) tracker.RegisterCooldown(tile);
                }
                else
                {
                    // Remove if still present
                    var existing = Find.WorldObjects.WorldObjectAt<AbandonedSettlement>(tile);
                    if (existing != null && existing.Spawned && !existing.Destroyed)
                        existing.Destroy();
                }

                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                if (Prefs.DevMode) Log.Message("[DAS] FinalizeRemoveAbandonedTile OK on tile " + tile + " (cooldownDays=" + NNMod.Settings.cooldownDays + ")");
            }
            catch (Exception e)
            {
                Log.ErrorOnce("[DAS] FinalizeRemoveAbandonedTile failed on tile " + tile + ": " + e, ~tile);
            }
        }
        public static void ReconcileAfterVanillaAbandon(int tile)
        {
            if (Scribe.mode != LoadSaveMode.Inactive || Current.Game == null || Find.World == null) return;

            var wo = Find.WorldObjects;
            var marker = wo.WorldObjectAt<AbandonedSettlement>(tile);

            // Auto-remove: handle based on cooldown respect setting
            if (NNMod.Settings.autoRemoveImmediately)
            {
                if (!NNMod.Settings.autoRemoveRespectCooldown || NNMod.Settings.cooldownDays == 0)
                {
                    // Remove immediately without cooldown
                    if (marker != null && marker.Spawned && !marker.Destroyed)
                        marker.Destroy();
                    if (Prefs.DevMode) Log.Message("[DAS] ReconcileAfterVanillaAbandon with auto-remove (immediate) on tile " + tile);
                    return;
                }
                else
                {
                    // Respect cooldown - let WorldComponent auto-remove after cooldown
                    if (Prefs.DevMode) Log.Message("[DAS] ReconcileAfterVanillaAbandon with auto-remove (respecting cooldown) on tile " + tile);
                    // Fall through to cooldown logic below
                }
            }

            if (NNMod.Settings.cooldownDays > 0)
            {
                if (marker == null)
                {
                    var a = (AbandonedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedSettlement);
                    a.Tile = tile;
                    wo.Add(a);
                }

                var tracker = Find.World.GetComponent<WorldComponent_TileCooldownTracker>();
                if (tracker != null) tracker.RegisterCooldown(tile);
            }
            else
            {
                if (marker != null && marker.Spawned && !marker.Destroyed)
                    marker.Destroy(); // proper deregistration
            }
        }
    }
}
