using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NoNeedAbandonedSettlement
{
    [StaticConstructorOnStartup]
    public static class NoNeedAbandonedSettlement
    {
        private const string HarmonyId = "NoNeedAbandonedSettlement.Jellypowered";
        private const string Vge1PackageId = "vanillaexpanded.gravship";

        static NoNeedAbandonedSettlement()
        {
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            LogCompat.Message("[DAS] Mod initialized.");

            if (ModsConfig.IsActive(Vge1PackageId))
            {
                MethodInfo target = AccessTools.Method(typeof(RitualBehaviorWorker_GravshipLaunch), nameof(RitualBehaviorWorker_GravshipLaunch.TryExecuteOn));
                Patches patches = target != null ? Harmony.GetPatchInfo(target) : null;
                bool patched = patches != null && patches.Postfixes.Any(patch => patch.owner == HarmonyId);

                if (patched)
                    LogCompat.Message("[DAS] VGE1 compatibility patch applied successfully.");
            }
        }
    }
}
