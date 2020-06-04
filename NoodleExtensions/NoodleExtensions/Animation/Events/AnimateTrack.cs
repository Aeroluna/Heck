using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using static NoodleExtensions.Animation.AnimationController;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions.Animation
{
    internal class AnimateTrack
    {
        private static Dictionary<Track, Coroutine> _activeCoroutines = new Dictionary<Track, Coroutine>();
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AnimateTrack")
            {
                Track track = GetTrack(customEventData);
                if (track != null)
                {
                    dynamic positionString = Trees.at(customEventData.data, "_position");
                    dynamic rotationString = Trees.at(customEventData.data, "_rotation");
                    dynamic scaleString = Trees.at(customEventData.data, "_scale");
                    dynamic localRotationString = Trees.at(customEventData.data, "_localRotation");
                    float duration = (float?)Trees.at(customEventData.data, "_duration") ?? 1f;

                    PointData position = null;
                    PointData rotation = null;
                    PointData scale = null;
                    PointData localRotation = null;

                    Dictionary<string, PointData> pointDefintions = Trees.at(((CustomBeatmapData)_customEventCallbackController._beatmapData).customData, "pointDefinitions");

                    if (positionString is string) pointDefintions.TryGetValue(positionString, out position);
                    else position = DynamicToPointData(positionString);
                    if (rotationString is string) pointDefintions.TryGetValue(rotationString, out rotation);
                    else rotation = DynamicToPointData(rotationString);
                    if (scaleString is string) pointDefintions.TryGetValue(scaleString, out scale);
                    else scale = DynamicToPointData(scaleString);
                    if (localRotationString is string) pointDefintions.TryGetValue(localRotationString, out localRotation);
                    else localRotation = DynamicToPointData(localRotationString);
                    if (_activeCoroutines.TryGetValue(track, out Coroutine coroutine) && coroutine != null) _instance.StopCoroutine(coroutine);
                    _activeCoroutines[track] = _instance.StartCoroutine(AnimateTrackCoroutine(position, rotation, scale, localRotation, duration, customEventData.time, track));
                }
            }
        }

        private static PointData DynamicToPointData(dynamic dyn)
        {
            IEnumerable<IEnumerable<float>> points = ((IEnumerable<object>)dyn)
                        ?.Cast<IEnumerable<object>>()
                        .Select(n => n.Select(Convert.ToSingle));
            if (points == null) return null;

            PointData pointData = new PointData();
            foreach (IEnumerable<float> rawPoint in points)
            {
                pointData.Add(new Vector4(rawPoint.ElementAt(0), rawPoint.ElementAt(1), rawPoint.ElementAt(2), rawPoint.ElementAt(3)));
            }
            return pointData;
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
            if (rotation != null) track.rotation = rotation.Interpolate(1);
            if (scale != null) track.scale = scale.Interpolate(1);
            if (localRotation != null) track.localRotation = localRotation.Interpolate(1);
            _activeCoroutines.Remove(track);
            yield break;
        }
    }
}
