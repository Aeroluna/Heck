namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using Heck.Animation;
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.NoodleObjectDataManager;
    using static NoodleExtensions.Plugin;

    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("Init")]
    internal static class ObstacleControllerInit
    {
        internal static readonly List<ObstacleController> _activeObstacles = new List<ObstacleController>();

        private static readonly FieldInfo _worldRotationField = AccessTools.Field(typeof(ObstacleController), "_worldRotation");
        private static readonly FieldInfo _inverseWorldRotationField = AccessTools.Field(typeof(ObstacleController), "_inverseWorldRotation");
        private static readonly MethodInfo _widthGetter = AccessTools.PropertyGetter(typeof(ObstacleData), nameof(ObstacleData.width));
        private static readonly FieldInfo _lengthField = AccessTools.Field(typeof(ObstacleController), "_length");

        private static readonly MethodInfo _getCustomWidth = AccessTools.Method(typeof(ObstacleControllerInit), nameof(GetCustomWidth));
        private static readonly MethodInfo _getWorldRotation = AccessTools.Method(typeof(ObstacleControllerInit), nameof(GetWorldRotation));
        private static readonly MethodInfo _getCustomLength = AccessTools.Method(typeof(ObstacleControllerInit), nameof(GetCustomLength));
        private static readonly MethodInfo _invertQuaternion = AccessTools.Method(typeof(ObstacleControllerInit), nameof(InvertQuaternion));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)

                // world rotation
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, _worldRotationField))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Call, _getWorldRotation))
                .RemoveInstructionsWithOffsets(-4, -1)

                // inverse world rotation
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, _inverseWorldRotationField))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _worldRotationField),
                    new CodeInstruction(OpCodes.Call, _invertQuaternion))
                .RemoveInstructionsWithOffsets(-5, -1)

                // width
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _widthGetter))
                .Advance(2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getCustomWidth))

                // length
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, _lengthField))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getCustomLength))

                .InstructionEnumeration();
        }

        private static void Postfix(ObstacleController __instance, Quaternion ____worldRotation, ObstacleData obstacleData, Vector3 ____startPos, Vector3 ____midPos, Vector3 ____endPos, ref Bounds ____bounds)
        {
            NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(obstacleData);
            if (noodleData == null)
            {
                return;
            }

            Quaternion? localRotationQuaternion = noodleData.LocalRotationQuaternion;

            Transform transform = __instance.transform;

            Quaternion localRotation = Quaternion.identity;
            if (localRotationQuaternion.HasValue)
            {
                localRotation = localRotationQuaternion.Value;
                transform.localRotation = ____worldRotation * localRotation;
            }

            if (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector3.one; // This is a fix for animation due to obstacles being recycled
            }

            IEnumerable<Track>? tracks = noodleData.Track;
            if (tracks != null)
            {
                foreach (Track track in tracks)
                {
                    // add to gameobjects
                    track.AddGameObject(__instance.gameObject);
                }
            }

            bool? cuttable = noodleData.Cuttable;
            if (cuttable.HasValue && !cuttable.Value)
            {
                ____bounds.size = Vector3.zero;
            }
            else
            {
                _activeObstacles.Add(__instance);
            }

            noodleData.StartPos = ____startPos;
            noodleData.MidPos = ____midPos;
            noodleData.EndPos = ____endPos;
            noodleData.LocalRotation = localRotation;
            noodleData.BoundsSize = ____bounds.size;
        }

        private static Quaternion InvertQuaternion(Quaternion quaternion)
        {
            return Quaternion.Inverse(quaternion);
        }

        private static Quaternion GetWorldRotation(ObstacleData obstacleData, float @default)
        {
            Quaternion worldRotation = Quaternion.Euler(0, @default, 0);

            NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(obstacleData);
            if (noodleData != null)
            {
                Quaternion? worldRotationQuaternion = noodleData.WorldRotationQuaternion;
                if (worldRotationQuaternion.HasValue)
                {
                    worldRotation = worldRotationQuaternion.Value;
                }

                noodleData.WorldRotation = worldRotation;
            }

            return worldRotation;
        }

        private static float GetCustomWidth(float @default, ObstacleData obstacleData)
        {
            NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(obstacleData);
            if (noodleData != null)
            {
                float? width = noodleData.Width;
                if (width.HasValue)
                {
                    return width.Value;
                }
            }

            return @default;
        }

        private static float GetCustomLength(float @default, ObstacleData obstacleData)
        {
            NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(obstacleData);
            if (noodleData != null)
            {
                float? length = noodleData.Length;
                if (length.HasValue)
                {
                    return length.Value * NoteLinesDistance;
                }
            }

            return @default;
        }
    }

    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("ManualUpdate")]
    internal static class ObstacleControllerManualUpdate
    {
        private static readonly FieldInfo _obstacleDataField = AccessTools.Field(typeof(ObstacleController), "_obstacleData");
        private static readonly FieldInfo _move1DurationField = AccessTools.Field(typeof(ObstacleController), "_move1Duration");
        private static readonly FieldInfo _finishMovementTime = AccessTools.Field(typeof(ObstacleController), "_finishMovementTime");
        private static readonly MethodInfo _obstacleTimeAdjust = AccessTools.Method(typeof(ObstacleControllerManualUpdate), nameof(ObstacleTimeAdjust));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundTime = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundTime &&
                    instructionList[i].opcode == OpCodes.Stloc_0)
                {
                    foundTime = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldfld, _obstacleDataField));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Ldfld, _move1DurationField));
                    instructionList.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 5, new CodeInstruction(OpCodes.Ldfld, _finishMovementTime));
                    instructionList.Insert(i + 6, new CodeInstruction(OpCodes.Call, _obstacleTimeAdjust));
                }
            }

            if (!foundTime)
            {
                Plugin.Logger.Log("Failed to find stloc.0!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static float ObstacleTimeAdjust(float original, ObstacleData obstacleData, float move1Duration, float finishMovementTime)
        {
            if (original > move1Duration)
            {
                NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(obstacleData);
                if (noodleData != null)
                {
                    float? time = noodleData.Track?.Select(n => AnimationHelper.TryGetProperty<float?>(n, TIME)).FirstOrDefault(n => n.HasValue);
                    if (time.HasValue)
                    {
                        return (time.Value * (finishMovementTime - move1Duration)) + move1Duration;
                    }
                }
            }

            return original;
        }

        private static void Prefix(
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
            NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(____obstacleData);
            if (noodleData == null)
            {
                return;
            }

            IEnumerable<Track>? tracks = noodleData.Track;
            NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
            if (tracks != null || animationObject != null)
            {
                // idk i just copied base game time
                float elapsedTime = ____audioTimeSyncController.songTime - ____startTimeOffset;
                float normalTime = (elapsedTime - ____move1Duration) / (____move2Duration + ____obstacleDuration);

                Animation.AnimationHelper.GetObjectOffset(animationObject, tracks, normalTime, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? _, out float? cuttable);

                if (positionOffset.HasValue)
                {
                    Vector3 startPos = noodleData.StartPos;
                    Vector3 midPos = noodleData.MidPos;
                    Vector3 endPos = noodleData.EndPos;

                    Vector3 offset = positionOffset.Value;
                    ____startPos = startPos + offset;
                    ____midPos = midPos + offset;
                    ____endPos = endPos + offset;
                }

                Transform transform = __instance.transform;

                if (rotationOffset.HasValue || localRotationOffset.HasValue)
                {
                    Quaternion worldRotation = noodleData.WorldRotation;
                    Quaternion localRotation = noodleData.LocalRotation;

                    Quaternion worldRotationQuatnerion = worldRotation;
                    if (rotationOffset.HasValue)
                    {
                        worldRotationQuatnerion *= rotationOffset.Value;
                        Quaternion inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
                        ____worldRotation = worldRotationQuatnerion;
                        ____inverseWorldRotation = inverseWorldRotation;
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
                        if (____bounds.size != Vector3.zero)
                        {
                            ____bounds.size = Vector3.zero;
                        }
                    }
                    else
                    {
                        Vector3 boundsSize = noodleData.BoundsSize;
                        if (____bounds.size != boundsSize)
                        {
                            ____bounds.size = boundsSize;
                        }
                    }
                }

                if (scaleOffset.HasValue)
                {
                    transform.localScale = scaleOffset.Value;
                }

                if (dissolve.HasValue)
                {
                    if (CutoutManager.ObstacleCutoutEffects.TryGetValue(__instance, out CutoutAnimateEffectWrapper cutoutAnimateEffect))
                    {
                        cutoutAnimateEffect.SetCutout(dissolve.Value);
                    }
                }
            }

            if (noodleData.DoUnhide)
            {
                __instance.hide = false;
            }
        }
    }

    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("GetPosForTime")]
    internal static class ObstacleControllerGetPosForTime
    {
        private static bool Prefix(
            ref Vector3 __result,
            ObstacleData ____obstacleData,
            Vector3 ____startPos,
            Vector3 ____midPos,
            float ____move1Duration,
            float ____move2Duration,
            float ____obstacleDuration,
            float time)
        {
            NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(____obstacleData);
            if (noodleData == null)
            {
                return true;
            }

            float jumpTime = Mathf.Clamp((time - ____move1Duration) / (____move2Duration + ____obstacleDuration), 0, 1);
            Animation.AnimationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, jumpTime, out Vector3? position);

            if (position.HasValue)
            {
                Vector3 noteOffset = noodleData.NoteOffset;
                Vector3 definitePosition = position.Value + noteOffset;
                definitePosition.x += noodleData.XOffset;
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

            return true;
        }
    }
}
