using Harmony;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("Start")]
    internal class SaberBurnMarkAreaStart
    {
        public static void Postfix(ref SaberBurnMarkArea __instance)
        {
            Extensions.SaberColourizer.saberBurnMarkArea = __instance;
        }
    }
}