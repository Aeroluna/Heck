using System.Collections.Generic;
using HarmonyLib;
using Heck;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(LightWithIdManager))]
    internal static class LightWithIdManagerDumpLightsToUnregister
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LightWithIdManager.LateUpdate))]
        private static void Postfix(List<ILightWithId> ____lightsToUnregister)
        {
            // For some reason this doesnt get emptied and is continuously looped over.
            // When unregistering a large amount of lights in Chroma, this can add lag, so we dump the list ourselves.
            ____lightsToUnregister.Clear();
        }
    }
}
