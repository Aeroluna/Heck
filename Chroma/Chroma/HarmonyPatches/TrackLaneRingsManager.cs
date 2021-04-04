namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;

    [ChromaPatch(typeof(TrackLaneRingsManager))]
    [ChromaPatch("Awake")]
    internal static class TrackLaneRingsManagerAwake
    {
        internal static List<TrackLaneRingsManager> RingManagers { get; } = new List<TrackLaneRingsManager>();

        private static void Postfix(TrackLaneRingsManager __instance)
        {
            RingManagers.Add(__instance);
        }
    }
}
