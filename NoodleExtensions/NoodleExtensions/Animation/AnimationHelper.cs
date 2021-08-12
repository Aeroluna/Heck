namespace NoodleExtensions.Animation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public static MemoryPoolContainer<GameNoteController> GameNotePool
        {
            get
            {
                BasicBeatmapObjectManager beatmapObjectManager = BeatmapObjectManager ?? throw new InvalidOperationException("BeatmapObjectManager was null.");
                return _gameNotePoolAccessor(ref beatmapObjectManager);
            }
        }

        public static MemoryPoolContainer<BombNoteController> BombNotePool
        {
            get
            {
                BasicBeatmapObjectManager beatmapObjectManager = BeatmapObjectManager ?? throw new InvalidOperationException("BeatmapObjectManager was null.");
                return _bombNotePoolAccessor(ref beatmapObjectManager);
            }
        }

        public static MemoryPoolContainer<ObstacleController> ObstaclePool
        {
            get
            {
                BasicBeatmapObjectManager beatmapObjectManager = BeatmapObjectManager ?? throw new InvalidOperationException("BeatmapObjectManager was null.");
                return _obstaclePoolAccessor(ref beatmapObjectManager);
            }
        }

        private static BasicBeatmapObjectManager? BeatmapObjectManager => HarmonyPatches.BeatmapObjectSpawnControllerStart.BeatmapObjectManager;

        internal static void OnTrackCreated(Track track)
        {
            track.AddProperty(POSITION, PropertyType.Vector3);
            track.AddProperty(ROTATION, PropertyType.Quaternion);
            track.AddProperty(SCALE, PropertyType.Vector3);
            track.AddProperty(LOCALROTATION, PropertyType.Quaternion);
            track.AddProperty(DISSOLVE, PropertyType.Linear);
            track.AddProperty(DISSOLVEARROW, PropertyType.Linear);
            track.AddProperty(TIME, PropertyType.Linear);
            track.AddProperty(CUTTABLE, PropertyType.Linear);

            track.AddPathProperty(POSITION, PropertyType.Vector3);
            track.AddPathProperty(ROTATION, PropertyType.Quaternion);
            track.AddPathProperty(SCALE, PropertyType.Vector3);
            track.AddPathProperty(LOCALROTATION, PropertyType.Quaternion);
            track.AddPathProperty(DEFINITEPOSITION, PropertyType.Vector3);
            track.AddPathProperty(DISSOLVE, PropertyType.Linear);
            track.AddPathProperty(DISSOLVEARROW, PropertyType.Linear);
            track.AddPathProperty(CUTTABLE, PropertyType.Linear);
        }

        internal static void GetDefinitePositionOffset(NoodleObjectData.AnimationObjectData? animationObject, IEnumerable<Track>? tracks, float time, out Vector3? definitePosition)
        {
            Vector3? pathDefinitePosition = animationObject?.LocalDefinitePosition?.Interpolate(time);

            if (pathDefinitePosition == null && tracks != null)
            {
                pathDefinitePosition = SumVectorNullables(tracks.Select(n => TryGetVector3PathProperty(n, DEFINITEPOSITION, time)).ToArray());
            }

            if (pathDefinitePosition.HasValue)
            {
                Vector3? pathPosition = animationObject?.LocalPosition?.Interpolate(time);
                Vector3? positionOffset = null;
                if (tracks != null)
                {
                    pathPosition ??= SumVectorNullables(tracks.Select(n => TryGetVector3PathProperty(n, POSITION, time)).ToArray());
                    positionOffset = SumVectorNullables(SumVectorNullables(tracks.Select(n => TryGetProperty<Vector3?>(n, POSITION)).ToArray()), pathPosition) * NoteLinesDistance;
                }
                else
                {
                    positionOffset = pathPosition;
                }

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

        internal static void GetObjectOffset(NoodleObjectData.AnimationObjectData? animationObject, IEnumerable<Track>? tracks, float time, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? dissolveArrow, out float? cuttable)
        {
            /*
             * position = SumVectorNullables
             * rotation = MultQuaternionNullables
             * scale = MultVectorNullables
             * localRotation = MultQuaternionNullables
             * dissolve = MultFloatNullables
             * dissolveArrow = MultFloatNullables
             * cuttable = MultFloatNullables
             */
            Vector3? pathPosition = null;
            Quaternion? pathRotation = null;
            Vector3? pathScale = null;
            Quaternion? pathLocalRotation = null;
            float? pathDissolve = null;
            float? pathDissolveArrow = null;
            float? pathCuttable = null;

            if (animationObject != null)
            {
                pathPosition = animationObject.LocalPosition?.Interpolate(time);
                pathRotation = animationObject.LocalRotation?.InterpolateQuaternion(time);
                pathScale = animationObject.LocalScale?.Interpolate(time);
                pathLocalRotation = animationObject.LocalLocalRotation?.InterpolateQuaternion(time);
                pathDissolve = animationObject.LocalDissolve?.InterpolateLinear(time);
                pathDissolveArrow = animationObject.LocalDissolveArrow?.InterpolateLinear(time);
                pathCuttable = animationObject.LocalCuttable?.InterpolateLinear(time);
            }

            if (tracks != null)
            {
                pathPosition ??= SumVectorNullables(tracks.Select(n => TryGetVector3PathProperty(n, POSITION, time)).ToArray());
                pathRotation ??= MultQuaternionNullables(tracks.Select(n => TryGetQuaternionPathProperty(n, ROTATION, time)).ToArray());
                pathScale ??= MultVectorNullables(tracks.Select(n => TryGetVector3PathProperty(n, SCALE, time)).ToArray());
                pathLocalRotation ??= MultQuaternionNullables(tracks.Select(n => TryGetQuaternionPathProperty(n, LOCALROTATION, time)).ToArray());
                pathDissolve ??= MultFloatNullables(tracks.Select(n => TryGetLinearPathProperty(n, DISSOLVE, time)).ToArray());
                pathDissolveArrow ??= MultFloatNullables(tracks.Select(n => TryGetLinearPathProperty(n, DISSOLVEARROW, time)).ToArray());
                pathCuttable ??= MultFloatNullables(tracks.Select(n => TryGetLinearPathProperty(n, CUTTABLE, time)).ToArray());

                Vector3? trackPosition = SumVectorNullables(tracks.Select(n => TryGetProperty<Vector3?>(n, POSITION)).ToArray());
                Quaternion? trackRotation = MultQuaternionNullables(tracks.Select(n => TryGetProperty<Quaternion?>(n, ROTATION)).ToArray());
                Vector3? trackScale = MultVectorNullables(tracks.Select(n => TryGetProperty<Vector3?>(n, SCALE)).ToArray());
                Quaternion? trackLocalRotation = MultQuaternionNullables(tracks.Select(n => TryGetProperty<Quaternion?>(n, LOCALROTATION)).ToArray());
                float? trackDissolve = MultFloatNullables(tracks.Select(n => TryGetProperty<float?>(n, DISSOLVE)).ToArray());
                float? trackDissolveArrow = MultFloatNullables(tracks.Select(n => TryGetProperty<float?>(n, DISSOLVEARROW)).ToArray());
                float? trackCuttable = MultFloatNullables(tracks.Select(n => TryGetProperty<float?>(n, CUTTABLE)).ToArray());

                positionOffset = SumVectorNullables(trackPosition, pathPosition) * NoteLinesDistance;
                rotationOffset = MultQuaternionNullables(trackRotation, pathRotation);
                scaleOffset = MultVectorNullables(trackScale, pathScale);
                localRotationOffset = MultQuaternionNullables(trackLocalRotation, pathLocalRotation);
                dissolve = MultFloatNullables(trackDissolve, pathDissolve);
                dissolveArrow = MultFloatNullables(trackDissolveArrow, pathDissolveArrow);
                cuttable = MultFloatNullables(trackDissolve, pathCuttable);
            }
            else
            {
                positionOffset = pathPosition * NoteLinesDistance;
                rotationOffset = pathRotation;
                scaleOffset = pathScale;
                localRotationOffset = pathLocalRotation;
                dissolve = pathDissolve;
                dissolveArrow = pathDissolveArrow;
                cuttable = pathCuttable;
            }

            if (LeftHandedMode)
            {
                MirrorVectorNullable(ref positionOffset);
                MirrorQuaternionNullable(ref rotationOffset);
                MirrorQuaternionNullable(ref localRotationOffset);
            }
        }

        internal static void GetAllPointData(
            Dictionary<string, object?> customData,
            Dictionary<string, PointDefinition> pointDefinitions,
            out PointDefinition? position,
            out PointDefinition? rotation,
            out PointDefinition? scale,
            out PointDefinition? localRotation,
            out PointDefinition? dissolve,
            out PointDefinition? dissolveArrow,
            out PointDefinition? cuttable,
            out PointDefinition? definitePosition)
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
