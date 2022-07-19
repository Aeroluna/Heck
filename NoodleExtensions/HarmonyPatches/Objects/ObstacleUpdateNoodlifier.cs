using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Animation;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class ObstacleUpdateNoodlifier : IAffinity, IDisposable
    {
        private static readonly FieldInfo _obstacleDataField = AccessTools.Field(typeof(ObstacleController), "_obstacleData");
        private static readonly FieldInfo _move1DurationField = AccessTools.Field(typeof(ObstacleController), "_move1Duration");
        private static readonly FieldInfo _finishMovementTime = AccessTools.Field(typeof(ObstacleController), "_finishMovementTime");

        private readonly CodeInstruction _obstacleTimeAdjust;
        private readonly DeserializedData _deserializedData;
        private readonly AnimationHelper _animationHelper;
        private readonly CutoutManager _cutoutManager;

        private ObstacleUpdateNoodlifier(
            [Inject(Id = ID)] DeserializedData deserializedData,
            AnimationHelper animationHelper,
            CutoutManager cutoutManager)
        {
            _deserializedData = deserializedData;
            _animationHelper = animationHelper;
            _cutoutManager = cutoutManager;
            _obstacleTimeAdjust = InstanceTranspilers.EmitInstanceDelegate<Func<float, ObstacleData, float, float, float>>(ObstacleTimeAdjust);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_obstacleTimeAdjust);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.ManualUpdate))]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- float num = this._audioTimeSyncController.songTime - this._startTimeOffset;
                 * ++ float num = ObstacletimeAdjust(this._audioTimeSyncController.songTime - this._startTimeOffset, this._obstacleData, this._move1Duration, this._finishMovementTime);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_0))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _obstacleDataField),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _move1DurationField),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _finishMovementTime),
                    _obstacleTimeAdjust)
                .InstructionEnumeration();
        }

        private float ObstacleTimeAdjust(float original, ObstacleData obstacleData, float move1Duration, float finishMovementTime)
        {
            if (!(original > move1Duration) || !_deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData))
            {
                return original;
            }

            float? time = noodleData.Track?.Select(n => n.GetLinearProperty(TIME)).FirstOrDefault(n => n.HasValue);
            if (time.HasValue)
            {
                return (time.Value * (finishMovementTime - move1Duration)) + move1Duration;
            }

            return original;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.ManualUpdate))]
        private void Prefix(
            ObstacleController __instance,
            ObstacleData ____obstacleData,
            AudioTimeSyncController ____audioTimeSyncController,
            float ____startTimeOffset,
            ref Vector3 ____startPos,
            ref Vector3 ____midPos,
            ref Vector3 ____endPos,
            float ____move1Duration,
            float ____move2Duration,
            float ____obstacleDuration,
            ref Quaternion ____worldRotation,
            ref Quaternion ____inverseWorldRotation,
            ref Bounds ____bounds)
        {
            if (!_deserializedData.Resolve(____obstacleData, out NoodleObstacleData? noodleData))
            {
                return;
            }

            if (noodleData.InternalDoUnhide)
            {
                __instance.Hide(false);
            }

            List<Track>? tracks = noodleData.Track;
            NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
            if (tracks == null && animationObject == null)
            {
                return;
            }

            float? time = noodleData.GetTimeProperty();
            float normalTime;
            if (time.HasValue)
            {
                normalTime = time.Value;
            }
            else
            {
                float elapsedTime = ____audioTimeSyncController.songTime - ____startTimeOffset;
                normalTime = (elapsedTime - ____move1Duration) / (____move2Duration + ____obstacleDuration);
            }

            _animationHelper.GetObjectOffset(
                animationObject,
                tracks,
                normalTime,
                out Vector3? positionOffset,
                out Quaternion? rotationOffset,
                out Vector3? scaleOffset,
                out Quaternion? localRotationOffset,
                out float? dissolve,
                out _,
                out float? cuttable);

            if (positionOffset.HasValue)
            {
                Vector3 startPos = noodleData.InternalStartPos;
                Vector3 midPos = noodleData.InternalMidPos;
                Vector3 endPos = noodleData.InternalEndPos;

                Vector3 offset = positionOffset.Value;
                ____startPos = startPos + offset;
                ____midPos = midPos + offset;
                ____endPos = endPos + offset;
            }

            Transform transform = __instance.transform;

            if (rotationOffset.HasValue || localRotationOffset.HasValue)
            {
                Quaternion worldRotation = noodleData.InternalWorldRotation;
                Quaternion localRotation = noodleData.InternalLocalRotation;

                Quaternion worldRotationQuatnerion = worldRotation;
                if (rotationOffset.HasValue)
                {
                    worldRotationQuatnerion *= rotationOffset.Value;
                    ____worldRotation = worldRotationQuatnerion;
                    ____inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
                }

                worldRotationQuatnerion *= localRotation;

                if (localRotationOffset.HasValue)
                {
                    worldRotationQuatnerion *= localRotationOffset.Value;
                }

                transform.localRotation = worldRotationQuatnerion;
            }

            if (cuttable.HasValue)
            {
                if (cuttable.Value >= 1)
                {
                    ____bounds.size = Vector3.zero;
                }
                else
                {
                    Vector3 boundsSize = noodleData.InternalBoundsSize;
                    ____bounds.size = boundsSize;
                }
            }

            if (scaleOffset.HasValue)
            {
                transform.localScale = scaleOffset.Value;
            }

            if (dissolve.HasValue)
            {
                _cutoutManager.ObstacleCutoutEffects[__instance].SetCutout(dissolve.Value);
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.GetPosForTime))]
        private bool DefinitePosForTime(
            ref Vector3 __result,
            ObstacleData ____obstacleData,
            Vector3 ____startPos,
            Vector3 ____midPos,
            float ____move1Duration,
            float ____move2Duration,
            float ____obstacleDuration,
            float time)
        {
            if (!_deserializedData.Resolve(____obstacleData, out NoodleObstacleData? noodleData))
            {
                return true;
            }

            float jumpTime = Mathf.Clamp((time - ____move1Duration) / (____move2Duration + ____obstacleDuration), 0, 1);
            _animationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, jumpTime, out Vector3? position);

            if (!position.HasValue)
            {
                return true;
            }

            Vector3 noteOffset = noodleData.InternalNoteOffset;
            Vector3 definitePosition = position.Value + noteOffset;
            if (time < ____move1Duration)
            {
                __result = Vector3.LerpUnclamped(____startPos, ____midPos, time / ____move1Duration);
                __result += definitePosition - ____midPos;
            }
            else
            {
                __result = definitePosition;
            }

            return false;
        }
    }
}
