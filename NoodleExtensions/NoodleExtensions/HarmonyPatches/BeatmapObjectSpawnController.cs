using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using static NoodleExtensions.Plugin;
using static NoodleExtensions.NoodleController.BeatmapObjectSpawnMovementDataVariables;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    [HarmonyPatch("SpawnObstacle")]
    internal class BeatmapObjectSpawnControllerSpawnObstacle
    {
        private static readonly MethodInfo jumpDuration = SymbolExtensions.GetMethodInfo(() => GetJumpDuration(null, 0));
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundJumpDuration = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundJumpDuration &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_jumpDuration")
                {
                    foundJumpDuration = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, jumpDuration));
                    instructionList.Insert(i - 2, new CodeInstruction(OpCodes.Ldarg_1));
                }
            }
            if (!foundJumpDuration) Logger.Log("Failed to find get_jumpDuration call, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static float GetJumpDuration(ObstacleData obstacleData, float @default)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                float? _njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? _spawnoffset = (float?)Trees.at(dynData, SPAWNOFFSET);

                float _localNoteJumpMovementSpeed = _njs ?? _noteJumpMovementSpeed;
                float _localNoteJumpStartBeatOffset = _spawnoffset ?? _noteJumpStartBeatOffset;
                float num = 60f / _startBPM;
                float num2 = _startHalfJumpDurationInBeats;
                while (_localNoteJumpMovementSpeed * num * num2 > _maxHalfJumpDistance)
                {
                    num2 /= 2f;
                }
                num2 += _localNoteJumpStartBeatOffset;
                if (num2 < 1f)
                {
                    num2 = 1f;
                }

                return num * num2 * 2f;
            }
            return @default;
        }
    }

    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    [HarmonyPatch("SpawnNote")]
    internal class BeatmapObjectSpawnControllerSpawnNote
    {
        private static readonly MethodInfo jumpDuration = SymbolExtensions.GetMethodInfo(() => GetJumpDuration(null, 0));
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundJumpDuration = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundJumpDuration &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_jumpDuration")
                {
                    foundJumpDuration = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, jumpDuration));
                    instructionList.Insert(i - 2, new CodeInstruction(OpCodes.Ldarg_1));
                }
            }
            if (!foundJumpDuration) Logger.Log("Failed to find get_jumpDuration call, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static float GetJumpDuration(NoteData obstacleData, float @default)
        {
            if (obstacleData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                float? _njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? _spawnoffset = (float?)Trees.at(dynData, SPAWNOFFSET);

                float _localNoteJumpMovementSpeed = _njs ?? _noteJumpMovementSpeed;
                float _localNoteJumpStartBeatOffset = _spawnoffset ?? _noteJumpStartBeatOffset;
                float num = 60f / _startBPM;
                float num2 = _startHalfJumpDurationInBeats;
                while (_localNoteJumpMovementSpeed * num * num2 > _maxHalfJumpDistance)
                {
                    num2 /= 2f;
                }
                num2 += _localNoteJumpStartBeatOffset;
                if (num2 < 1f)
                {
                    num2 = 1f;
                }

                return num * num2 * 2f;
            }
            return @default;
        }
    }
}
