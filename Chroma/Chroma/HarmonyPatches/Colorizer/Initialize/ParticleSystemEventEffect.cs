namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("Start")]
    internal static class ParticleSystemEventEffectStart
    {
        [HarmonyPriority(Priority.High)]
        private static void Postfix(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
        {
            new ParticleColorizer(__instance, ____colorEvent);
        }
    }

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("OnDestroy")]
    internal static class ParticleSystemEventEffectOnDestroy
    {
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(BeatmapEventType ____colorEvent)
        {
            if (____colorEvent.TryGetParticleColorizer(out ParticleColorizer particleColorizer))
            {
                particleColorizer.UnsubscribeEvent();
            }

            ParticleColorizer.Colorizers.Remove(____colorEvent);
        }
    }

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class ParticleSystemEventEffectSetLastEvent
    {
        private static void Prefix(BeatmapEventData beatmapEventData, BeatmapEventType ____colorEvent)
        {
            if (beatmapEventData.type == ____colorEvent && ParticleColorizer.Colorizers.TryGetValue(____colorEvent, out ParticleColorizer particleColorizer))
            {
                particleColorizer.PreviousValue = beatmapEventData.value;
            }
        }
    }
}
