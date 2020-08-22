namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("Start")]
    internal static class SaberBurnMarkAreaStart
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(SaberBurnMarkArea __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            Colorizer.SaberColorizer.SaberBurnMarkArea = __instance;
        }
    }
}
