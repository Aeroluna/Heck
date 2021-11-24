using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    // Readjust spawn effect to take global position instead of local
    [HeckPatch(typeof(BeatEffectSpawner))]
    [HeckPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJump
    {
        private static readonly MethodInfo _jumpStartPosGetter = AccessTools.PropertyGetter(typeof(NoteController), nameof(NoteController.jumpStartPos));
        private static readonly MethodInfo _beatEffectInit = AccessTools.Method(typeof(BeatEffect), nameof(BeatEffect.Init));

        private static readonly MethodInfo _getNoteControllerPosition = AccessTools.Method(typeof(BeatEffectSpawnerHandleNoteDidStartJump), nameof(GetNoteControllerPosition));
        private static readonly MethodInfo _getNoteControllerRotation = AccessTools.Method(typeof(BeatEffectSpawnerHandleNoteDidStartJump), nameof(GetNoteControllerRotation));

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)

                // position
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _jumpStartPosGetter))
                .Advance(-2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, _getNoteControllerPosition))
                .RemoveInstructions(4)

                // rotation
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _beatEffectInit))
                .Advance(-1)
                .Set(OpCodes.Call, _getNoteControllerRotation)

                .InstructionEnumeration();
        }

        private static Vector3 GetNoteControllerPosition(NoteController noteController)
        {
            return noteController.transform.position;
        }

        private static Quaternion GetNoteControllerRotation(NoteController noteController)
        {
            return noteController.transform.rotation;
        }
    }
}
