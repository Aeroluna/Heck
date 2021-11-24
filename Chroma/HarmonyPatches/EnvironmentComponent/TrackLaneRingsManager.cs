using System.Collections.Generic;
using Chroma.Lighting.EnvironmentEnhancement;
using Heck;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    [HeckPatch(typeof(TrackLaneRingsManager))]
    [HeckPatch("Awake")]
    internal static class TrackLaneRingsManagerAwake
    {
        internal static List<TrackLaneRingsManager> RingManagers { get; } = new();

        [UsedImplicitly]
        private static bool Prefix()
        {
            return !ComponentInitializer.SkipAwake;
        }

        [UsedImplicitly]
        private static void Postfix(TrackLaneRingsManager __instance)
        {
            RingManagers.Add(__instance);
        }
    }
}
