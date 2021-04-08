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
        private static readonly FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<GameNoteController>>.Accessor _gameNotePoolAccessor = FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<GameNoteController>>.GetAccessor("_gameNotePoolContainer");
        private static readonly FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<BombNoteController>>.Accessor _bombNotePoolAccessor = FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<BombNoteController>>.GetAccessor("_bombNotePoolContainer");
        private static readonly FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<ObstacleController>>.Accessor _obstaclePoolAccessor = FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<ObstacleController>>.GetAccessor("_obstaclePoolContainer");
        private static readonly FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectSpawner>.Accessor _beatmapObjectSpawnAccessor = FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectSpawner>.GetAccessor("_beatmapObjectSpawner");

        public static MemoryPoolContainer<GameNoteController> GameNotePool
        {
            get
            {
                BasicBeatmapObjectManager beatmapObjectManager = BeatmapObjectManager;
                return _gameNotePoolAccessor(ref beatmapObjectManager);
            }
        }

        public static MemoryPoolContainer<BombNoteController> BombNotePool
        {
            get
            {
                BasicBeatmapObjectManager beatmapObjectManager = BeatmapObjectManager;
                return _bombNotePoolAccessor(ref beatmapObjectManager);
            }
        }

        public static MemoryPoolContainer<ObstacleController> ObstaclePool
        {
            get
            {
                BasicBeatmapObjectManager beatmapObjectManager = BeatmapObjectManager;
                return _obstaclePoolAccessor(ref beatmapObjectManager);
            }
        }

        ////public static Dictionary<string, Track> Tracks => ((CustomBeatmapData)AnimationController.Instance.CustomEventCallbackController._beatmapData).customData.tracks;

        ////public static Dictionary<string, PointDefinition> PointDefinitions => Trees.at(((CustomBeatmapData)AnimationController.Instance.CustomEventCallbackController._beatmapData).customData, "pointDefinitions");

        private static BasicBeatmapObjectManager BeatmapObjectManager => HarmonyPatches.BeatmapObjectSpawnControllerStart.BeatmapObjectManager;

        /*public static dynamic TryGetPathProperty(Track track, string propertyName, float time)
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
        }*/

        public static float? TryGetLinearPathProperty(Track track, string propertyName, float time)
        {
            PointDefinitionInterpolation pointDataInterpolation = GetPathInterpolation(track, propertyName, PropertyType.Linear);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateLinear(time);
            }

            return null;
        }

        public static Quaternion? TryGetQuaternionPathProperty(Track track, string propertyName, float time)
        {
            PointDefinitionInterpolation pointDataInterpolation = GetPathInterpolation(track, propertyName, PropertyType.Quaternion);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateQuaternion(time);
            }

            return null;
        }

        public static Vector3? TryGetVector3PathProperty(Track track, string propertyName, float time)
        {
            PointDefinitionInterpolation pointDataInterpolation = GetPathInterpolation(track, propertyName, PropertyType.Vector3);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.Interpolate(time);
            }

            return null;
        }

        public static Vector4? TryGetVector4PathProperty(Track track, string propertyName, float time)
        {
            PointDefinitionInterpolation pointDataInterpolation = GetPathInterpolation(track, propertyName, PropertyType.Vector4);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateVector4(time);
            }

            return null;
        }

        /*public static dynamic TryGetProperty(Track track, string propertyName)
        {
            Property property = null;
            track?.Properties.TryGetValue(propertyName, out property);
            return property?.Value;
        }*/

        public static object TryGetPropertyAsObject(Track track, string propertyName)
        {
            Property property = null;
            track?.Properties.TryGetValue(propertyName, out property);
            return property?.Value;
        }

        public static void TryGetPointData(dynamic customData, string pointName, out PointDefinition pointData, Dictionary<string, PointDefinition> pointDefinitions)
        {
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

        /*public static Track GetTrack(dynamic customData, string name = TRACK)
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
        }*/

        public static Track GetTrackPreload(dynamic customData, IReadonlyBeatmapData beatmapData, string name = TRACK)
        {
            string trackName = Trees.at(customData, name);
            if (trackName == null)
            {
                return null;
            }

            if (((CustomBeatmapData)beatmapData).customData.tracks.TryGetValue(trackName, out Track track))
            {
                return track;
            }
            else
            {
                NoodleLogger.Log($"Could not find track {trackName}!", IPA.Logging.Logger.Level.Error);
                return null;
            }
        }

        /*public static IEnumerable<Track> GetTrackArray(dynamic customData, string name = TRACK)
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
        }*/

        public static IEnumerable<Track> GetTrackArrayPreload(dynamic customData, IReadonlyBeatmapData beatmapData, string name = TRACK)
        {
            IEnumerable<string> trackNames = ((List<object>)Trees.at(customData, name)).Cast<string>();
            if (trackNames == null)
            {
                return null;
            }

            HashSet<Track> tracks = new HashSet<Track>();
            foreach (string trackName in trackNames)
            {
                if (((CustomBeatmapData)beatmapData).customData.tracks.TryGetValue(trackName, out Track track))
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

        internal static void GetDefinitePositionOffset(NoodleObjectData.AnimationObjectData animationObject, Track track, float time, out Vector3? definitePosition)
        {
            PointDefinition localDefinitePosition = animationObject.LocalDefinitePosition;

            Vector3? pathDefinitePosition = localDefinitePosition?.Interpolate(time) ?? TryGetVector3PathProperty(track, DEFINITEPOSITION, time);

            if (pathDefinitePosition.HasValue)
            {
                Vector3? pathPosition = animationObject.LocalPosition?.Interpolate(time) ?? TryGetVector3PathProperty(track, POSITION, time);
                Vector3? positionOffset = SumVectorNullables((Vector3?)TryGetPropertyAsObject(track, POSITION), pathPosition);
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

        internal static void GetObjectOffset(NoodleObjectData.AnimationObjectData animationObject, Track track, float time, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? dissolveArrow, out float? cuttable)
        {
            Vector3? pathPosition = animationObject.LocalPosition?.Interpolate(time) ?? TryGetVector3PathProperty(track, POSITION, time);
            Quaternion? pathRotation = animationObject.LocalRotation?.InterpolateQuaternion(time) ?? TryGetQuaternionPathProperty(track, ROTATION, time);
            Vector3? pathScale = animationObject.LocalScale?.Interpolate(time) ?? TryGetVector3PathProperty(track, SCALE, time);
            Quaternion? pathLocalRotation = animationObject.LocalLocalRotation?.InterpolateQuaternion(time) ?? TryGetQuaternionPathProperty(track, LOCALROTATION, time);
            float? pathDissolve = animationObject.LocalDissolve?.InterpolateLinear(time) ?? TryGetLinearPathProperty(track, DISSOLVE, time);
            float? pathDissolveArrow = animationObject.LocalDissolveArrow?.InterpolateLinear(time) ?? TryGetLinearPathProperty(track, DISSOLVEARROW, time);
            float? pathCuttable = animationObject.LocalCuttable?.InterpolateLinear(time) ?? TryGetLinearPathProperty(track, CUTTABLE, time);

            positionOffset = SumVectorNullables((Vector3?)TryGetPropertyAsObject(track, POSITION), pathPosition) * NoteLinesDistance;
            rotationOffset = MultQuaternionNullables((Quaternion?)TryGetPropertyAsObject(track, ROTATION), pathRotation);
            scaleOffset = MultVectorNullables((Vector3?)TryGetPropertyAsObject(track, SCALE), pathScale);
            localRotationOffset = MultQuaternionNullables((Quaternion?)TryGetPropertyAsObject(track, LOCALROTATION), pathLocalRotation);
            dissolve = MultFloatNullables((float?)TryGetPropertyAsObject(track, DISSOLVE), pathDissolve);
            dissolveArrow = MultFloatNullables((float?)TryGetPropertyAsObject(track, DISSOLVEARROW), pathDissolveArrow);
            cuttable = MultFloatNullables((float?)TryGetPropertyAsObject(track, CUTTABLE), pathCuttable);

            if (NoodleController.LeftHandedMode)
            {
                MirrorVectorNullable(ref positionOffset);
                MirrorQuaternionNullable(ref rotationOffset);
                MirrorQuaternionNullable(ref localRotationOffset);
            }
        }

        internal static void GetAllPointData(dynamic customData, Dictionary<string, PointDefinition> pointDefinitions, out PointDefinition position, out PointDefinition rotation, out PointDefinition scale, out PointDefinition localRotation, out PointDefinition dissolve, out PointDefinition dissolveArrow, out PointDefinition cuttable, out PointDefinition definitePosition)
        {
            TryGetPointData(customData, POSITION, out position, pointDefinitions);
            TryGetPointData(customData, ROTATION, out rotation, pointDefinitions);
            TryGetPointData(customData, SCALE, out scale, pointDefinitions);
            TryGetPointData(customData, LOCALROTATION, out localRotation, pointDefinitions);
            TryGetPointData(customData, DISSOLVE, out dissolve, pointDefinitions);
            TryGetPointData(customData, DISSOLVEARROW, out dissolveArrow, pointDefinitions);
            TryGetPointData(customData, CUTTABLE, out cuttable, pointDefinitions);
            TryGetPointData(customData, DEFINITEPOSITION, out definitePosition, pointDefinitions);
        }

        // End of NE specific
        private static PointDefinitionInterpolation GetPathInterpolation(Track track, string propertyName, PropertyType propertyType)
        {
            Property pathProperty = null;
            track?.PathProperties.TryGetValue(propertyName, out pathProperty);
            if (pathProperty != null)
            {
                PointDefinitionInterpolation pointDataInterpolation = (PointDefinitionInterpolation)pathProperty.Value;

                return pointDataInterpolation;
            }

            return null;
        }
    }
}
