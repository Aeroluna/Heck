using HarmonyLib;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("Start")]
    internal class SaberBurnMarkAreaStart
    {
        private static void Postfix(SaberBurnMarkArea __instance)
        {
            Extensions.SaberColourizer.saberBurnMarkArea = __instance;
        }
    }
}