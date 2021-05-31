namespace Chroma.HarmonyPatches
{
    using Heck;

    // We're replacing this monobehaviour, we dont care about its stuff
    [HeckPatch(typeof(TrackLaneRingsRotationEffect))]
    [HeckPatch("Start")]
    internal static class TrackLaneRingsRotationEffectStart
    {
        private static bool Prefix()
        {
            return false;
        }
    }
}
