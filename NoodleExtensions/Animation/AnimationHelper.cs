using System.Collections.Generic;
using System.Linq;
using Heck.Animation;
using UnityEngine;
using static Heck.Animation.AnimationHelper;
using static Heck.NullableExtensions;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    public static class AnimationHelper
    {
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

        internal static void GetDefinitePositionOffset(NoodleObjectData.AnimationObjectData? animationObject, List<Track>? tracks, float time, out Vector3? definitePosition)
        {
            Vector3? pathDefinitePosition = animationObject?.LocalDefinitePosition?.Interpolate(time);

            if (!pathDefinitePosition.HasValue && tracks != null)
            {
                pathDefinitePosition = tracks.Count > 1
                    ? SumVectorNullables(tracks.Select(n => TryGetVector3PathProperty(n, DEFINITE_POSITION, time)))
                    : TryGetVector3PathProperty(tracks.First(), DEFINITE_POSITION, time);
            }

            if (pathDefinitePosition.HasValue)
            {
                Vector3? pathPosition = animationObject?.LocalPosition?.Interpolate(time);
                Vector3? positionOffset = null;
                if (tracks != null)
                {
                    if (tracks.Count > 1)
                    {
                        pathPosition ??= SumVectorNullables(tracks.Select(n => TryGetVector3PathProperty(n, POSITION, time)));
                        positionOffset = SumVectorNullables(SumVectorNullables(tracks.Select(n => TryGetProperty<Vector3?>(n, POSITION))), pathPosition);
                    }
                    else
                    {
                        Track track = tracks.First();
                        pathPosition ??= TryGetVector3PathProperty(track, POSITION, time);
                        positionOffset = SumVectorNullables(TryGetProperty<Vector3?>(track, POSITION), pathPosition);
                    }
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

        internal static void GetObjectOffset(
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
                    pathPosition ??= SumVectorNullables(tracks.Select(n => TryGetVector3PathProperty(n, POSITION, time)));
                    pathRotation ??= MultQuaternionNullables(tracks.Select(n => TryGetQuaternionPathProperty(n, ROTATION, time)));
                    pathScale ??= MultVectorNullables(tracks.Select(n => TryGetVector3PathProperty(n, SCALE, time)));
                    pathLocalRotation ??= MultQuaternionNullables(tracks.Select(n => TryGetQuaternionPathProperty(n, LOCAL_ROTATION, time)));
                    pathDissolve ??= MultFloatNullables(tracks.Select(n => TryGetLinearPathProperty(n, DISSOLVE, time)));
                    pathDissolveArrow ??= MultFloatNullables(tracks.Select(n => TryGetLinearPathProperty(n, DISSOLVE_ARROW, time)));
                    pathCuttable ??= MultFloatNullables(tracks.Select(n => TryGetLinearPathProperty(n, CUTTABLE, time)));

                    trackPosition = SumVectorNullables(tracks.Select(n => TryGetProperty<Vector3?>(n, POSITION)));
                    trackRotation = MultQuaternionNullables(tracks.Select(n => TryGetProperty<Quaternion?>(n, ROTATION)));
                    trackScale = MultVectorNullables(tracks.Select(n => TryGetProperty<Vector3?>(n, SCALE)));
                    trackLocalRotation = MultQuaternionNullables(tracks.Select(n => TryGetProperty<Quaternion?>(n, LOCAL_ROTATION)));
                    trackDissolve = MultFloatNullables(tracks.Select(n => TryGetProperty<float?>(n, DISSOLVE)));
                    trackDissolveArrow = MultFloatNullables(tracks.Select(n => TryGetProperty<float?>(n, DISSOLVE_ARROW)));
                    trackCuttable = MultFloatNullables(tracks.Select(n => TryGetProperty<float?>(n, CUTTABLE)));
                }
                else
                {
                    Track track = tracks.First();
                    pathPosition ??= TryGetVector3PathProperty(track, POSITION, time);
                    pathRotation ??= TryGetQuaternionPathProperty(track, ROTATION, time);
                    pathScale ??= TryGetVector3PathProperty(track, SCALE, time);
                    pathLocalRotation ??= TryGetQuaternionPathProperty(track, LOCAL_ROTATION, time);
                    pathDissolve ??= TryGetLinearPathProperty(track, DISSOLVE, time);
                    pathDissolveArrow ??= TryGetLinearPathProperty(track, DISSOLVE_ARROW, time);
                    pathCuttable ??= TryGetLinearPathProperty(track, CUTTABLE, time);

                    trackPosition = TryGetProperty<Vector3?>(track, POSITION);
                    trackRotation = TryGetProperty<Quaternion?>(track, ROTATION);
                    trackScale = TryGetProperty<Vector3?>(track, SCALE);
                    trackLocalRotation = TryGetProperty<Quaternion?>(track, LOCAL_ROTATION);
                    trackDissolve = TryGetProperty<float?>(track, DISSOLVE);
                    trackDissolveArrow = TryGetProperty<float?>(track, DISSOLVE_ARROW);
                    trackCuttable = TryGetProperty<float?>(track, CUTTABLE);
                }

                positionOffset = SumVectorNullables(trackPosition, pathPosition) * NoteLinesDistance;
                rotationOffset = MultQuaternionNullables(trackRotation, pathRotation);
                scaleOffset = MultVectorNullables(trackScale, pathScale);
                localRotationOffset = MultQuaternionNullables(trackLocalRotation, pathLocalRotation);
                dissolve = MultFloatNullables(trackDissolve, pathDissolve);
                dissolveArrow = MultFloatNullables(trackDissolveArrow, pathDissolveArrow);
                cuttable = MultFloatNullables(trackCuttable, pathCuttable);
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

            if (!LeftHandedMode)
            {
                return;
            }

            MirrorVectorNullable(ref positionOffset);
            MirrorQuaternionNullable(ref rotationOffset);
            MirrorQuaternionNullable(ref localRotationOffset);
        }
    }
}
