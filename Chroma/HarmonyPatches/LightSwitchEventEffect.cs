using System.Collections;
using Chroma.Lighting;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("Start")]
    internal static class LightSwitchEventEffectStart
    {
        [UsedImplicitly]
        private static bool Prefix(LightSwitchEventEffect __instance)
        {
            __instance.StartCoroutine(WaitThenStart(__instance));
            return false;
        }

        // For some reason, not waiting for end of frame causes the SO initializer to grab colors from the previous map, so whatever
        private static IEnumerator WaitThenStart(LightSwitchEventEffect instance)
        {
            yield return new WaitForEndOfFrame();
            ChromaLightSwitchEventEffect newEffect = instance.gameObject.AddComponent<ChromaLightSwitchEventEffect>();
            newEffect.CopyValues(instance);
            Object.Destroy(instance);
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
