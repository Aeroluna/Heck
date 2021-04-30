namespace Heck.HarmonyPatches
{
    using System.Diagnostics;
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
                Animation.HeckEventDataManager.DeserializeBeatmapData(__result);
            }
        }
    }
}
