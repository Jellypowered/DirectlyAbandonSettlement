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
}
