using System.Reflection;
using HarmonyLib;
using Verse;

namespace NoNeedAbandonedSettlement
{
    [StaticConstructorOnStartup]
    public static class NoNeedAbandonedSettlement
    {
        static NoNeedAbandonedSettlement()
        {
            var harmony = new Harmony("NoNeedAbandonedSettlement.Jellypowered");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("NoNeedAbandonedSettlement");
        }
    }
}
