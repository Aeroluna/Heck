namespace Chroma.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static Plugin;

    [ChromaPatch(typeof(LightRotationEventEffect))]
    [ChromaPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static bool Prefix(
            BeatmapEventData beatmapEventData,
            LightRotationEventEffect __instance,
            BeatmapEventType ____event,
            Quaternion ____startRotation,
            ref float ____rotationSpeed,
            Vector3 ____rotationVector)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (beatmapEventData.type == ____event)
            {
                if (beatmapEventData is CustomBeatmapEventData customData)
                {
                    bool isLeftEvent = ____event == BeatmapEventType.Event12;

                    dynamic dynData = customData.customData;

                    bool lockPosition = ((bool?)Trees.at(dynData, LOCKPOSITION)).GetValueOrDefault(false);

                    float precisionSpeed = ((float?)Trees.at(dynData, PRECISESPEED)).GetValueOrDefault(beatmapEventData.value);

                    int? dir = (int?)Trees.at(dynData, DIRECTION);
                    dir = dir.GetValueOrDefault(-1);

                    float direction = (Random.value > 0.5f) ? 1f : -1f;
                    switch (dir)
                    {
                        case 0:
                            direction = isLeftEvent ? -1 : 1;
                            break;

                        case 1:
                            direction = isLeftEvent ? 1 : -1;
                            break;
                    }

                    // Actual lasering
                    if (beatmapEventData.value == 0)
                    {
                        __instance.enabled = false;
                        if (!lockPosition)
                        {
                            __instance.transform.localRotation = ____startRotation;
                        }
                    }
                    else if (beatmapEventData.value > 0)
                    {
                        __instance.enabled = true;
                        ____rotationSpeed = precisionSpeed * 20f * direction;
                        if (!lockPosition)
                        {
                            __instance.transform.localRotation = ____startRotation;
                            __instance.transform.Rotate(____rotationVector, Random.Range(0f, 180f), Space.Self);
                        }
                    }

                    return false;
                }
            }

            return true;
        }
    }
}
