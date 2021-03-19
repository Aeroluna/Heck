namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("Start")]
    internal static class SaberBurnMarkAreaStart
    {
        private static void Postfix(SaberBurnMarkArea __instance)
        {
            Colorizer.SaberColorizer.SaberBurnMarkArea = __instance;
        }
    }
}
