namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(SaberManager))]
    [HarmonyPatch("Start")]
    internal static class SaberManagerStart
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(Saber ____leftSaber, Saber ____rightSaber)
        {
            SaberColorizer.BSMStart(____leftSaber, ____leftSaber.saberType);
            SaberColorizer.BSMStart(____rightSaber, ____rightSaber.saberType);
        }
    }
}
