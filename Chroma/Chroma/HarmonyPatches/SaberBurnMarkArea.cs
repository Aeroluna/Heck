namespace Chroma.HarmonyPatches
{
    [ChromaPatch(typeof(SaberBurnMarkArea))]
    [ChromaPatch("Start")]
    internal static class SaberBurnMarkAreaStart
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(SaberBurnMarkArea __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            Extensions.SaberColorizer.SaberBurnMarkArea = __instance;
        }
    }
}
