namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(Saber))]
    [HeckPatch("ManualUpdate")]
    internal static class SaberManualUpdate
    {
        private static readonly MethodInfo _addNewData = AccessTools.Method(typeof(SaberMovementData), nameof(SaberMovementData.AddNewData));
        private static readonly MethodInfo _addNewDataBetter = AccessTools.Method(typeof(SaberManualUpdate), nameof(AddNewDataBetter));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addNewData))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _addNewDataBetter)
                .InstructionEnumeration();
        }

        // We store all positions as localpositions so that abrupt changes in world position do not affect this
        // it gets converted back to world position to calculate cut
        private static void AddNewDataBetter(SaberMovementData movementData, Vector3 saberBladeTopPos, Vector3 saberBladeBottomPos, float time, Saber saber)
        {
            // Convert world pos to local
            Transform parent = saber.transform.parent.parent;
            saberBladeTopPos = parent.InverseTransformPoint(saberBladeTopPos);
            saberBladeBottomPos = parent.InverseTransformPoint(saberBladeBottomPos);
            movementData.AddNewData(saberBladeTopPos, saberBladeBottomPos, time);
        }
    }
}
