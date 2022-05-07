using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    [HeckPatch(PatchType.Features)]
    internal static class SaberPlayerMovementFix
    {
        private static readonly MethodInfo _addNewData = AccessTools.Method(typeof(SaberMovementData), nameof(SaberMovementData.AddNewData));
        private static readonly MethodInfo _addNewDataBetter = AccessTools.Method(typeof(SaberPlayerMovementFix), nameof(AddNewDataBetter));

        private static readonly MethodInfo _convertToWorld = AccessTools.Method(typeof(SaberPlayerMovementFix), nameof(ConvertToWorld));

        private static readonly MethodInfo _movementDataGetter = AccessTools.PropertyGetter(typeof(Saber), nameof(Saber.movementData));
        private static readonly MethodInfo _createSaberMovementData = AccessTools.Method(typeof(SaberPlayerMovementFix), nameof(CreateSaberMovementData));

        private static readonly Dictionary<Saber, SaberMovementData> _worldMovementData = new();

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Saber), nameof(Saber.ManualUpdate))]
        private static IEnumerable<CodeInstruction> SaberTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addNewData))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Set(OpCodes.Call, _addNewDataBetter)
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(NoteCutter), nameof(NoteCutter.Cut))]
        private static IEnumerable<CodeInstruction> NoteCutterTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloca_S, 2),
                    new CodeInstruction(OpCodes.Ldloca_S, 3),
                    new CodeInstruction(OpCodes.Call, _convertToWorld))
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(SaberModelController), nameof(SaberModelController.Init))]
        private static IEnumerable<CodeInstruction> SaberWorldMovementTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _movementDataGetter))
                .SetOperandAndAdvance(_createSaberMovementData)
                .InstructionEnumeration();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaberTrail), nameof(SaberTrail.OnDestroy))]
        private static void CleanupWorldMovement(IBladeMovementData ____movementData)
        {
            Saber? saber = _worldMovementData.FirstOrDefault(n => n.Value == ____movementData).Key;
            if (saber != null)
            {
                _worldMovementData.Remove(saber);
            }
        }

        // We store all positions as localpositions so that abrupt changes in world position do not affect this
        // it gets converted back to world position to calculate cut
        private static void AddNewDataBetter(SaberMovementData movementData, Vector3 saberBladeTopPos, Vector3 saberBladeBottomPos, float time, Saber saber)
        {
            if (_worldMovementData.TryGetValue(saber, out SaberMovementData worldMovementData))
            {
                worldMovementData.AddNewData(saberBladeTopPos, saberBladeBottomPos, time);
            }

            if (saber.transform.parent == null)
            {
                return;
            }

            // Convert world pos to local
            Transform? playerTransform = saber.transform.parent.parent;

            // For some reason, SiraUtil's FPFCToggle unparents the left and right hand from VRGameCore
            // This only affects fpfc so w/e, just null check and go home
            if (playerTransform != null)
            {
                saberBladeTopPos = playerTransform.InverseTransformPoint(saberBladeTopPos);
                saberBladeBottomPos = playerTransform.InverseTransformPoint(saberBladeBottomPos);
            }

            movementData.AddNewData(saberBladeTopPos, saberBladeBottomPos, time);
        }

        private static void ConvertToWorld(Saber saber, ref Vector3 topPos, ref Vector3 bottomPos)
        {
            if (saber.transform.parent == null)
            {
                return;
            }

            Transform playerTransform = saber.transform.parent.parent;

            if (playerTransform == null)
            {
                return;
            }

            topPos = playerTransform.TransformPoint(topPos);
            bottomPos = playerTransform.TransformPoint(bottomPos);
        }

        private static IBladeMovementData CreateSaberMovementData(Saber saber)
        {
            // use world movement data for saber trail
            SaberMovementData movementData = new();
            _worldMovementData.Add(saber, movementData);
            return movementData;
        }
    }
}
