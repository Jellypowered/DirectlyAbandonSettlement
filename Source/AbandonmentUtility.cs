using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace NoNeedAbandonedSettlement
{
    public static class AbandonmentUtility
    {
        public static void FinalizeAbandon(MapParent settlement)
        {
            var tile = settlement.Tile;
            Find.WorldObjects.Remove(settlement);
            SoundDefOf.Tick_High.PlayOneShotOnCamera();

            if (NNMod.Settings.cooldownDays > 0)
            {
                Find.WorldObjects.Add(new AbandonedSettlement { Tile = tile });
                Find.World.GetComponent<WorldComponent_TileCooldownTracker>()?.RegisterCooldown(tile);
            }
        }

        public static void FinalizeRemoveAbandonedTile(AbandonedSettlement abandonedSettlement)
        {
            if (NNMod.Settings.cooldownDays > 0)
            {
                Find.World.GetComponent<WorldComponent_TileCooldownTracker>()
                    ?.RegisterCooldown(abandonedSettlement.Tile);
            }
            else
            {
                Find.WorldObjects.Remove(abandonedSettlement);
            }

            SoundDefOf.Tick_High.PlayOneShotOnCamera();
        }
    }
}
