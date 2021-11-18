namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using Heck;

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
