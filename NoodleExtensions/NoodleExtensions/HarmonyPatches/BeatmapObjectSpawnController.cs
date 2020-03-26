using HarmonyLib;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    [HarmonyPatch("Start")]
    internal class BeatmapObjectSpawnControllerStart
    {
        private static void Postfix(BeatmapObjectSpawnController __instance)
        {
            NoodleController.InitBeatmapObjectSpawnController(__instance);
        }
    }
}