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
                Track track = GetTrack(customEventData);
                if (track != null)
                {
                    string positionString = (string)Trees.at(customEventData.data, "_position");
                    string rotationString = (string)Trees.at(customEventData.data, "_rotation");
                    string scaleString = (string)Trees.at(customEventData.data, "_scale");
                    string localRotationString = (string)Trees.at(customEventData.data, "_localRotation");
                    float duration = (float?)Trees.at(customEventData.data, "_duration") ?? 1f;

                    Dictionary<string, PointData> pointDefintions = ((CustomBeatmapData)_customEventCallbackController._beatmapData).customData.pointDefinitions;

                    PointData position = null;
                    PointData rotation = null;
                    PointData scale = null;
                    PointData localRotation = null;
                    if (positionString != null) pointDefintions.TryGetValue(positionString, out position);
                    if (rotationString != null) pointDefintions.TryGetValue(rotationString, out rotation);
                    if (scaleString != null) pointDefintions.TryGetValue(scaleString, out scale);
                    if (localRotationString != null) pointDefintions.TryGetValue(localRotationString, out localRotation);
                    _instance.StartCoroutine(AnimateTrackCoroutine(position, rotation, scale, localRotation, duration, customEventData.time, track));
                }
            }
        }

        private static IEnumerator AnimateTrackCoroutine(PointData position, PointData rotation, PointData scale, PointData localRotation,
            float duration, float startTime, Track track)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime = _customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = elapsedTime / duration;
                if (position != null) track.position = position.Interpolate(time);
                if (rotation != null) track.rotation = rotation.Interpolate(time);
                if (scale != null) track.scale = scale.Interpolate(time);
                if (localRotation != null) track.localRotation = localRotation.Interpolate(time);
                yield return null;
            }
            if (position != null) track.position = position.Interpolate(1);
            yield break;
        }
    }
}
