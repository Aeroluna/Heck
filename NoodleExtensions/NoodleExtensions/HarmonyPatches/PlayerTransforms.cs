namespace NoodleExtensions.HarmonyPatches
{
    /*using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;*/
    using UnityEngine;

    // Possibly breaks in multiplayer? We don't support that anyways.
    [NoodlePatch(typeof(PlayerTransforms))]
    [NoodlePatch("Update")]
    internal static class PlayerTransformsUpdate
    {
        private static void Postfix(ref Vector3 ____headPseudoLocalPos, Transform ____headTransform)
        {
            ____headPseudoLocalPos = ____headTransform.localPosition;
        }
    }

    /*[NoodlePatch(typeof(PlayerTransforms))]
    [NoodlePatch("MoveTowardsHead")]
    internal static class PlayerTransformsMoveTowardsHead
    {
        private static readonly MethodInfo _headOffsetZNotPsuedo = SymbolExtensions.GetMethodInfo(() => HeadOffsetZNotPsuedo(Quaternion.identity, Vector3.zero));
        private static readonly FieldInfo _headTransformField = AccessTools.Field(typeof(PlayerTransforms), "_headTransform");
        private static readonly MethodInfo _localPositionMethod = typeof(Transform).GetProperty("localPosition").GetGetMethod();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundHeadLocalPos = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundHeadLocalPos &&
                    instructionList[i].opcode == OpCodes.Call &&
                  ((MethodInfo)instructionList[i].operand).Name == "HeadOffsetZ")
                {
                    foundHeadLocalPos = true;
                    instructionList[i].operand = _headOffsetZNotPsuedo;

                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldfld, _headTransformField));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Callvirt, _localPositionMethod));

                    instructionList.RemoveAt(0);
                }
            }

            if (!foundHeadLocalPos)
            {
                NoodleLogger.Log("Failed to find call to HeadOffsetZ!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static float HeadOffsetZNotPsuedo(Quaternion noteInverseWorldRotation, Vector3 localPos)
        {
            return (noteInverseWorldRotation * localPos).z;
        }
    }*/
}
