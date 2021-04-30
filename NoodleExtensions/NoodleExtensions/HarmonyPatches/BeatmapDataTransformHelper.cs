namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using Heck;
    using IPA.Utilities;
    using static NoodleExtensions.Plugin;

    [HeckPatch(typeof(BeatmapDataTransformHelper))]
    [HeckPatch("CreateTransformedBeatmapData")]
    internal static class BeatmapDataTransformHelperCreateTransformedBeatmapData
    {
        private static readonly MethodInfo _reorderLineData = SymbolExtensions.GetMethodInfo(() => ReorderLineData(null));

        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundBeatmapData = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundBeatmapData &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "GetCopy")
                {
                    foundBeatmapData = true;

                    // yoink label5 so we can insert our code w/o breaking shit
                    CodeInstruction sourceLabel = instructionList[i - 4];
                    CodeInstruction newLabel = new CodeInstruction(instructionList[i - 4]);
                    sourceLabel.labels.Clear();

                    instructionList.Insert(i - 4, newLabel);
                    instructionList.Insert(i - 3, new CodeInstruction(OpCodes.Call, _reorderLineData));
                    instructionList.Insert(i - 2, new CodeInstruction(OpCodes.Stloc_0));
                }
            }

            if (!foundBeatmapData)
            {
                Logger.Log("Failed to find GetCopy!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static IReadonlyBeatmapData ReorderLineData(IReadonlyBeatmapData beatmapData)
        {
            if (beatmapData is CustomBeatmapData)
            {
                CustomBeatmapData customBeatmapData = (CustomBeatmapData)beatmapData.GetCopy();

                // there is some ambiguity with these variables but who frikkin cares
                float startHalfJumpDurationInBeats = 4;
                float maxHalfJumpDistance = 18;
                float moveDuration = 0.5f;

                for (int i = 0; i < customBeatmapData.beatmapLinesData.Count; i++)
                {
                    BeatmapLineData beatmapLineData = customBeatmapData.beatmapLinesData[i] as BeatmapLineData;
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        dynamic customData;
                        if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData || beatmapObjectData is CustomWaypointData)
                        {
                            customData = beatmapObjectData;
                        }
                        else
                        {
                            Logger.Log("beatmapObjectData was not CustomObstacleData, CustomNoteData, or CustomWaypointData");
                            continue;
                        }

                        dynamic dynData = customData.customData;
                        float noteJumpMovementSpeed = (float?)Trees.at(dynData, NOTEJUMPSPEED) ?? GameplayCoreInstallerInstallBindings.CachedNoteJumpMovementSpeed;
                        float noteJumpStartBeatOffset = (float?)Trees.at(dynData, NOTESPAWNOFFSET) ?? GameplayCoreInstallerInstallBindings.CachedNoteJumpStartBeatOffset;

                        // how do i not repeat this in a reasonable way
                        float num = 60f / (float)Trees.at(dynData, "bpm");
                        float num2 = startHalfJumpDurationInBeats;
                        while (noteJumpMovementSpeed * num * num2 > maxHalfJumpDistance)
                        {
                            num2 /= 2f;
                        }

                        num2 += noteJumpStartBeatOffset;
                        if (num2 < 1f)
                        {
                            num2 = 1f;
                        }

                        float jumpDuration = num * num2 * 2f;
                        dynData.aheadTime = moveDuration + (jumpDuration * 0.5f);
                    }

                    _beatmapObjectsDataAccessor(ref beatmapLineData) = beatmapLineData.beatmapObjectsData.OrderBy(n => n.time - (float)((dynamic)n).customData.aheadTime).ToList();
                }

                return customBeatmapData;
            }

            Logger.Log("beatmapData was not CustomBeatmapData", IPA.Logging.Logger.Level.Error);
            return beatmapData;
        }

        private static void Postfix(IReadonlyBeatmapData __result)
        {
            // Skip if calling class is MultiplayerConnectPlayerInstaller
            StackTrace stackTrace = new StackTrace();
            if (!stackTrace.GetFrame(2).GetMethod().Name.Contains("MultiplayerConnectedPlayerInstaller"))
            {
                NoodleObjectDataManager.DeserializeBeatmapData(__result);
                Animation.NoodleEventDataManager.DeserializeBeatmapData(__result);
            }
        }
    }
}
