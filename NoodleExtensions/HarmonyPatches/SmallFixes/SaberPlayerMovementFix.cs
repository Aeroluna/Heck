using System.Collections.Generic;
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

        // Set trail parent so it follows always playerm ovement
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaberTrail), nameof(SaberTrail.Init))]
        private static void ParentSaberTrail(SaberTrail __instance, TrailRenderer ____trailRenderer)
        {
            // Parent to VRGameCore
            ____trailRenderer.transform.SetParent(__instance.transform.parent.parent.parent, false);
        }

        // We store all positions as localpositions so that abrupt changes in world position do not affect this
        // it gets converted back to world position to calculate cut
        private static void AddNewDataBetter(SaberMovementData movementData, Vector3 saberBladeTopPos, Vector3 saberBladeBottomPos, float time, Saber saber)
        {
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
            Transform playerTransform = saber.transform.parent.parent;

            // For some reason, SiraUtil's FPFCToggle unparents the left and right hand from VRGameCore
            // This only affects fpfc so w/e, just null check and go home
            if (playerTransform == null)
            {
                return;
            }

            topPos = playerTransform.TransformPoint(topPos);
            bottomPos = playerTransform.TransformPoint(bottomPos);
        }
    }
}
