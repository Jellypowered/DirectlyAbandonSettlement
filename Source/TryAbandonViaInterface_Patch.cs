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

            if (__instance is AbandonedSettlement abandonedSettlement)
            {
                var tracker = Find.World.GetComponent<WorldComponent_TileCooldownTracker>();
                if (tracker?.IsOnCooldown(abandonedSettlement.Tile) != true)
                {
                    yield return new Command_Action
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
                }
            }
        }
    }


    [HarmonyPatch(typeof(SettlementAbandonUtility), "TryAbandonViaInterface")]
    public static class TryAbandonViaInterface_Patch
    {
        public static bool Prefix(MapParent settlement)
        {
            Map map = settlement.Map;

            if (map == null || map.mapPawns?.AnyColonistSpawned != true)
            {
                AbandonmentUtility.FinalizeAbandon(settlement);
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                var pawns = map.mapPawns.PawnsInFaction(Faction.OfPlayer);

                if (pawns.Any())
                {
                    stringBuilder.AppendLine("ConfirmAbandonHomeWithColonyPawns".Translate(
                        string.Join("\n", pawns.Select(p => $"    {p.LabelCap}"))
                    ));
                }

                PawnDiedOrDownedThoughtsUtility.BuildMoodThoughtsListString(
                    map.mapPawns.AllPawns,
                    PawnDiedOrDownedThoughtsKind.Banished,
                    stringBuilder,
                    null,
                    "\n\n" + "ConfirmAbandonHomeNegativeThoughts_Everyone".Translate(),
                    "ConfirmAbandonHomeNegativeThoughts"
                );

                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    stringBuilder.ToString(),
                    () => AbandonmentUtility.FinalizeAbandon(settlement),
                    true
                ));
            }

            return false;
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
