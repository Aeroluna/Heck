namespace NoodleExtensions.HarmonyPatches
{
    using Chroma;
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapDataTransformHelper))]
    [HarmonyPatch("CreateTransformedBeatmapData")]
    internal static class BeatmapDataTransformHelperCreateTransformedBeatmapData
    {
        private static void Postfix(IReadonlyBeatmapData __result)
        {
            ChromaObjectDataManager.DeserializeBeatmapData(__result);
            ChromaEventDataManager.DeserializeBeatmapData(__result);
        }
    }
}
