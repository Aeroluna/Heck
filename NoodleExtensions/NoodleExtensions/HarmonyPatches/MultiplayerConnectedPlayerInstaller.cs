namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using static NoodleExtensions.Plugin;

    [NoodlePatch(typeof(MultiplayerConnectedPlayerInstaller))]
    [NoodlePatch("InstallBindings")]
    internal static class MultiplayerConnectedPlayerInstallerInstallBindings
    {
        private static readonly MethodInfo _excludeFakeNote = SymbolExtensions.GetMethodInfo(() => ExcludeFakeNote(null));

        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundBeatmapData = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundBeatmapData &&
                    instructionList[i].opcode == OpCodes.Call &&
                    ((MethodInfo)instructionList[i].operand).Name == "CreateTransformedBeatmapData")
                {
                    foundBeatmapData = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _excludeFakeNote));
                }
            }

            if (!foundBeatmapData)
            {
                NoodleLogger.Log("Failed to find Call to CreateTransformedBeatmapData!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static IReadonlyBeatmapData ExcludeFakeNote(IReadonlyBeatmapData result)
        {
            foreach (BeatmapLineData b in result.beatmapLinesData)
            {
                BeatmapLineData refBeatmapLineData = b;
                _beatmapObjectsDataAccessor(ref refBeatmapLineData) = b.beatmapObjectsData.Where(n =>
                {
                    dynamic dynData = null;

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

                    bool? fake = Trees.at(dynData, FAKENOTE);
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
                    ANIMATETRACK,
                    ASSIGNPATHANIMATION,
                    ASSIGNPLAYERTOTRACK,
                    ASSIGNTRACKPARENT,
                };

                customBeatmapData.customEventsData.RemoveAll(n => excludedTypes.Contains(n.type));

                customBeatmapData.customData.isMultiplayer = true;
            }

            return result;
        }
    }
}
