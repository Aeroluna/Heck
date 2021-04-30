namespace Chroma.HarmonyPatches
{
    using Heck;
    using System.Collections.Generic;

    [HeckPatch(typeof(TrackLaneRingsManager))]
    [HeckPatch("Awake")]
    internal static class TrackLaneRingsManagerAwake
    {
        internal static List<TrackLaneRingsManager> RingManagers { get; } = new List<TrackLaneRingsManager>();

        private static bool Prefix()
        {
            if (ComponentInitializer.SkipAwake)
            {
                return false;
            }

            return true;
        }

        private static void Postfix(TrackLaneRingsManager __instance)
        {
            RingManagers.Add(__instance);
        }
    }
}
