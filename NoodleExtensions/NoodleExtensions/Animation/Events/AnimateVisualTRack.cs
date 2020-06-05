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
using static NoodleExtensions.Plugin;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions.Animation
{
    // TODO: like all this stuff
    internal class AnimateVisualTrack
    {
        private static Dictionary<Track, Coroutine> _activeCoroutines = new Dictionary<Track, Coroutine>();
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AnimateVisualTrack")
            {
                Track track = GetTrack(customEventData);
                if (track != null)
                {
                    dynamic positionString = Trees.at(customEventData.data, POSITION);
                    dynamic rotationString = Trees.at(customEventData.data, ROTATION);
                    dynamic scaleString = Trees.at(customEventData.data, SCALE);
                    dynamic localRotationString = Trees.at(customEventData.data, LOCALROTATION);
                    float duration = (float)Trees.at(customEventData.data, DURATION);

                    PointData position;
                    PointData rotation;
                    PointData scale;
                    PointData localRotation;

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
                    _activeCoroutines[track] = _instance.StartCoroutine(AnimateVisualTrackCoroutine(position, rotation, scale, localRotation, duration, customEventData.time, track));
                }
            }
        }

        private static IEnumerator AnimateVisualTrackCoroutine(PointData position, PointData rotation, PointData scale, PointData localRotation,
            float duration, float startTime, Track track)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime = _customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = elapsedTime / duration;
                if (position != null) track.visualPosition = position.Interpolate(time);
                if (rotation != null) track.visualRotation = rotation.Interpolate(time);
                if (scale != null) track.visualScale = scale.Interpolate(time);
                if (localRotation != null) track.visualLocalRotation = localRotation.Interpolate(time);
                yield return null;
            }
            if (position != null) track.visualPosition = position.Interpolate(1);
            if (rotation != null) track.visualRotation = rotation.Interpolate(1);
            if (scale != null) track.visualScale = scale.Interpolate(1);
            if (localRotation != null) track.visualLocalRotation = localRotation.Interpolate(1);
            _activeCoroutines.Remove(track);
            yield break;
        }
    }
}
