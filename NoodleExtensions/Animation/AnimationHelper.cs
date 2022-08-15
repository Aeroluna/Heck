using System.Collections.Generic;
using System.Linq;
using Heck;
using Heck.Animation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.HeckController;
using static Heck.NullableExtensions;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    [UsedImplicitly]
    internal class AnimationHelper
    {
        private readonly bool _leftHanded;

        private AnimationHelper([Inject(Id = LEFT_HANDED_ID)] bool leftHanded)
        {
            _leftHanded = leftHanded;
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
                        pathPosition ??= SumVectorNullables(tracks.Select(n => n.GetVector3PathProperty(OFFSET_POSITION, time)));
                        positionOffset = SumVectorNullables(SumVectorNullables(tracks.Select(n => n.GetProperty<Vector3>(OFFSET_POSITION))), pathPosition);
                    }
                    else
                    {
                        Track track = tracks.First();
                        pathPosition ??= track.GetVector3PathProperty(OFFSET_POSITION, time);
                        positionOffset = SumVectorNullables(track.GetProperty<Vector3>(OFFSET_POSITION), pathPosition);
                    }
                }
                else
                {
                    positionOffset = pathPosition;
                }

                definitePosition = (SumVectorNullables(positionOffset, pathDefinitePosition) * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance)?.Mirror(_leftHanded);
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
                pathRotation = animationObject.LocalRotation?.Interpolate(time);
                pathScale = animationObject.LocalScale?.Interpolate(time);
                pathLocalRotation = animationObject.LocalLocalRotation?.Interpolate(time);
                pathDissolve = animationObject.LocalDissolve?.Interpolate(time);
                pathDissolveArrow = animationObject.LocalDissolveArrow?.Interpolate(time);
                pathCuttable = animationObject.LocalCuttable?.Interpolate(time);
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
                    pathPosition ??= SumVectorNullables(tracks.Select(n => n.GetVector3PathProperty(OFFSET_POSITION, time)));
                    pathRotation ??= MultQuaternionNullables(tracks.Select(n => n.GetQuaternionPathProperty(OFFSET_ROTATION, time)));
                    pathScale ??= MultVectorNullables(tracks.Select(n => n.GetVector3PathProperty(SCALE, time)));
                    pathLocalRotation ??= MultQuaternionNullables(tracks.Select(n => n.GetQuaternionPathProperty(LOCAL_ROTATION, time)));
                    pathDissolve ??= MultFloatNullables(tracks.Select(n => n.GetLinearPathProperty(DISSOLVE, time)));
                    pathDissolveArrow ??= MultFloatNullables(tracks.Select(n => n.GetLinearPathProperty(DISSOLVE_ARROW, time)));
                    pathCuttable ??= MultFloatNullables(tracks.Select(n => n.GetLinearPathProperty(INTERACTABLE, time)));

                    trackPosition = SumVectorNullables(tracks.Select(n => n.GetProperty<Vector3>(OFFSET_POSITION)));
                    trackRotation = MultQuaternionNullables(tracks.Select(n => n.GetProperty<Quaternion>(OFFSET_ROTATION)));
                    trackScale = MultVectorNullables(tracks.Select(n => n.GetProperty<Vector3>(SCALE)));
                    trackLocalRotation = MultQuaternionNullables(tracks.Select(n => n.GetProperty<Quaternion>(LOCAL_ROTATION)));
                    trackDissolve = MultFloatNullables(tracks.Select(n => n.GetProperty<float>(DISSOLVE)));
                    trackDissolveArrow = MultFloatNullables(tracks.Select(n => n.GetProperty<float>(DISSOLVE_ARROW)));
                    trackCuttable = MultFloatNullables(tracks.Select(n => n.GetProperty<float>(INTERACTABLE)));
                }
                else
                {
                    Track track = tracks.First();
                    pathPosition ??= track.GetVector3PathProperty(OFFSET_POSITION, time);
                    pathRotation ??= track.GetQuaternionPathProperty(OFFSET_ROTATION, time);
                    pathScale ??= track.GetVector3PathProperty(SCALE, time);
                    pathLocalRotation ??= track.GetQuaternionPathProperty(LOCAL_ROTATION, time);
                    pathDissolve ??= track.GetLinearPathProperty(DISSOLVE, time);
                    pathDissolveArrow ??= track.GetLinearPathProperty(DISSOLVE_ARROW, time);
                    pathCuttable ??= track.GetLinearPathProperty(V2_CUTTABLE, time);

                    trackPosition = track.GetProperty<Vector3>(OFFSET_POSITION);
                    trackRotation = track.GetProperty<Quaternion>(OFFSET_ROTATION);
                    trackScale = track.GetProperty<Vector3>(SCALE);
                    trackLocalRotation = track.GetProperty<Quaternion>(LOCAL_ROTATION);
                    trackDissolve = track.GetProperty<float>(DISSOLVE);
                    trackDissolveArrow = track.GetProperty<float>(DISSOLVE_ARROW);
                    trackCuttable = track.GetProperty<float>(INTERACTABLE);
                }

                positionOffset = SumVectorNullables(trackPosition, pathPosition) * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
                rotationOffset = MultQuaternionNullables(trackRotation, pathRotation);
                scaleOffset = MultVectorNullables(trackScale, pathScale);
                localRotationOffset = MultQuaternionNullables(trackLocalRotation, pathLocalRotation);
                dissolve = MultFloatNullables(trackDissolve, pathDissolve);
                dissolveArrow = MultFloatNullables(trackDissolveArrow, pathDissolveArrow);
                cuttable = MultFloatNullables(trackCuttable, pathCuttable);
            }
            else
            {
                positionOffset = pathPosition * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
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

            positionOffset = positionOffset?.Mirror();
            rotationOffset = rotationOffset?.Mirror();
            scaleOffset = scaleOffset?.Mirror();
            localRotationOffset = localRotationOffset?.Mirror();
        }
    }
}
