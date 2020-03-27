using HarmonyLib;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("Start")]
    internal class SaberBurnMarkAreaStart
    {
        private static void Postfix(ref SaberBurnMarkArea __instance)
        {
            Extensions.SaberColourizer.saberBurnMarkArea = __instance;
        }
    }
}