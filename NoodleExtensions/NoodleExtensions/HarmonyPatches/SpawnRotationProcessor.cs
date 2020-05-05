using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(SpawnRotationProcessor))]
    [NoodlePatch("ProcessBeatmapEventData")]
    internal class SpawnRotationProcessorProcessBeatmapEventData
    {
        private static bool Prefix(BeatmapEventData beatmapEventData, ref float ____rotation)
        {
            if (beatmapEventData.type.IsRotationEvent() && beatmapEventData is CustomBeatmapEventData customData)
            {
                dynamic dynData = customData.customData;
                float? _rotation = (float?)Trees.at(dynData, ROTATION);

                if (_rotation.HasValue)
                {
                    ____rotation += _rotation.Value;
                    return false;
                }
            }

            return true;
        }
    }
}