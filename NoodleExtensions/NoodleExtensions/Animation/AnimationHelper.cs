using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.Plugin;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions.Animation
{
    internal static class AnimationHelper
    {
        private static Dictionary<string, Track> _tracks { get => ((CustomBeatmapData)AnimationController.instance.customEventCallbackController._beatmapData).customData.tracks; }
        private static Dictionary<string, PointData> _pointDefinitions { get => Trees.at(((CustomBeatmapData)AnimationController.instance.customEventCallbackController._beatmapData).customData, "pointDefinitions"); }

        private static BeatmapObjectManager _beatmapObjectManager;

        private static BeatmapObjectManager beatmapObjectManager
        {
            get
            {
                if (_beatmapObjectManager == null) _beatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().First();
                return _beatmapObjectManager;
            }
        }

        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _noteAPoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_noteAPool");
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _noteBPoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_noteBPool");
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _bombNotePoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_bombNotePool");
        private static readonly FieldAccessor<BeatmapObjectManager, ObstacleController.Pool>.Accessor _obstaclePoolAccessor = FieldAccessor<BeatmapObjectManager, ObstacleController.Pool>.GetAccessor("_obstaclePool");

        internal static void GetDefinitePosition(dynamic customData, out PointData position)
        {
            TryGetPointData(_pointDefinitions, customData, DEFINITEPOSITION, out position);
        }

        internal static void GetObjectOffset(dynamic customData, Track track, float time, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? dissolveArrow)
        {
            AnimationHelper.GetAllPointData(customData, out PointData localPosition, out PointData localRotation, out PointData localScale, out PointData localLocalRotation, out PointData localDissolve, out PointData localDissolveArrow);

            Vector3? pathPosition = localPosition?.Interpolate(time) ?? track?._pathPosition.Interpolate(time);
            Quaternion? pathRotation = localRotation?.InterpolateQuaternion(time) ?? track?._pathRotation.InterpolateQuaternion(time);
            Vector3? pathScale = localScale?.Interpolate(time) ?? track?._pathScale.Interpolate(time);
            Quaternion? pathLocalRotation = localLocalRotation?.InterpolateQuaternion(time) ?? track?._pathLocalRotation.InterpolateQuaternion(time);
            float? pathDissolve = localDissolve?.InterpolateLinear(time) ?? track?._pathDissolve.InterpolateLinear(time);
            float? pathDissolveArrow = localDissolveArrow?.InterpolateLinear(time) ?? track?._pathDissolveArrow.InterpolateLinear(time);

            positionOffset = SumVectorNullables(track?._position, pathPosition) * _noteLinesDistance;
            rotationOffset = MultQuaternionNullables(track?._rotation, pathRotation);
            scaleOffset = MultVectorNullables(track?._scale, pathScale);
            localRotationOffset = MultQuaternionNullables(track?._localRotation, pathLocalRotation);
            dissolve = MultFloatNullables(track?._dissolve, pathDissolve);
            dissolveArrow = MultFloatNullables(track?._dissolveArrow, pathDissolveArrow);
        }

        internal static void GetAllPointData(dynamic customData, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation, out PointData dissolve, out PointData dissolveArrow)
        {
            Dictionary<string, PointData> pointDefinitions = _pointDefinitions;

            TryGetPointData(pointDefinitions, customData, POSITION, out position);
            TryGetPointData(pointDefinitions, customData, ROTATION, out rotation);
            TryGetPointData(pointDefinitions, customData, SCALE, out scale);
            TryGetPointData(pointDefinitions, customData, LOCALROTATION, out localRotation);
            TryGetPointData(pointDefinitions, customData, DISSOLVE, out dissolve);
            TryGetPointData(pointDefinitions, customData, DISSOLVEARROW, out dissolveArrow);
        }

        private static void TryGetPointData(Dictionary<string, PointData> pointDefinitions, dynamic customData, string pointName, out PointData pointData)
        {
            dynamic pointString = Trees.at(customData, pointName);
            if (pointString is PointData castedData)
            {
                pointData = castedData;
            }
            else if (pointString is string)
            {
                if (!pointDefinitions.TryGetValue(pointString, out pointData))
                {
                    Logger.Log($"Could not find point definition {pointString}!", IPA.Logging.Logger.Level.Error);
                    pointData = null;
                }
            }
            else
            {
                pointData = PointData.DynamicToPointData(pointString);
                if (pointData != null) ((IDictionary<string, object>)customData)[pointName] = pointData;
            }
        }

        internal static Track GetTrack(dynamic customData)
        {
            string trackName = Trees.at(customData, TRACK);
            if (trackName == null) return null;
            if (_tracks.TryGetValue(trackName, out Track track))
            {
                return track;
            }
            else
            {
                Logger.Log($"Could not find track {trackName}!", IPA.Logging.Logger.Level.Error);
                return null;
            }
        }

        internal static Vector3? SumVectorNullables(Vector3? vectorOne, Vector3? vectorTwo)
        {
            if (!vectorOne.HasValue && !vectorTwo.HasValue) return null;
            Vector3 total = _vectorZero;
            if (vectorOne.HasValue) total += vectorOne.Value;
            if (vectorTwo.HasValue) total += vectorTwo.Value;
            return total;
        }
        internal static Vector3? MultVectorNullables(Vector3? vectorOne, Vector3? vectorTwo)
        {
            if (vectorOne.HasValue)
            {
                if (vectorTwo.HasValue) return Vector3.Scale(vectorOne.Value, vectorTwo.Value);
                else return vectorOne;
            }
            else if (vectorTwo.HasValue)
            {
                return vectorTwo;
            }
            return null;
        }
        internal static Quaternion? MultQuaternionNullables(Quaternion? quaternionOne, Quaternion? quaternionTwo)
        {
            if (quaternionOne.HasValue)
            {
                if (quaternionTwo.HasValue) return quaternionOne.Value * quaternionTwo.Value;
                else return quaternionOne;
            }
            else if (quaternionTwo.HasValue)
            {
                return quaternionTwo;
            }
            return null;
        }
        internal static float? MultFloatNullables(float? floatOne, float? floatTwo)
        {
            if (floatOne.HasValue)
            {
                if (floatTwo.HasValue) return floatOne.Value * floatTwo.Value;
                else return floatOne;
            }
            else if (floatTwo.HasValue)
            {
                return floatTwo;
            }
            return null;
        }
    }
}