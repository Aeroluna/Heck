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

        // This method runs on each frame in the game scene, so avoid allocations (do NOT use Linq).
        internal void GetDefinitePositionOffset(
            NoodleObjectData.AnimationObjectData? animationObject,
            IReadOnlyList<Track>? tracks,
            float time,
            out Vector3? definitePosition)
        {
            Vector3? pathDefinitePosition = animationObject?.LocalDefinitePosition?.Interpolate(time);

            if (!pathDefinitePosition.HasValue && tracks != null)
            {
                if (tracks.Count > 1)
                {
                    Vector3? sumPathDefinitePosition = null;

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (Track track in tracks)
                    {
                        sumPathDefinitePosition = SumVectorNullables(sumPathDefinitePosition, track.GetVector3PathProperty(DEFINITE_POSITION, time));
                    }

                    pathDefinitePosition = sumPathDefinitePosition;
                }
                else
                {
                    pathDefinitePosition = tracks.First().GetVector3PathProperty(DEFINITE_POSITION, time);
                }
            }

            if (pathDefinitePosition.HasValue)
            {
                Vector3? pathPosition = animationObject?.LocalPosition?.Interpolate(time);
                Vector3? positionOffset = null;
                if (tracks != null)
                {
                    if (tracks.Count > 1)
                    {
                        Vector3? sumPathPosition = null;
                        Vector3? sumPositionOffset = null;
                        bool hasPathPosition = pathPosition.HasValue;

                        foreach (Track track in tracks)
                        {
                            if (!hasPathPosition)
                            {
                                sumPathPosition = SumVectorNullables(sumPathPosition, track.GetVector3PathProperty(OFFSET_POSITION, time));
                            }

                            sumPositionOffset = SumVectorNullables(sumPositionOffset, track.GetProperty<Vector3>(OFFSET_POSITION));
                        }

                        if (!hasPathPosition)
                        {
                            pathPosition = sumPathPosition;
                        }

                        positionOffset = SumVectorNullables(sumPositionOffset, pathPosition);
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

        // This method runs on each frame in the game scene, so avoid allocations (do NOT use Linq).
        internal void GetObjectOffset(
            NoodleObjectData.AnimationObjectData? animationObject,
            IReadOnlyList<Track>? tracks,
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
                    Vector3? sumPathPosition = null;
                    Quaternion? multPathRotation = null;
                    Vector3? multPathScale = null;
                    Quaternion? multPathLocalRotation = null;
                    float? multPathDissolve = null;
                    float? multPathDissolveArrow = null;
                    float? multPathCuttable = null;

                    Vector3? sumTrackPosition = null;
                    Quaternion? multTrackRotation = null;
                    Vector3? multTrackScale = null;
                    Quaternion? multTrackLocalRotation = null;
                    float? multTrackDissolve = null;
                    float? multTrackDissolveArrow = null;
                    float? multTrackCuttable = null;

                    bool hasPathPosition = pathPosition.HasValue;
                    bool hasPathRotation = pathRotation.HasValue;
                    bool hasPathScale = pathScale.HasValue;
                    bool hasPathLocalRotation = pathLocalRotation.HasValue;
                    bool hasPathDissolve = pathDissolve.HasValue;
                    bool hasPathDissolveArrow = pathDissolveArrow.HasValue;
                    bool hasPathCuttable = pathCuttable.HasValue;

                    foreach (Track? track in tracks)
                    {
                        if (!hasPathPosition)
                        {
                            sumPathPosition = SumVectorNullables(sumPathPosition, track.GetVector3PathProperty(OFFSET_POSITION, time));
                        }

                        if (!hasPathRotation)
                        {
                            multPathRotation = MultQuaternionNullables(multPathRotation, track.GetQuaternionPathProperty(OFFSET_ROTATION, time));
                        }

                        if (!hasPathScale)
                        {
                            multPathScale = MultVectorNullables(multPathScale, track.GetVector3PathProperty(SCALE, time));
                        }

                        if (!hasPathLocalRotation)
                        {
                            multPathLocalRotation = MultQuaternionNullables(multPathLocalRotation, track.GetQuaternionPathProperty(LOCAL_ROTATION, time));
                        }

                        if (!hasPathDissolve)
                        {
                            multPathDissolve = MultFloatNullables(multPathDissolve, track.GetLinearPathProperty(DISSOLVE, time));
                        }

                        if (!hasPathDissolveArrow)
                        {
                            multPathDissolveArrow = MultFloatNullables(multPathDissolveArrow, track.GetLinearPathProperty(DISSOLVE_ARROW, time));
                        }

                        if (!hasPathCuttable)
                        {
                            multPathCuttable = MultFloatNullables(multPathCuttable, track.GetLinearPathProperty(INTERACTABLE, time));
                        }

                        sumTrackPosition = SumVectorNullables(sumTrackPosition, track.GetProperty<Vector3>(OFFSET_POSITION));
                        multTrackRotation = MultQuaternionNullables(multTrackRotation, track.GetProperty<Quaternion>(OFFSET_ROTATION));
                        multTrackScale = MultVectorNullables(multTrackScale, track.GetProperty<Vector3>(SCALE));
                        multTrackLocalRotation = MultQuaternionNullables(multTrackLocalRotation, track.GetProperty<Quaternion>(LOCAL_ROTATION));
                        multTrackDissolve = MultFloatNullables(multTrackDissolve, track.GetProperty<float>(DISSOLVE));
                        multTrackDissolveArrow = MultFloatNullables(multTrackDissolveArrow, track.GetProperty<float>(DISSOLVE_ARROW));
                        multTrackCuttable = MultFloatNullables(multTrackCuttable, track.GetProperty<float>(INTERACTABLE));
                    }

                    if (!hasPathPosition)
                    {
                        pathPosition = sumPathPosition;
                    }

                    if (!hasPathRotation)
                    {
                        pathRotation = multPathRotation;
                    }

                    if (!hasPathScale)
                    {
                        pathScale = multPathScale;
                    }

                    if (!hasPathLocalRotation)
                    {
                        pathLocalRotation = multPathLocalRotation;
                    }

                    if (!hasPathDissolve)
                    {
                        pathDissolve = multPathDissolve;
                    }

                    if (!hasPathDissolveArrow)
                    {
                        pathDissolveArrow = multPathDissolveArrow;
                    }

                    if (!hasPathCuttable)
                    {
                        pathCuttable = multPathCuttable;
                    }

                    trackPosition = sumTrackPosition;
                    trackRotation = multTrackRotation;
                    trackScale = multTrackScale;
                    trackLocalRotation = multTrackLocalRotation;
                    trackDissolve = multTrackDissolve;
                    trackDissolveArrow = multTrackDissolveArrow;
                    trackCuttable = multTrackCuttable;
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
