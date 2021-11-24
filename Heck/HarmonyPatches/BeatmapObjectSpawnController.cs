using HarmonyLib;
using JetBrains.Annotations;

namespace Heck.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    [HarmonyPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        internal static BeatmapObjectSpawnController BeatmapObjectSpawnController { get; private set; } = null!;

        [UsedImplicitly]
        private static void Postfix(BeatmapObjectSpawnController __instance)
        {
            BeatmapObjectSpawnController = __instance;
        }
    }
}
