using HarmonyLib;
using IPA.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NoodleExtensions.Animation;
using CustomJSONData;
using System.Reflection.Emit;
using UnityEngine;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteFloorMovement))]
    [HarmonyPatch("ManualUpdate")]
    internal class NoteFloorMovementManualUpdate
    {
        private static readonly MethodInfo _definiteNoteFloorMovement = SymbolExtensions.GetMethodInfo(() => DefiniteNoteFloorMovement(Vector3.zero, null));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundFinalPosition = false;
            // TODO: cache
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
            }
            if (!foundFinalPosition) Logger.Log("Failed to find _localPosition stfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteFloorMovement(Vector3 original, NoteFloorMovement noteFloorMovement)
        {
            dynamic dynData = NoteControllerUpdate._customNoteData.customData;
            dynamic animationObject = Trees.at(dynData, "_animation");
            Track track = AnimationHelper.GetTrack(dynData);
            AnimationHelper.GetDefinitePosition(animationObject, track, out PointData position);
            if (position != null)
            {
                Vector3 endPos = NoteControllerUpdate._floorEndPosAccessor(ref noteFloorMovement);
                return original + ((position.Interpolate(0) * _noteLinesDistance) - endPos);
            }
            else return original;
        }
    }
}