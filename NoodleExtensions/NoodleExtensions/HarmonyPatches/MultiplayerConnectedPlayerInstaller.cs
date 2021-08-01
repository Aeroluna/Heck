namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using Heck;
    using IPA.Utilities;
    using static NoodleExtensions.Plugin;

    [HeckPatch(typeof(MultiplayerConnectedPlayerInstaller))]
    [HeckPatch("InstallBindings")]
    internal static class MultiplayerConnectedPlayerInstallerInstallBindings
    {
        private static readonly MethodInfo _createTransformedBeatmapData = AccessTools.Method(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData));

        private static readonly MethodInfo _excludeFakeNote = AccessTools.Method(typeof(MultiplayerConnectedPlayerInstallerInstallBindings), nameof(ExcludeFakeNoteAndAllWalls));

        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");

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
            foreach (BeatmapLineData b in result.beatmapLinesData)
            {
                BeatmapLineData refBeatmapLineData = b;
                _beatmapObjectsDataAccessor(ref refBeatmapLineData) = b.beatmapObjectsData.Where(n =>
                {
                    Dictionary<string, object?> dynData;

                    switch (n)
                    {
                        case CustomNoteData customNoteData:
                            dynData = customNoteData.customData;
                            break;

                        case CustomObstacleData customObstacleData:
                            return false;

                        default:
                            return true;
                    }

                    bool? fake = dynData.Get<bool?>(FAKENOTE);
                    if (fake.HasValue && fake.Value)
                    {
                        return false;
                    }

                    return true;
                }).ToList();
            }

            if (result is CustomBeatmapData customBeatmapData)
            {
                string[] excludedTypes = new string[]
                {
                    ASSIGNPLAYERTOTRACK,
                    ASSIGNTRACKPARENT,
                };

                customBeatmapData.customEventsData.RemoveAll(n => excludedTypes.Contains(n.type));
            }

            return result;
        }
    }
}
