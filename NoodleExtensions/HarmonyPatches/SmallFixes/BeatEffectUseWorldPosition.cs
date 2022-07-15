using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    // Readjust spawn effect to take global position instead of local
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(BeatEffectSpawner))]
    internal static class BeatEffectUseWorldPosition
    {
        private static readonly MethodInfo _jumpStartPosGetter = AccessTools.PropertyGetter(typeof(NoteController), nameof(NoteController.jumpStartPos));
        private static readonly MethodInfo _beatEffectInit = AccessTools.Method(typeof(BeatEffect), nameof(BeatEffect.Init));

        private static readonly MethodInfo _transformGetter = AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform));
        private static readonly MethodInfo _positionGetter = AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.position));
        private static readonly MethodInfo _rotationGetter = AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.rotation));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatEffectSpawner.HandleNoteDidStartJump))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)

                // position
                /*
                 * -- beatEffect.transform.SetPositionAndRotation(noteController.worldRotation * noteController.jumpStartPos - new Vector3(0f, 0.15f, 0f), Quaternion.identity);
                 * ++ beatEffect.transform.SetPositionAndRotation(noteController.transform.position - new Vector3(0f, 0.15f, 0f), Quaternion.identity);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _jumpStartPosGetter))
                .Advance(-2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, _transformGetter),
                    new CodeInstruction(OpCodes.Call, _positionGetter))
                .RemoveInstructions(4)

                // rotation
                /*
                 * -- beatEffect.Init(color, this._effectDuration, noteController.worldRotation);
                 * ++ beatEffect.Init(color, this._effectDuration, noteController.transform.rotation);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _beatEffectInit))
                .Advance(-1)
                .RemoveInstruction()
                .Insert(
                    new CodeInstruction(OpCodes.Call, _transformGetter),
                    new CodeInstruction(OpCodes.Call, _rotationGetter))

                .InstructionEnumeration();
        }
    }
}
