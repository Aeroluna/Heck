using System.Collections.Generic;
using Heck;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    [HeckPatch(typeof(LightWithIdManager))]
    [HeckPatch("LateUpdate")]
    internal static class LightWithIdManagerLateUpdate
    {
        [UsedImplicitly]
        private static void Postfix(List<ILightWithId> ____lightsToUnregister)
        {
            // For some reason this doesnt get emptied and is continuously looped over.
            // When unregistering a large amount of lights in Chroma, this can add lag, so we dump the list ourselves.
            ____lightsToUnregister.Clear();
        }
    }
}
