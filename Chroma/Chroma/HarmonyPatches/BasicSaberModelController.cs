namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(BasicSaberModelController))]
    [HarmonyPatch("Init")]
    internal static class BasicSaberModelControllerInit
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(BasicSaberModelController __instance, SaberType saberType)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            SaberColorizer.BSMStart(__instance, saberType);
        }
    }
}
