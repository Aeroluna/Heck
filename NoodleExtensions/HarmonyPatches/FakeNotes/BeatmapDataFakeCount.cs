using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using NoodleExtensions.Extras;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch]
    [HarmonyPatch(typeof(BeatmapData))]
    internal static class BeatmapDataFakeCount
    {
        private static readonly List<object> _countGetters = new()
        {
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.obstaclesCount)),
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.cuttableNotesCount)),
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.bombsCount))
        };

        private static readonly MethodInfo _fakeObjectCheck = AccessTools.Method(typeof(BeatmapDataFakeCount), nameof(FakeObjectCheck));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapData.AddBeatmapObjectData))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call) { operands = _countGetters })
                .Repeat(n => n
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_0))
                    .Advance(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, _fakeObjectCheck))
                    .RemoveInstructions(5))
                .InstructionEnumeration();
        }

        private static int FakeObjectCheck(BeatmapData beatmapData, int objectCount, BeatmapObjectData beatmapObjectData)
        {
            if (beatmapData is not CustomBeatmapData customBeatmapData)
            {
                return objectCount + 1;
            }

            Dictionary<string, object?> dictionary = customBeatmapData.beatmapCustomData;
            bool? noodleRequirement = dictionary.Get<bool?>("noodleRequirement");
            if (!noodleRequirement.HasValue)
            {
                noodleRequirement = dictionary.Get<List<object>>("_requirements")?.Cast<string>().Contains(CAPABILITY) ?? false;
                dictionary["noodleRequirement"] = noodleRequirement;
            }

            if (!noodleRequirement.Value)
            {
                return objectCount + 1;
            }

            bool? fake = beatmapObjectData.GetDataForObject().Get<bool?>(FAKE_NOTE);
            if (fake is true)
            {
                return objectCount;
            }

            return objectCount + 1;
        }
    }
}
