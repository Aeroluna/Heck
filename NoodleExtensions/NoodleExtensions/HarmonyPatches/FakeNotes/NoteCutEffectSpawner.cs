namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using UnityEngine;

    internal static class SpawnFlyingSpriteHelper
    {
        internal static readonly MethodInfo _conditionedSpawnFlyingSprite = SymbolExtensions.GetMethodInfo(() => ConditionedSpawnFlyingSprite(null, Vector3.zero, Quaternion.identity, Quaternion.identity, null));
        internal static readonly MethodInfo _conditionedSpawnFlyingScore = SymbolExtensions.GetMethodInfo(() => ConditionedSpawnFlyingScore(null, null, 0, 0, Vector3.zero, Quaternion.identity, Quaternion.identity, Color.clear, null));

        private static void ConditionedSpawnFlyingSprite(FlyingSpriteSpawner flyingSpriteSpawner, Vector3 pos, Quaternion rotation, Quaternion inverseRotation, INoteController noteController)
        {
            if (FakeNoteHelper.GetFakeNote(noteController))
            {
                flyingSpriteSpawner.SpawnFlyingSprite(pos, rotation, inverseRotation);
            }
        }

        private static void ConditionedSpawnFlyingScore(
            FlyingScoreSpawner flyingScoreSpawner,
            NoteCutInfo noteCutInfo,
            int noteLineIndex,
            int multiplier,
            Vector3 pos,
            Quaternion rotation,
            Quaternion inverseRotation,
            Color color,
            INoteController noteController)
        {
            if (FakeNoteHelper.GetFakeNote(noteController))
            {
                flyingScoreSpawner.SpawnFlyingScore(noteCutInfo, noteLineIndex, multiplier, pos, rotation, inverseRotation, color);
            }
        }
    }

    [NoodlePatch(typeof(NoteCutEffectSpawner))]
    [NoodlePatch("SpawnNoteCutEffect")]
    internal static class NoteCutEffectSpawnerSpawnNoteCutEffect
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundSpawnFlyingSprite = false;
            bool foundSpawnFlyingScore = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundSpawnFlyingSprite &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "SpawnFlyingSprite")
                {
                    foundSpawnFlyingSprite = true;

                    instructionList[i] = new CodeInstruction(OpCodes.Call, SpawnFlyingSpriteHelper._conditionedSpawnFlyingSprite);
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_2));
                }

                if (!foundSpawnFlyingScore &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "SpawnFlyingScore")
                {
                    foundSpawnFlyingScore = true;

                    instructionList[i] = new CodeInstruction(OpCodes.Call, SpawnFlyingSpriteHelper._conditionedSpawnFlyingScore);
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_2));
                }
            }

            if (!foundSpawnFlyingSprite)
            {
                NoodleLogger.Log("Failed to find call to SpawnFlyingSprite", IPA.Logging.Logger.Level.Error);
            }

            if (!foundSpawnFlyingScore)
            {
                NoodleLogger.Log("Failed to find call to SpawnFlyingScore", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }

    [NoodlePatch(typeof(NoteCutEffectSpawner))]
    [NoodlePatch("SpawnBombCutEffect")]
    internal static class NoteCutEffectSpawnerSpawnBombCutEffect
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundSpawnFlyingSprite = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundSpawnFlyingSprite &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "SpawnFlyingSprite")
                {
                    foundSpawnFlyingSprite = true;

                    instructionList[i] = new CodeInstruction(OpCodes.Call, SpawnFlyingSpriteHelper._conditionedSpawnFlyingSprite);
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_2));
                }
            }

            if (!foundSpawnFlyingSprite)
            {
                NoodleLogger.Log("Failed to find call to SpawnFlyingSprite", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }
}
