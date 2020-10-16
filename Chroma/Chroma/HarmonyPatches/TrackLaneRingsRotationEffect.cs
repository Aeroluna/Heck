namespace Chroma.HarmonyPatches
{
    // We're replacing this monobehaviour, we dont care about its stuff
    [ChromaPatch(typeof(TrackLaneRingsRotationEffect))]
    [ChromaPatch("Start")]
    internal static class TrackLaneRingsRotationEffectStart
    {
        private static bool Prefix()
        {
            return false;
        }
    }
}
