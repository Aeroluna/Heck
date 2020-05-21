using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(BeatmapObjectSpawnController))]
    [NoodlePatch("SpawnObstacle")]
    [NoodlePatch("SpawnNote")]
    internal class BeatmapObjectSpawnControllerSpawnObject
    {
        private static readonly MethodInfo _getJumpDuration = SymbolExtensions.GetMethodInfo(() => GetJumpDuration(null, 0));

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

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _getJumpDuration));
                    instructionList.Insert(i - 2, new CodeInstruction(OpCodes.Ldarg_1));
                }
            }
            if (!foundJumpDuration) Logger.Log("Failed to find get_jumpDuration call, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        internal static float GetJumpDuration(BeatmapObjectData beatmapObjectData, float @default)
        {
            if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData)
            {
                dynamic dynData = ((dynamic)beatmapObjectData).customData;
                float? njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? spawnoffset = (float?)Trees.at(dynData, SPAWNOFFSET);
                SpawnDataHelper.GetNoteJumpValues(njs, spawnoffset, out float localJumpDuration, out float _, out Vector3 _, out Vector3 _, out Vector3 _);
                return localJumpDuration;
            }
            return @default;
        }
    }
}