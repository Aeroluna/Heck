using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Reflection.Emit;
using NoodleExtensions.Animation;
using CustomJSONData;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteJump))]
    [HarmonyPatch("ManualUpdate")]
    internal class NoteJumpManualUpdate
    {
        private static readonly FieldInfo _localPositionField = AccessTools.Field(typeof(NoteJump), "_localPosition");
        private static readonly MethodInfo _definiteNoteJump = SymbolExtensions.GetMethodInfo(() => DefiniteNoteJump(Vector3.zero, 0));
        private static readonly MethodInfo _convertToLocalSpace = SymbolExtensions.GetMethodInfo(() => ConvertToLocalSpace(null));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundPosition = false;
            bool foundTransformUp = false;
            int instructionListCount = instructionList.Count;
            for (int i = 0; i < instructionListCount; i++)
            {
                if (!foundPosition &&
                    instructionList[i].opcode == OpCodes.Stind_R4)
                {
                    foundPosition = true;
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 4, new CodeInstruction(OpCodes.Ldfld, _localPositionField));
                    instructionList.Insert(i + 5, new CodeInstruction(OpCodes.Ldloc_1));
                    instructionList.Insert(i + 6, new CodeInstruction(OpCodes.Call, _definiteNoteJump));
                    instructionList.Insert(i + 7, new CodeInstruction(OpCodes.Stfld, _localPositionField));
                }
                if (instructionList[i].opcode == OpCodes.Callvirt &&
                  ((MethodInfo)instructionList[i].operand).Name == "get_up")
                {
                    instructionList[i] = new CodeInstruction(OpCodes.Call, _convertToLocalSpace);
                }
            }
            if (!foundPosition) Logger.Log("Failed to find stind.r4, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundTransformUp) Logger.Log("Failed to find call to get_up, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteJump(Vector3 original, float time)
        {
            dynamic dynData = NoteControllerUpdate._customNoteData.customData;
            dynamic animationObject = Trees.at(dynData, "_animation");
            Track track = AnimationHelper.GetTrack(dynData);
            AnimationHelper.GetDefinitePosition(animationObject, out PointData position);
            position = position ?? track?.definitePosition;
            if (position != null) return position.Interpolate(time) * _noteLinesDistance;
            else return original;
        }

        private static Vector3 ConvertToLocalSpace(Transform rotatedObject)
        {
            return rotatedObject.parent.InverseTransformDirection(rotatedObject.up); ;
        }
    }
}