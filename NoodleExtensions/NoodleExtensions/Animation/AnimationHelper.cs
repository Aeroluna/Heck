namespace NoodleExtensions.Animation
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.NullableExtensions;
    using static NoodleExtensions.Plugin;

    public static class AnimationHelper
    {
        private static BeatmapObjectManager beatmapObjectManager;

        private static Dictionary<string, Track> Tracks { get => ((CustomBeatmapData)AnimationController.Instance.CustomEventCallbackController._beatmapData).customData.tracks; }

        private static Dictionary<string, PointDefinition> PointDefinitions { get => Trees.at(((CustomBeatmapData)AnimationController.Instance.CustomEventCallbackController._beatmapData).customData, "pointDefinitions"); }

        private static BeatmapObjectManager BeatmapObjectManager
        {
            get
            {
                if (beatmapObjectManager == null)
                {
                    beatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().First();
                }

                return beatmapObjectManager;
            }
        }

        public static dynamic TryGetPathProperty(Track track, string propertyName, float time)
        {
            Property pathProperty = null;
            track?.PathProperties.TryGetValue(propertyName, out pathProperty);
            if (pathProperty == null)
            {
                return null;
            }

            PointDefinitionInterpolation pointDataInterpolation = (PointDefinitionInterpolation)pathProperty.Value;

            switch (pathProperty.PropertyType)
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
            track?.Properties.TryGetValue(propertyName, out property);
            return property?.Value;
        }

        public static void TryGetPointData(dynamic customData, string pointName, out PointDefinition pointData, Dictionary<string, PointDefinition> pointDefinitions = null)
        {
            if (pointDefinitions == null)
            {
                pointDefinitions = PointDefinitions;
            }

            dynamic pointString = Trees.at(customData, pointName);
            if (pointString is PointDefinition castedData)
            {
                pointData = castedData;
            }
            else if (pointString is string)
            {
                if (!pointDefinitions.TryGetValue(pointString, out pointData))
                {
                    NoodleLogger.Log($"Could not find point definition {pointString}!", IPA.Logging.Logger.Level.Error);
                    pointData = null;
                }
            }
            else
            {
                pointData = PointDefinition.DynamicToPointData(pointString);
                if (pointData != null)
                {
                    ((IDictionary<string, object>)customData)[pointName] = pointData;
                }
            }
        }

        public static Track GetTrack(dynamic customData)
        {
            string trackName = Trees.at(customData, TRACK);
            if (trackName == null)
            {
                return null;
            }

            if (Tracks.TryGetValue(trackName, out Track track))
            {
                return track;
            }
            else
            {
                NoodleLogger.Log($"Could not find track {trackName}!", IPA.Logging.Logger.Level.Error);
                return null;
            }
        }

        internal static void AddTrackProperties(Track track)
        {
            IDictionary<string, Property> properties = track.Properties;
            properties.Add(POSITION, new Property(PropertyType.Vector3));
            properties.Add(ROTATION, new Property(PropertyType.Quaternion));
            properties.Add(SCALE, new Property(PropertyType.Vector3));
            properties.Add(LOCALROTATION, new Property(PropertyType.Quaternion));
            properties.Add(DISSOLVE, new Property(PropertyType.Linear));
            properties.Add(DISSOLVEARROW, new Property(PropertyType.Linear));

            IDictionary<string, Property> pathProperties = track.PathProperties;
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
            TryGetPointData(customData, DEFINITEPOSITION, out PointDefinition localDefinitePosition);

            Vector3? pathDefinitePosition = localDefinitePosition?.Interpolate(time) ?? TryGetPathProperty(track, DEFINITEPOSITION, time);

            if (pathDefinitePosition.HasValue)
            {
                definitePosition = SumVectorNullables(TryGetProperty(track, POSITION), pathDefinitePosition) * NoteLinesDistance;
            }
            else
            {
                definitePosition = null;
            }
        }

        internal static void GetObjectOffset(dynamic customData, Track track, float time, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? dissolveArrow)
        {
            GetAllPointData(customData, out PointDefinition localPosition, out PointDefinition localRotation, out PointDefinition localScale, out PointDefinition localLocalRotation, out PointDefinition localDissolve, out PointDefinition localDissolveArrow);

            Vector3? pathPosition = localPosition?.Interpolate(time) ?? TryGetPathProperty(track, POSITION, time);
            Quaternion? pathRotation = localRotation?.InterpolateQuaternion(time) ?? TryGetPathProperty(track, ROTATION, time);
            Vector3? pathScale = localScale?.Interpolate(time) ?? TryGetPathProperty(track, SCALE, time);
            Quaternion? pathLocalRotation = localLocalRotation?.InterpolateQuaternion(time) ?? TryGetPathProperty(track, LOCALROTATION, time);
            float? pathDissolve = localDissolve?.InterpolateLinear(time) ?? TryGetPathProperty(track, DISSOLVE, time);
            float? pathDissolveArrow = localDissolveArrow?.InterpolateLinear(time) ?? TryGetPathProperty(track, DISSOLVEARROW, time);

            positionOffset = SumVectorNullables(TryGetProperty(track, POSITION), pathPosition) * NoteLinesDistance;
            rotationOffset = MultQuaternionNullables(TryGetProperty(track, ROTATION), pathRotation);
            scaleOffset = MultVectorNullables(TryGetProperty(track, SCALE), pathScale);
            localRotationOffset = MultQuaternionNullables(TryGetProperty(track, LOCALROTATION), pathLocalRotation);
            dissolve = MultFloatNullables(TryGetProperty(track, DISSOLVE), pathDissolve);
            dissolveArrow = MultFloatNullables(TryGetProperty(track, DISSOLVEARROW), pathDissolveArrow);
        }

        internal static void GetAllPointData(dynamic customData, out PointDefinition position, out PointDefinition rotation, out PointDefinition scale, out PointDefinition localRotation, out PointDefinition dissolve, out PointDefinition dissolveArrow)
        {
            Dictionary<string, PointDefinition> pointDefinitions = PointDefinitions;

            TryGetPointData(customData, POSITION, out position, pointDefinitions);
            TryGetPointData(customData, ROTATION, out rotation, pointDefinitions);
            TryGetPointData(customData, SCALE, out scale, pointDefinitions);
            TryGetPointData(customData, LOCALROTATION, out localRotation, pointDefinitions);
            TryGetPointData(customData, DISSOLVE, out dissolve, pointDefinitions);
            TryGetPointData(customData, DISSOLVEARROW, out dissolveArrow, pointDefinitions);
        }
    }
}
