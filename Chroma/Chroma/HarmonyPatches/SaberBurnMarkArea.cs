namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("Start")]
    internal class SaberBurnMarkAreaStart
    {
#pragma warning disable SA1313
        private static void Postfix(SaberBurnMarkArea __instance)
#pragma warning restore SA1313
        {
            Extensions.SaberColorizer.SaberBurnMarkArea = __instance;
        }
    }
}
