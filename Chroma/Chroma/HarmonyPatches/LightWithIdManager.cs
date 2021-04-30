namespace Chroma.HarmonyPatches
{
    using Heck;
    using System.Collections.Generic;

    [HeckPatch(typeof(LightWithIdManager))]
    [HeckPatch("LateUpdate")]
    internal static class LightWithIdManagerLateUpdate
    {
        private static void Postfix(List<ILightWithId> ____lightsToUnregister)
        {
            // For some reason this doesnt get emptied and is continuously looped over.
            // When unregistering a large amount of lights in Chroma, this can add lag, so we dump the list ourselves.
            ____lightsToUnregister.Clear();
        }
    }
}
