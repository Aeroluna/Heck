using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller))]
    internal static class ExcludeFakeOnMultiplayerConnected
    {
        private static readonly MethodInfo _createTransformedBeatmapData = AccessTools.Method(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData));

        private static readonly MethodInfo _excludeFakeNote = AccessTools.Method(typeof(ExcludeFakeOnMultiplayerConnected), nameof(ExcludeFakeNoteAndAllWalls));

        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _createTransformedBeatmapData))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Call, _excludeFakeNote))
                .InstructionEnumeration();
        }

        private static IReadonlyBeatmapData ExcludeFakeNoteAndAllWalls(IReadonlyBeatmapData result)
        {
            foreach (IReadonlyBeatmapLineData readonlyBeatmapLineData in result.beatmapLinesData)
            {
                BeatmapLineData beatmapLineData = (BeatmapLineData)readonlyBeatmapLineData;
                _beatmapObjectsDataAccessor(ref beatmapLineData) = beatmapLineData.beatmapObjectsData.Where(n =>
                {
                    Dictionary<string, object?> dynData;

                    switch (n)
                    {
                        case CustomNoteData customNoteData:
                            dynData = customNoteData.customData;
                            break;

                        case CustomObstacleData:
                            return false;

                        default:
                            return true;
                    }

                    bool? fake = dynData.Get<bool?>(FAKE_NOTE);
                    return fake is not true;
                }).ToList();
            }

            return result;
        }
    }
}
