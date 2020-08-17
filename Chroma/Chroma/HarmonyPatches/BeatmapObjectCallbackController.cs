namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapObjectCallbackController))]
    [HarmonyPatch("Start")]
    internal static class BeatmapObjectCallbackControllerStart
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(BeatmapObjectCallbackController __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            __instance.StartCoroutine(ChromaController.DelayedStart());
        }
    }
}
