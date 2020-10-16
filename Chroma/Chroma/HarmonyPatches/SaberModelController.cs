namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(SaberModelController))]
    [HarmonyPatch("Init")]
    internal static class SaberModelControllerInit
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(SaberModelController __instance, Saber saber)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            SaberColorizer.BSMStart(__instance, saber.saberType);
        }
    }
}
