namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(Saber))]
    [HarmonyPatch("Start")]
    internal static class SaberStart
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(Saber __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            SaberColorizer.BSMStart(__instance, __instance.saberType);
        }
    }
}
