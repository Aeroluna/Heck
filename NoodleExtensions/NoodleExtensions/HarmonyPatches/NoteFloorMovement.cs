using CustomJSONData;
using HarmonyLib;
using NoodleExtensions.Animation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteFloorMovement))]
    [NoodlePatch("ManualUpdate")]
    internal class NoteFloorMovementManualUpdate
    {
        private static readonly MethodInfo _definiteNoteFloorMovement = SymbolExtensions.GetMethodInfo(() => DefiniteNoteFloorMovement(Vector3.zero, null));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundFinalPosition = false;
            int instructionListCount = instructionList.Count;
            for (int i = 0; i < instructionListCount; i++)
            {
                if (!foundFinalPosition &&
                    instructionList[i].opcode == OpCodes.Stfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "_localPosition")
                {
                    foundFinalPosition = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _definiteNoteFloorMovement));
                }
            }
            if (!foundFinalPosition) Logger.Log("Failed to find _localPosition stfld!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteFloorMovement(Vector3 original, NoteFloorMovement noteFloorMovement)
        {
            dynamic dynData = NoteControllerUpdate._customNoteData.customData;
            dynamic animationObject = Trees.at(dynData, "_animation");
            Track track = Trees.at(dynData, "track");
            AnimationHelper.GetDefinitePositionOffset(animationObject, track, 0, out Vector3? position);
            if (position.HasValue)
            {
                Vector3 noteOffset = Trees.at(dynData, "noteOffset");
                Vector3 endPos = NoteControllerUpdate._floorEndPosAccessor(ref noteFloorMovement);
                return original + (position.Value + noteOffset - endPos);
            }
            else return original;
        }
    }
}
