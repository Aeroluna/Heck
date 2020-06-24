using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.NullableExtensions;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.Plugin;
using static NoodleExtensions.Animation.AnimationController;

namespace NoodleExtensions.Animation
{
    public static class AnimationHelper
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

        internal static void AddTrackProperties(Track track)
        {
            IDictionary<string, Property> properties = track._properties;
            properties.Add(POSITION, new Property(PropertyType.Vector3));
            properties.Add(ROTATION, new Property(PropertyType.Quaternion));
            properties.Add(SCALE, new Property(PropertyType.Vector3));
            properties.Add(LOCALROTATION, new Property(PropertyType.Quaternion));
            properties.Add(DISSOLVE, new Property(PropertyType.Linear));
            properties.Add(DISSOLVEARROW, new Property(PropertyType.Linear));

            IDictionary<string, Property> pathProperties = track._pathProperties;
            pathProperties.Add(POSITION, new Property(PropertyType.Vector3));
            pathProperties.Add(ROTATION, new Property(PropertyType.Quaternion));
            pathProperties.Add(SCALE, new Property(PropertyType.Vector3));
            pathProperties.Add(LOCALROTATION, new Property(PropertyType.Quaternion));
            pathProperties.Add(DEFINITEPOSITION, new Property(PropertyType.Vector3));
            pathProperties.Add(DISSOLVE, new Property(PropertyType.Linear));
            pathProperties.Add(DISSOLVEARROW, new Property(PropertyType.Linear));
        }

        internal static void GetDefinitePositionOffset(dynamic customData, Track track, float time, out Vector3? definitePosition)
        {
            TryGetPointData(customData, DEFINITEPOSITION, out PointData localDefinitePosition);

            Vector3? pathDefinitePosition = localDefinitePosition?.Interpolate(time) ?? TryGetPathProperty(track, DEFINITEPOSITION, time);

            if (pathDefinitePosition.HasValue) definitePosition = SumVectorNullables(TryGetProperty(track, POSITION), pathDefinitePosition) * _noteLinesDistance;
            else definitePosition = null;
        }

        internal static void GetObjectOffset(dynamic customData, Track track, float time, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? dissolveArrow)
        {
            GetAllPointData(customData, out PointData localPosition, out PointData localRotation, out PointData localScale, out PointData localLocalRotation, out PointData localDissolve, out PointData localDissolveArrow);

            Vector3? pathPosition = localPosition?.Interpolate(time) ?? TryGetPathProperty(track, POSITION, time);
            Quaternion? pathRotation = localRotation?.InterpolateQuaternion(time) ?? TryGetPathProperty(track, ROTATION, time);
            Vector3? pathScale = localScale?.Interpolate(time) ?? TryGetPathProperty(track, SCALE, time);
            Quaternion? pathLocalRotation = localLocalRotation?.InterpolateQuaternion(time) ?? TryGetPathProperty(track, LOCALROTATION, time);
            float? pathDissolve = localDissolve?.InterpolateLinear(time) ?? TryGetPathProperty(track, DISSOLVE, time);
            float? pathDissolveArrow = localDissolveArrow?.InterpolateLinear(time) ?? TryGetPathProperty(track, DISSOLVEARROW, time);

            positionOffset = SumVectorNullables(TryGetProperty(track, POSITION), pathPosition) * _noteLinesDistance;
            rotationOffset = MultQuaternionNullables(TryGetProperty(track, ROTATION), pathRotation);
            scaleOffset = MultVectorNullables(TryGetProperty(track, SCALE), pathScale);
            localRotationOffset = MultQuaternionNullables(TryGetProperty(track, LOCALROTATION), pathLocalRotation);
            dissolve = MultFloatNullables(TryGetProperty(track, DISSOLVE), pathDissolve);
            dissolveArrow = MultFloatNullables(TryGetProperty(track, DISSOLVEARROW), pathDissolveArrow);
        }

        public static dynamic TryGetPathProperty(Track track, string propertyName, float time)
        {
            Property pathProperty = null;
            track?._pathProperties.TryGetValue(propertyName, out pathProperty);
            if (pathProperty == null) return null;
            PointDataInterpolation pointDataInterpolation = (PointDataInterpolation)pathProperty._property;

            switch (pathProperty._propertyType)
            {
                case PropertyType.Linear:
                    return pointDataInterpolation.InterpolateLinear(time);
                case PropertyType.Quaternion:
                    return pointDataInterpolation.InterpolateQuaternion(time);
                case PropertyType.Vector3:
                    return pointDataInterpolation.Interpolate(time);
                default:
                    return null;
            }
        }

        public static dynamic TryGetProperty(Track track, string propertyName)
        {
            Property property = null;
            track?._properties.TryGetValue(propertyName, out property);
            return property?._property;
        }

        internal static void GetAllPointData(dynamic customData, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation, out PointData dissolve, out PointData dissolveArrow)
        {
            Dictionary<string, PointData> pointDefinitions = _pointDefinitions;

            TryGetPointData(customData, POSITION, out position, pointDefinitions);
            TryGetPointData(customData, ROTATION, out rotation, pointDefinitions);
            TryGetPointData(customData, SCALE, out scale, pointDefinitions);
            TryGetPointData(customData, LOCALROTATION, out localRotation, pointDefinitions);
            TryGetPointData(customData, DISSOLVE, out dissolve, pointDefinitions);
            TryGetPointData(customData, DISSOLVEARROW, out dissolveArrow, pointDefinitions);
        }

        public static void TryGetPointData(dynamic customData, string pointName, out PointData pointData, Dictionary<string, PointData> pointDefinitions = null)
        {
            if (pointDefinitions == null) pointDefinitions = _pointDefinitions;
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

        public static Track GetTrack(dynamic customData)
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
    }
}
