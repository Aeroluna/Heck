namespace Heck.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    [HarmonyPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        internal static BeatmapObjectSpawnController? BeatmapObjectSpawnController { get; private set; }

        private static void Postfix(BeatmapObjectSpawnController __instance)
        {
            BeatmapObjectSpawnController = __instance;
        }
    }
}
