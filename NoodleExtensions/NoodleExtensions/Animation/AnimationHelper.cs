namespace NoodleExtensions.Animation
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.NullableExtensions;
    using static NoodleExtensions.Plugin;

    public static class AnimationHelper
    {
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _noteAPoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_noteAPool");
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _noteBPoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_noteBPool");
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _bombNotePoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_bombNotePool");
        private static readonly FieldAccessor<BeatmapObjectManager, ObstacleController.Pool>.Accessor _obstaclePoolAccessor = FieldAccessor<BeatmapObjectManager, ObstacleController.Pool>.GetAccessor("_obstaclePool");

        private static BeatmapObjectManager _beatmapObjectManager;

        public static NoteController.Pool NoteAPool
        {
            get
            {
                BeatmapObjectManager beatmapObjectManager = BeatmapObjectManager;
                return _noteAPoolAccessor(ref beatmapObjectManager);
            }
        }

        public static NoteController.Pool NoteBPool
        {
            get
            {
                BeatmapObjectManager beatmapObjectManager = BeatmapObjectManager;
                return _noteBPoolAccessor(ref beatmapObjectManager);
            }
        }

        public static NoteController.Pool BombNotePool
        {
            get
            {
                BeatmapObjectManager beatmapObjectManager = BeatmapObjectManager;
                return _bombNotePoolAccessor(ref beatmapObjectManager);
            }
        }

        public static ObstacleController.Pool ObstaclePool
        {
            get
            {
                BeatmapObjectManager beatmapObjectManager = BeatmapObjectManager;
                return _obstaclePoolAccessor(ref beatmapObjectManager);
            }
        }

        public static Dictionary<string, Track> Tracks { get => ((CustomBeatmapData)AnimationController.Instance.CustomEventCallbackController._beatmapData).customData.tracks; }

        public static Dictionary<string, PointDefinition> PointDefinitions { get => Trees.at(((CustomBeatmapData)AnimationController.Instance.CustomEventCallbackController._beatmapData).customData, "pointDefinitions"); }

        private static BeatmapObjectManager BeatmapObjectManager
        {
            get
            {
                if (_beatmapObjectManager == null)
                {
                    _beatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().First();
                }

                return _beatmapObjectManager;
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

                case PropertyType.Vector4:
                    return pointDataInterpolation.InterpolateVector4(time);

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
            switch (pointString)
            {
                case null:
                    pointData = null;
                    break;
                case PointDefinition castedData:
                    pointData = castedData;
                    break;
                case string castedString:
                    if (!pointDefinitions.TryGetValue(castedString, out pointData))
                    {
                        NoodleLogger.Log($"Could not find point definition {castedString}!", IPA.Logging.Logger.Level.Error);
                        pointData = null;
                    }

                    break;
                default:
                    pointData = PointDefinition.DynamicToPointData(pointString);
                    if (pointData != null)
                    {
                        ((IDictionary<string, object>)customData)[pointName] = pointData;
                    }

                    break;
            }
        }

        public static Track GetTrack(dynamic customData, string name = TRACK)
        {
            string trackName = Trees.at(customData, name);
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

        public static IEnumerable<Track> GetTrackArray(dynamic customData, string name = TRACK)
        {
            IEnumerable<string> trackNames = ((List<object>)Trees.at(customData, name)).Cast<string>();
            if (trackNames == null)
            {
                return null;
            }

            HashSet<Track> tracks = new HashSet<Track>();
            foreach (string trackName in trackNames)
            {
                if (Tracks.TryGetValue(trackName, out Track track))
                {
                    tracks.Add(track);
                }
                else
                {
                    NoodleLogger.Log($"Could not find track {trackName}!", IPA.Logging.Logger.Level.Error);
                }
            }

            return tracks;
        }

        // NE Specific properties below
        internal static void OnTrackCreated(Track track)
        {
            IDictionary<string, Property> properties = track.Properties;
            properties.Add(POSITION, new Property(PropertyType.Vector3));
            properties.Add(ROTATION, new Property(PropertyType.Quaternion));
            properties.Add(SCALE, new Property(PropertyType.Vector3));
            properties.Add(LOCALROTATION, new Property(PropertyType.Quaternion));
            properties.Add(DISSOLVE, new Property(PropertyType.Linear));
            properties.Add(DISSOLVEARROW, new Property(PropertyType.Linear));
            properties.Add(TIME, new Property(PropertyType.Linear));
            properties.Add(CUTTABLE, new Property(PropertyType.Linear));

            IDictionary<string, Property> pathProperties = track.PathProperties;
            pathProperties.Add(POSITION, new Property(PropertyType.Vector3));
            pathProperties.Add(ROTATION, new Property(PropertyType.Quaternion));
            pathProperties.Add(SCALE, new Property(PropertyType.Vector3));
            pathProperties.Add(LOCALROTATION, new Property(PropertyType.Quaternion));
            pathProperties.Add(DEFINITEPOSITION, new Property(PropertyType.Vector3));
            pathProperties.Add(DISSOLVE, new Property(PropertyType.Linear));
            pathProperties.Add(DISSOLVEARROW, new Property(PropertyType.Linear));
            pathProperties.Add(CUTTABLE, new Property(PropertyType.Linear));
        }

        internal static void GetDefinitePositionOffset(dynamic customData, Track track, float time, out Vector3? definitePosition)
        {
            TryGetPointData(customData, DEFINITEPOSITION, out PointDefinition localDefinitePosition);

            Vector3? pathDefinitePosition = localDefinitePosition?.Interpolate(time) ?? TryGetPathProperty(track, DEFINITEPOSITION, time);

            if (pathDefinitePosition.HasValue)
            {
                TryGetPointData(customData, POSITION, out PointDefinition localPosition, PointDefinitions);
                Vector3? pathPosition = localPosition?.Interpolate(time) ?? TryGetPathProperty(track, POSITION, time);
                Vector3? positionOffset = SumVectorNullables(TryGetProperty(track, POSITION), pathPosition);
                definitePosition = SumVectorNullables(positionOffset, pathDefinitePosition) * NoteLinesDistance;

                if (NoodleController.LeftHandedMode)
                {
                    MirrorVectorNullable(ref definitePosition);
                }
            }
            else
            {
                definitePosition = null;
            }
        }

        internal static void GetObjectOffset(dynamic customData, Track track, float time, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? dissolveArrow, out float? cuttable)
        {
            GetAllPointData(customData, out PointDefinition localPosition, out PointDefinition localRotation, out PointDefinition localScale, out PointDefinition localLocalRotation, out PointDefinition localDissolve, out PointDefinition localDissolveArrow, out PointDefinition localCuttable);

            Vector3? pathPosition = localPosition?.Interpolate(time) ?? TryGetPathProperty(track, POSITION, time);
            Quaternion? pathRotation = localRotation?.InterpolateQuaternion(time) ?? TryGetPathProperty(track, ROTATION, time);
            Vector3? pathScale = localScale?.Interpolate(time) ?? TryGetPathProperty(track, SCALE, time);
            Quaternion? pathLocalRotation = localLocalRotation?.InterpolateQuaternion(time) ?? TryGetPathProperty(track, LOCALROTATION, time);
            float? pathDissolve = localDissolve?.InterpolateLinear(time) ?? TryGetPathProperty(track, DISSOLVE, time);
            float? pathDissolveArrow = localDissolveArrow?.InterpolateLinear(time) ?? TryGetPathProperty(track, DISSOLVEARROW, time);
            float? pathCuttable = localCuttable?.InterpolateLinear(time) ?? TryGetPathProperty(track, CUTTABLE, time);

            positionOffset = SumVectorNullables(TryGetProperty(track, POSITION), pathPosition) * NoteLinesDistance;
            rotationOffset = MultQuaternionNullables(TryGetProperty(track, ROTATION), pathRotation);
            scaleOffset = MultVectorNullables(TryGetProperty(track, SCALE), pathScale);
            localRotationOffset = MultQuaternionNullables(TryGetProperty(track, LOCALROTATION), pathLocalRotation);
            dissolve = MultFloatNullables(TryGetProperty(track, DISSOLVE), pathDissolve);
            dissolveArrow = MultFloatNullables(TryGetProperty(track, DISSOLVEARROW), pathDissolveArrow);
            cuttable = MultFloatNullables(TryGetProperty(track, CUTTABLE), pathCuttable);

            if (NoodleController.LeftHandedMode)
            {
                MirrorVectorNullable(ref positionOffset);
                MirrorQuaternionNullable(ref rotationOffset);
                MirrorQuaternionNullable(ref localRotationOffset);
            }
        }

        internal static void GetAllPointData(dynamic customData, out PointDefinition position, out PointDefinition rotation, out PointDefinition scale, out PointDefinition localRotation, out PointDefinition dissolve, out PointDefinition dissolveArrow, out PointDefinition cuttable)
        {
            Dictionary<string, PointDefinition> pointDefinitions = PointDefinitions;

            TryGetPointData(customData, POSITION, out position, pointDefinitions);
            TryGetPointData(customData, ROTATION, out rotation, pointDefinitions);
            TryGetPointData(customData, SCALE, out scale, pointDefinitions);
            TryGetPointData(customData, LOCALROTATION, out localRotation, pointDefinitions);
            TryGetPointData(customData, DISSOLVE, out dissolve, pointDefinitions);
            TryGetPointData(customData, DISSOLVEARROW, out dissolveArrow, pointDefinitions);
            TryGetPointData(customData, CUTTABLE, out cuttable, pointDefinitions);
        }
    }
}
