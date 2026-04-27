using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace NoNeedAbandonedSettlement
{
    [HarmonyPatch(typeof(WorldObject), nameof(WorldObject.GetGizmos))]
    public static class AbandonedSettlement_RemoveGizmoPatch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, WorldObject __instance)
        {
            foreach (var gizmo in __result)
                yield return gizmo;

            if (!(__instance is AbandonedSettlement abandonedSettlement))
                yield break;

            var tracker = Find.World.GetComponent<WorldComponent_TileCooldownTracker>();
            int ticks = tracker?.GetTicksRemaining(abandonedSettlement.Tile) ?? 0;

            var cmd = new Command_Action
            {
                defaultLabel = "DAS_RemoveAbandonedTileLabel".Translate(),
                defaultDesc = "DAS_RemoveAbandonedTileDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmo_RemoveAbandoned"),
                action = () =>
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "DAS_RemoveAbandonedTileConfirm".Translate(),
                        () => AbandonmentUtility.FinalizeRemoveAbandonedTile(abandonedSettlement),
                        true
                    ));
                }
            };

            if (ticks > 0)
            {
                int days = Mathf.CeilToInt(ticks / 60000f);
                cmd.Disable("DAS_CooldownActive".Translate(days));
            }

            yield return cmd;
        }
    }



    [HarmonyPatch(typeof(SettlementAbandonUtility), "Abandon")]
    [HarmonyPriority(Priority.Last)]
    public static class SettlementAbandonUtility_Abandon_Postfix
    {
        public static void Postfix(MapParent settlement)
        {
            // Don’t mutate during load/scribe
            if (Scribe.mode != LoadSaveMode.Inactive || Current.Game == null || Find.World == null) return;
            if (settlement == null) return;

            AbandonmentUtility.ReconcileAfterVanillaAbandon(settlement.Tile);
        }
    }

    [HarmonyPatch(typeof(AbandonedSettlement), nameof(AbandonedSettlement.GetInspectString))]
    public static class AbandonedSettlement_InspectStringPatch
    {
        public static void Postfix(AbandonedSettlement __instance, ref string __result)
        {
            var tracker = Find.World.GetComponent<WorldComponent_TileCooldownTracker>();
            int ticksRemaining = tracker?.GetTicksRemaining(__instance.Tile) ?? 0;

            if (ticksRemaining > 0)
            {
                int days = Mathf.CeilToInt(ticksRemaining / 60000f);
                __result += "\n" + "DAS_CooldownRemainingLabel".Translate(days);
            }
        }
    }

    [HarmonyPatch(typeof(RitualBehaviorWorker_GravshipLaunch), nameof(RitualBehaviorWorker_GravshipLaunch.TryExecuteOn))]
    [HarmonyPriority(Priority.Last)]
    public static class RitualBehaviorWorker_GravshipLaunch_TryExecuteOn_DASCompatPatch
    {
        private static bool loggedCompatTrigger;

        public static void Postfix(object[] __args)
        {
            if (!ModsConfig.IsActive("vanillaexpanded.gravship")) return;
            if (Scribe.mode != LoadSaveMode.Inactive || Current.Game == null || Find.World == null) return;

            int tile = TryExtractPlayerSettlementTile(__args);
            if (tile >= 0)
            {
                if (Prefs.DevMode && !loggedCompatTrigger)
                {
                    LogCompat.Message("[DAS] VGE1 compatibility hook triggered on tile " + tile + ".");
                    loggedCompatTrigger = true;
                }

                AbandonmentUtility.ReconcileAfterVanillaAbandon(tile);
            }
        }

        private static int TryExtractPlayerSettlementTile(object[] args)
        {
            if (args == null) return -1;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is MapParent mapParent)
                {
                    if (mapParent is Settlement settlement && settlement.Faction == Faction.OfPlayer)
                        return settlement.Tile;

                    continue;
                }

                if (args[i] is Map map)
                {
                    if (map.Parent is Settlement settlement && settlement.Faction == Faction.OfPlayer)
                        return settlement.Tile;

                    continue;
                }

                if (args[i] is TargetInfo target && target.IsValid && target.Map != null)
                {
                    if (target.Map.Parent is Settlement settlement && settlement.Faction == Faction.OfPlayer)
                        return settlement.Tile;
                }
            }

            return -1;
        }
    }
}
