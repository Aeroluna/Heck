namespace Chroma.HarmonyPatches
{
    using System.Diagnostics;
    using Chroma;
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapDataTransformHelper))]
    [HarmonyPatch("CreateTransformedBeatmapData")]
    internal static class BeatmapDataTransformHelperCreateTransformedBeatmapData
    {
        private static void Postfix(IReadonlyBeatmapData __result)
        {
            // Skip if calling class is MultiplayerConnectPlayerInstaller
            StackTrace stackTrace = new StackTrace();
            if (!stackTrace.GetFrame(2).GetMethod().Name.Contains("MultiplayerConnectedPlayerInstaller"))
            {
                ChromaObjectDataManager.DeserializeBeatmapData(__result);
                ChromaEventDataManager.DeserializeBeatmapData(__result);
            }
        }
    }
}
