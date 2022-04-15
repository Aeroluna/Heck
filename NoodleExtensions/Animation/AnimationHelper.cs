using System.Collections.Generic;
using System.Linq;
using Heck;
using Heck.Animation;
using JetBrains.Annotations;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using UnityEngine;
using Zenject;
using static Heck.NullableExtensions;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    [UsedImplicitly]
    internal class AnimationHelper
    {
        private readonly BeatmapObjectSpawnMovementData _movementData;
        private readonly bool _leftHanded;

        private AnimationHelper([Inject(Id = HeckController.LEFT_HANDED)] bool leftHanded, InitializedSpawnMovementData movementData)
        {
            _leftHanded = leftHanded;
            _movementData = movementData.MovementData;
        }

        internal static void OnTrackCreated(Track track)
        {
            track.AddProperty(POSITION, PropertyType.Vector3);
            track.AddProperty(ROTATION, PropertyType.Quaternion);
            track.AddProperty(SCALE, PropertyType.Vector3);
            track.AddProperty(LOCAL_ROTATION, PropertyType.Quaternion);
            track.AddProperty(DISSOLVE, PropertyType.Linear);
            track.AddProperty(DISSOLVE_ARROW, PropertyType.Linear);
            track.AddProperty(TIME, PropertyType.Linear);
            track.AddProperty(CUTTABLE, PropertyType.Linear);

            track.AddPathProperty(POSITION, PropertyType.Vector3);
            track.AddPathProperty(ROTATION, PropertyType.Quaternion);
            track.AddPathProperty(SCALE, PropertyType.Vector3);
            track.AddPathProperty(LOCAL_ROTATION, PropertyType.Quaternion);
            track.AddPathProperty(DEFINITE_POSITION, PropertyType.Vector3);
            track.AddPathProperty(DISSOLVE, PropertyType.Linear);
            track.AddPathProperty(DISSOLVE_ARROW, PropertyType.Linear);
            track.AddPathProperty(CUTTABLE, PropertyType.Linear);
        }

        internal void GetDefinitePositionOffset(
            NoodleObjectData.AnimationObjectData? animationObject,
            List<Track>? tracks,
            float time,
            out Vector3? definitePosition)
        {
            Vector3? pathDefinitePosition = animationObject?.LocalDefinitePosition?.Interpolate(time);

            if (!pathDefinitePosition.HasValue && tracks != null)
            {
                pathDefinitePosition = tracks.Count > 1
                    ? SumVectorNullables(tracks.Select(n => n.GetVector3PathProperty(DEFINITE_POSITION, time)))
                    : tracks.First().GetVector3PathProperty(DEFINITE_POSITION, time);
            }

            if (pathDefinitePosition.HasValue)
            {
                Vector3? pathPosition = animationObject?.LocalPosition?.Interpolate(time);
                Vector3? positionOffset = null;
                if (tracks != null)
                {
                    if (tracks.Count > 1)
                    {
                        pathPosition ??= SumVectorNullables(tracks.Select(n => n.GetVector3PathProperty(POSITION, time)));
                        positionOffset = SumVectorNullables(SumVectorNullables(tracks.Select(n => n.GetProperty<Vector3?>(POSITION))), pathPosition);
                    }
                    else
                    {
                        Track track = tracks.First();
                        pathPosition ??= track.GetVector3PathProperty(POSITION, time);
                        positionOffset = SumVectorNullables(track.GetProperty<Vector3?>(POSITION), pathPosition);
                    }
                }
                else
                {
                    positionOffset = pathPosition;
                }

                definitePosition = SumVectorNullables(positionOffset, pathDefinitePosition) * _movementData.noteLinesDistance;

                if (_leftHanded)
                {
                    MirrorVectorNullable(ref definitePosition);
                }
            }
            else
            {
                definitePosition = null;
            }
        }

        internal void GetObjectOffset(
            NoodleObjectData.AnimationObjectData? animationObject,
            List<Track>? tracks,
            float time,
            out Vector3? positionOffset,
            out Quaternion? rotationOffset,
            out Vector3? scaleOffset,
            out Quaternion? localRotationOffset,
            out float? dissolve,
            out float? dissolveArrow,
            out float? cuttable)
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
                Vector3? trackPosition;
                Quaternion? trackRotation;
                Vector3? trackScale;
                Quaternion? trackLocalRotation;
                float? trackDissolve;
                float? trackDissolveArrow;
                float? trackCuttable;

                if (tracks.Count > 1)
                {
                    pathPosition ??= SumVectorNullables(tracks.Select(n => n.GetVector3PathProperty(POSITION, time)));
                    pathRotation ??= MultQuaternionNullables(tracks.Select(n => n.GetQuaternionPathProperty(ROTATION, time)));
                    pathScale ??= MultVectorNullables(tracks.Select(n => n.GetVector3PathProperty(SCALE, time)));
                    pathLocalRotation ??= MultQuaternionNullables(tracks.Select(n => n.GetQuaternionPathProperty(LOCAL_ROTATION, time)));
                    pathDissolve ??= MultFloatNullables(tracks.Select(n => n.GetLinearPathProperty(DISSOLVE, time)));
                    pathDissolveArrow ??= MultFloatNullables(tracks.Select(n => n.GetLinearPathProperty(DISSOLVE_ARROW, time)));
                    pathCuttable ??= MultFloatNullables(tracks.Select(n => n.GetLinearPathProperty(CUTTABLE, time)));

                    trackPosition = SumVectorNullables(tracks.Select(n => n.GetProperty<Vector3?>(POSITION)));
                    trackRotation = MultQuaternionNullables(tracks.Select(n => n.GetProperty<Quaternion?>(ROTATION)));
                    trackScale = MultVectorNullables(tracks.Select(n => n.GetProperty<Vector3?>(SCALE)));
                    trackLocalRotation = MultQuaternionNullables(tracks.Select(n => n.GetProperty<Quaternion?>(LOCAL_ROTATION)));
                    trackDissolve = MultFloatNullables(tracks.Select(n => n.GetProperty<float?>(DISSOLVE)));
                    trackDissolveArrow = MultFloatNullables(tracks.Select(n => n.GetProperty<float?>(DISSOLVE_ARROW)));
                    trackCuttable = MultFloatNullables(tracks.Select(n => n.GetProperty<float?>(CUTTABLE)));
                }
                else
                {
                    Track track = tracks.First();
                    pathPosition ??= track.GetVector3PathProperty(POSITION, time);
                    pathRotation ??= track.GetQuaternionPathProperty(ROTATION, time);
                    pathScale ??= track.GetVector3PathProperty(SCALE, time);
                    pathLocalRotation ??= track.GetQuaternionPathProperty(LOCAL_ROTATION, time);
                    pathDissolve ??= track.GetLinearPathProperty(DISSOLVE, time);
                    pathDissolveArrow ??= track.GetLinearPathProperty(DISSOLVE_ARROW, time);
                    pathCuttable ??= track.GetLinearPathProperty(CUTTABLE, time);

                    trackPosition = track.GetProperty<Vector3?>(POSITION);
                    trackRotation = track.GetProperty<Quaternion?>(ROTATION);
                    trackScale = track.GetProperty<Vector3?>(SCALE);
                    trackLocalRotation = track.GetProperty<Quaternion?>(LOCAL_ROTATION);
                    trackDissolve = track.GetProperty<float?>(DISSOLVE);
                    trackDissolveArrow = track.GetProperty<float?>(DISSOLVE_ARROW);
                    trackCuttable = track.GetProperty<float?>(CUTTABLE);
                }

                positionOffset = SumVectorNullables(trackPosition, pathPosition) * _movementData.noteLinesDistance;
                rotationOffset = MultQuaternionNullables(trackRotation, pathRotation);
                scaleOffset = MultVectorNullables(trackScale, pathScale);
                localRotationOffset = MultQuaternionNullables(trackLocalRotation, pathLocalRotation);
                dissolve = MultFloatNullables(trackDissolve, pathDissolve);
                dissolveArrow = MultFloatNullables(trackDissolveArrow, pathDissolveArrow);
                cuttable = MultFloatNullables(trackCuttable, pathCuttable);
            }
            else
            {
                positionOffset = pathPosition * _movementData.noteLinesDistance;
                rotationOffset = pathRotation;
                scaleOffset = pathScale;
                localRotationOffset = pathLocalRotation;
                dissolve = pathDissolve;
                dissolveArrow = pathDissolveArrow;
                cuttable = pathCuttable;
            }

            if (!_leftHanded)
            {
                return;
            }

            MirrorVectorNullable(ref positionOffset);
            MirrorQuaternionNullable(ref rotationOffset);
            MirrorQuaternionNullable(ref localRotationOffset);
        }
    }
}
