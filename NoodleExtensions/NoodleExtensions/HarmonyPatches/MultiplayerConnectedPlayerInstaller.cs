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
        private static readonly MethodInfo _excludeFakeNote = SymbolExtensions.GetMethodInfo(() => ExcludeFakeNoteAndAllWalls(null));

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
                Logger.Log("Failed to find Call to CreateTransformedBeatmapData!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static IReadonlyBeatmapData ExcludeFakeNoteAndAllWalls(IReadonlyBeatmapData result)
        {
            foreach (BeatmapLineData b in result.beatmapLinesData)
            {
                BeatmapLineData refBeatmapLineData = b;
                _beatmapObjectsDataAccessor(ref refBeatmapLineData) = b.beatmapObjectsData.Where(n =>
                {
                    Dictionary<string, object> dynData;

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
