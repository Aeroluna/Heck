namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using HarmonyLib;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [NoodlePatch(typeof(NoteFloorMovement))]
    [NoodlePatch("ManualUpdate")]
    internal static class NoteFloorMovementManualUpdate
    {
        private static readonly MethodInfo _definiteNoteFloorMovement = SymbolExtensions.GetMethodInfo(() => DefiniteNoteFloorMovement(Vector3.zero, null));
        private static readonly MethodInfo _setLocalPosition = typeof(Transform).GetProperty("localPosition").GetSetMethod();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundFinalPosition = false;
            bool foundPosition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundFinalPosition &&
                    instructionList[i].opcode == OpCodes.Stfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "_localPosition")
                {
                    foundFinalPosition = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _definiteNoteFloorMovement));
                }

                if (!foundPosition &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "set_position")
                {
                    foundPosition = true;
                    instructionList[i].operand = _setLocalPosition;
                }
            }

            if (!foundFinalPosition)
            {
                NoodleLogger.Log("Failed to find _localPosition stfld!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundPosition)
            {
                NoodleLogger.Log("Failed to find callvirt to set_position!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteFloorMovement(Vector3 original, NoteFloorMovement noteFloorMovement)
        {
            dynamic dynData = NoteControllerUpdate.CustomNoteData.customData;
            dynamic animationObject = Trees.at(dynData, "_animation");
            Track track = Trees.at(dynData, "track");
            AnimationHelper.GetDefinitePositionOffset(animationObject, track, 0, out Vector3? position);
            if (position.HasValue)
            {
                Vector3 noteOffset = Trees.at(dynData, "noteOffset");
                Vector3 endPos = NoteControllerUpdate._floorEndPosAccessor(ref noteFloorMovement);
                return original + (position.Value + noteOffset - endPos);
            }
            else
            {
                return original;
            }
        }
    }
}
