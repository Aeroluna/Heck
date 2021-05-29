namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using UnityEngine;

    // Readjust spawn effect to take global position instead of local
    [HeckPatch(typeof(BeatEffectSpawner))]
    [HeckPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJump
    {
        private static readonly MethodInfo _getNoteControllerPosition = SymbolExtensions.GetMethodInfo(() => GetNoteControllerPosition(null));
        private static readonly MethodInfo _getNoteControllerRotation = SymbolExtensions.GetMethodInfo(() => GetNoteControllerRotation(null));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundInit = false;
            bool foundJumpStartPos = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundJumpStartPos &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_jumpStartPos")
                {
                    foundJumpStartPos = true;

                    instructionList.RemoveRange(i - 2, 4);
                    instructionList.Insert(i - 2, new CodeInstruction(OpCodes.Call, _getNoteControllerPosition));
                }
            }

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundInit &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "Init")
                {
                    foundInit = true;

                    instructionList[i - 1] = new CodeInstruction(OpCodes.Call, _getNoteControllerRotation);
                }
            }

            if (!foundJumpStartPos)
            {
                Plugin.Logger.Log("Failed to find callvirt to get_jumpStartPos!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundInit)
            {
                Plugin.Logger.Log("Failed to find callvirt to Init!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
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
