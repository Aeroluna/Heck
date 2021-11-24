using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;
using static NoodleExtensions.NoodleCustomDataManager;

namespace NoodleExtensions.HarmonyPatches
{
    [HeckPatch(typeof(BeatmapObjectCallbackController))]
    [HeckPatch("LateUpdate")]
    internal static class BeatmapObjectCallBackControllerLateUpdate
    {
        private static readonly FieldInfo _aheadTimeField = AccessTools.Field(typeof(BeatmapObjectCallbackData), nameof(BeatmapObjectCallbackData.aheadTime));

        private static readonly MethodInfo _getAheadTime = AccessTools.Method(typeof(BeatmapObjectCallBackControllerLateUpdate), nameof(GetAheadTime));
        private static readonly MethodInfo _beatmapObjectSpawnControllerCallback = AccessTools.Method(typeof(BeatmapObjectSpawnController), nameof(BeatmapObjectSpawnController.HandleBeatmapObjectCallback));

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _aheadTimeField))
                .Advance(-1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_3))
                .Advance(2)
                .Insert(new CodeInstruction(OpCodes.Call, _getAheadTime))
                .InstructionEnumeration();
        }

        private static float GetAheadTime(BeatmapObjectCallbackData beatmapObjectCallbackData, BeatmapObjectData beatmapObjectData, float @default)
        {
            if (beatmapObjectCallbackData.callback.Method != _beatmapObjectSpawnControllerCallback ||
                (beatmapObjectData is not CustomObstacleData && beatmapObjectData is not CustomNoteData))
            {
                return @default;
            }

            NoodleObjectData? noodleData = TryGetObjectData<NoodleObjectData>(beatmapObjectData);
            float? aheadTime = noodleData?.AheadTimeInternal;
            return aheadTime ?? @default;
        }
    }
}
