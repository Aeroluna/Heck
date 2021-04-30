namespace NoodleExtensions.Animation
{
    using System.Collections.Generic;
    using Heck.Animation;
    using IPA.Utilities;
    using UnityEngine;
    using static Heck.Animation.AnimationHelper;
    using static Heck.NullableExtensions;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
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

        private static BasicBeatmapObjectManager BeatmapObjectManager => HarmonyPatches.BeatmapObjectSpawnControllerStart.BeatmapObjectManager;

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

                if (LeftHandedMode)
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

            if (LeftHandedMode)
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
    }
}
