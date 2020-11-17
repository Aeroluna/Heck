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

    [NoodlePatch(typeof(BeatmapDataTransformHelper))]
    [NoodlePatch("CreateTransformedBeatmapData")]
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
                    instructionList[i].opcode == OpCodes.Stloc_0)
                {
                    foundBeatmapData = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_0));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Call, _reorderLineData));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Stloc_0));
                }
            }

            if (!foundBeatmapData)
            {
                NoodleLogger.Log("Failed to find stloc.0!", IPA.Logging.Logger.Level.Error);
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
                            NoodleLogger.Log("beatmapObjectData was not CustomObstacleData, CustomNoteData, or CustomWaypointData");
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

            NoodleLogger.Log("beatmapData was not CustomBeatmapData", IPA.Logging.Logger.Level.Error);
            return beatmapData;
        }
    }
}
