namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;
    using static NoodleExtensions.Plugin;

    [NoodlePatch(typeof(BeatmapObjectSpawnController))]
    [NoodlePatch("SpawnObstacle")]
    [NoodlePatch("SpawnNote")]
    internal static class BeatmapObjectSpawnControllerSpawnObject
    {
        private static readonly MethodInfo _getJumpDuration = SymbolExtensions.GetMethodInfo(() => GetJumpDuration(null, 0));

        internal static float GetJumpDuration(BeatmapObjectData beatmapObjectData, float @default)
        {
            if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData)
            {
                dynamic dynData = ((dynamic)beatmapObjectData).customData;
                float? njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? spawnoffset = (float?)Trees.at(dynData, NOTESPAWNOFFSET);
                SpawnDataHelper.GetNoteJumpValues(njs, spawnoffset, out float localJumpDuration, out float _, out Vector3 _, out Vector3 _, out Vector3 _);
                return localJumpDuration;
            }

            return @default;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundJumpDuration = false;
            int instructionListCount = instructionList.Count;
            for (int i = 0; i < instructionListCount; i++)
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

            if (!foundJumpDuration)
            {
                NoodleLogger.Log("Failed to find get_jumpDuration call!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }
}
