namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("Start")]
    internal static class LightSwitchEventEffectStart
    {
        [HarmonyPriority(Priority.High)]
        private static void Postfix(LightSwitchEventEffect __instance, BeatmapEventType ____event)
        {
            new LightColorizer(__instance, ____event);
        }
    }

    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("OnDestroy")]
    internal static class LightSwitchEventEffectOnDestroy
    {
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(BeatmapEventType ____event)
        {
            LightColorizer.Colorizers.Remove(____event);
        }
    }
}
