namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using static NoodleExtensions.NoodleObjectDataManager;

    [NoodlePatch(typeof(BeatmapObjectManager))]
    [NoodlePatch("HandleNoteControllerNoteWasCut")]
    internal static class BeatmapObjectManagerHandleNoteWasCut
    {
        private static readonly MethodInfo _despawnMethod = typeof(BeatmapObjectManager).GetMethod("Despawn", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(NoteController) }, null);

        [HarmonyPriority(Priority.High)]
        private static bool Prefix(BeatmapObjectManager __instance, NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (!(noteController is MultiplayerConnectedPlayerNoteController) && !FakeNoteHelper.GetFakeNote(noteController))
            {
                NoteCutCoreEffectsSpawnerStart.NoteCutCoreEffectsSpawner.HandleNoteWasCut(noteController, noteCutInfo);
                _despawnMethod.Invoke(__instance, new object[] { noteController });

                return false;
            }

            return true;
        }
    }

    [NoodlePatch(typeof(BeatmapObjectManager))]
    [NoodlePatch("HandleNoteControllerNoteWasMissed")]
    internal static class BeatmapObjectManagerHandleNoteWasMissed
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            if (!(noteController is MultiplayerConnectedPlayerNoteController))
            {
                return FakeNoteHelper.GetFakeNote(noteController);
            }

            return true;
        }
    }

    // TODO: find out what actually causes obstacle flickering
    [NoodlePatch(typeof(BeatmapObjectManager))]
    [NoodlePatch("SpawnObstacle")]
    internal static class BeatmapObjectManagerSpawnObstacle
    {
        private static readonly MethodInfo _getHiddenForType = SymbolExtensions.GetMethodInfo(() => GetHiddenForType(null));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundHide = false;
            int instructrionListCount = instructionList.Count;
            for (int i = 0; i < instructrionListCount; i++)
            {
                if (!foundHide &&
                       instructionList[i].opcode == OpCodes.Call &&
                       ((MethodInfo)instructionList[i].operand).Name == "get_spawnHidden")
                {
                    foundHide = true;

                    instructionList[i].operand = _getHiddenForType;
                }
            }

            if (!foundHide)
            {
                NoodleLogger.Log("Failed to find call to get_spawnHidden!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static void Postfix(ObstacleController __result)
        {
            if (__result is MultiplayerConnectedPlayerObstacleController)
            {
                return;
            }

            NoodleObstacleData noodleData = (NoodleObstacleData)NoodleObjectDatas[__result.obstacleData];

            noodleData.DoUnhide = true;
        }

        private static bool GetHiddenForType(BeatmapObjectManager beatmapObjectManager)
        {
            if (beatmapObjectManager is BasicBeatmapObjectManager)
            {
                return true;
            }

            return beatmapObjectManager.spawnHidden;
        }
    }
}
