namespace Chroma.HarmonyPatches
{
    [ChromaPatch(typeof(TrackLaneRing))]
    [ChromaPatch("FixedUpdateRing")]
    internal static class TrackLaneRingFixedUpdateRing
    {
        private static bool Prefix(TrackLaneRing __instance)
        {
            if (EnvironmentEnhancementManager.SkipRingUpdate != null && EnvironmentEnhancementManager.SkipRingUpdate.TryGetValue(__instance, out bool doSkip))
            {
                return !doSkip;
            }

            return true;
        }
    }

    [ChromaPatch(typeof(TrackLaneRing))]
    [ChromaPatch("LateUpdateRing")]
    internal static class TrackLaneRingLateUpdateRing
    {
        private static bool Prefix(TrackLaneRing __instance)
        {
            if (EnvironmentEnhancementManager.SkipRingUpdate != null && EnvironmentEnhancementManager.SkipRingUpdate.TryGetValue(__instance, out bool doSkip))
            {
                return !doSkip;
            }

            return true;
        }
    }
}
