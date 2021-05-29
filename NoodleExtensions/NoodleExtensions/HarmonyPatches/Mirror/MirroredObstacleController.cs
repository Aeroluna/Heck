namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(MirroredObstacleController))]
    [HeckPatch("Mirror")]
    internal static class MirroredObstacleControllerAwake
    {
        private static readonly FieldInfo _followedObstacleField = AccessTools.Field(typeof(MirroredObstacleController), "_followedObstacle");

        // Looks like you forgot to fill _followedObstacle beat games, i got you covered!
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundRemoveListeners = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundRemoveListeners &&
                    instructionList[i].opcode == OpCodes.Call &&
                    ((MethodInfo)instructionList[i].operand).Name == "RemoveListeners")
                {
                    foundRemoveListeners = true;
                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Stfld, _followedObstacleField),
                    };
                    instructionList.InsertRange(i + 1, codeInstructions);
                }
            }

            if (!foundRemoveListeners)
            {
                Plugin.Logger.Log("Failed to find call to RemoveListeners!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }

    [HeckPatch(typeof(MirroredObstacleController))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredObstacleControllerUpdatePositionAndRotation
    {
        // Must be overwritten to compensate for rotation
        private static bool Prefix(MirroredObstacleController __instance, ObstacleController ____followedObstacle, Transform ____transform, Transform ____followedTransform)
        {
            Vector3 position = ____followedTransform.position;
            Quaternion quaternion = ____followedTransform.rotation;
            position.y = -position.y;
            quaternion = quaternion.Reflect(Vector3.up);
            ____transform.SetPositionAndRotation(position, quaternion);

            if (____transform.localScale != ____followedTransform.localScale)
            {
                ____transform.localScale = ____followedTransform.localScale;
            }

            if (CutoutManager.ObstacleCutoutEffects.TryGetValue(__instance, out CutoutAnimateEffectWrapper cutoutAnimateEffect))
            {
                if (CutoutManager.ObstacleCutoutEffects.TryGetValue(____followedObstacle, out CutoutAnimateEffectWrapper followedCutoutAnimateEffect))
                {
                    cutoutAnimateEffect.SetCutout(followedCutoutAnimateEffect.Cutout);
                }
            }

            return false;
        }
    }
}
