using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatDataMirrorTransform))]
    [HarmonyPatch("CreateTransformedData")]
    internal class BeatDataMirrorTransformCreateTransformedData
    {
        private static void Postfix(BeatmapData __result)
        {
            for (int num5 = 0; num5 < __result.beatmapEventData.Length; num5++)
            {
                BeatmapEventData beatmapEventData = __result.beatmapEventData[num5];
                if (beatmapEventData.type.IsRotationEvent())
                {
                    if (beatmapEventData is CustomBeatmapEventData customData)
                    {
                        dynamic dynData = customData.customData;
                        float? rotation = (float?)Trees.at(dynData, ROTATION);

                        if (rotation.HasValue) dynData._rotation = rotation * -1;
                    }
                }
            }
        }
    }
}