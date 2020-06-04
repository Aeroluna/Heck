using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System.Threading.Tasks;
using System.Collections;
using static NoodleExtensions.Animation.AnimationController;

namespace NoodleExtensions.Animation
{
    internal class TrackMovement
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AnimateTrack")
            {
                string position = (string)Trees.at(customEventData.data, "_position");
                float duration = (float?)Trees.at(customEventData.data, "_duration") ?? 1f;

                Dictionary<string, PointData> pointDefintions = ((CustomBeatmapData)_customEventCallbackController._beatmapData).customData.pointDefinitions;
                foreach (KeyValuePair<string, PointData> kvp in pointDefintions)
                {
                    Logger.Log($"Key = {kvp.Key}, Value = {kvp.Value}");
                }
                if (pointDefintions.TryGetValue(position, out PointData pointData))
                {
                    Track track = GetTrack(customEventData);
                    if (track != null)
                    {
                        _instance.StartCoroutine(AnimateTrackCoroutine(pointData, duration, customEventData.time, track));
                    }
                }
            }
        }

        private static IEnumerator AnimateTrackCoroutine(PointData pointData, float duration, float startTime, Track track)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime = _customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = elapsedTime / duration;
                track.position = pointData.Interpolate(time);
                yield return null;
            }
            track.position = pointData.Interpolate(1);
            yield break;
        }
    }
}
