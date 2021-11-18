namespace Chroma.HarmonyPatches
{
    using System.Collections;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("Start")]
    internal static class LightSwitchEventEffectStart
    {
        private static bool Prefix(LightSwitchEventEffect __instance)
        {
            __instance.StartCoroutine(WaitThenStart(__instance));
            return false;
        }

        // For some reason, not waiting for end of frame causes the SO initializer to grab colors from the previous map, so whatever
        private static IEnumerator WaitThenStart(LightSwitchEventEffect instance)
        {
            yield return new WaitForEndOfFrame();
            LightSwitchEventEffect oldEffect = instance;
            ChromaLightSwitchEventEffect newEffect = oldEffect.gameObject.AddComponent<ChromaLightSwitchEventEffect>();
            newEffect.CopyValues(oldEffect);
            Object.Destroy(oldEffect);
        }
    }

    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        // Allow other patches to still run
        [HarmonyPriority(Priority.Last)]
        private static bool Prefix()
        {
            return false;
        }
    }
}
