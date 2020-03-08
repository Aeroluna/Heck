using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatDataMirrorTransform))]
    [HarmonyPatch("CreateTransformedData")]
    internal class BeatDataMirrorTransformCreateTransformedData
    {
        private static void Postfix(ref BeatmapData __result)
        {
            for (int num5 = 0; num5 < __result.beatmapEventData.Length; num5++)
            {
                BeatmapEventData beatmapEventData = __result.beatmapEventData[num5];
                if (beatmapEventData.type.IsRotationEvent())
                {
                    if (beatmapEventData is CustomBeatmapEventData customData)
                    {
                        dynamic dynData = customData.customData;
                        float? _rotation = (float?)Trees.at(dynData, ROTATION);

                        if (_rotation.HasValue) dynData._rotation = _rotation * -1;
                    }
                }
            }
        }
    }
}