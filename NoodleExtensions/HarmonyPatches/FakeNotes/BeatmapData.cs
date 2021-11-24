using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using JetBrains.Annotations;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new[] { typeof(int) })]
    internal static class BeatmapDataCtor
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            BeatmapDataAddBeatmapObjectData.NeedsCheck = true;
        }
    }

    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch("AddBeatmapObjectData")]
    internal static class BeatmapDataAddBeatmapObjectData
    {
        private static readonly List<object> _countGetters = new()
        {
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.obstaclesCount)),
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.cuttableNotesCount)),
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.bombsCount))
        };

        private static readonly MethodInfo _fakeObjectCheck = AccessTools.Method(typeof(BeatmapDataAddBeatmapObjectData), nameof(FakeObjectCheck));
        private static bool _noodleRequirement;

        internal static bool NeedsCheck { get; set; }

        [UsedImplicitly]
        private static void Prefix(BeatmapData __instance)
        {
            if (!NeedsCheck)
            {
                return;
            }

            NeedsCheck = false;
            if (__instance is CustomBeatmapData customBeatmapData)
            {
                IEnumerable<string>? requirements = customBeatmapData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>();
                _noodleRequirement = requirements?.Contains(CAPABILITY) ?? false;
            }
            else
            {
                _noodleRequirement = false;
            }
        }

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call) { operands = _countGetters })
                .Repeat(n => n
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                    .Advance(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, _fakeObjectCheck))
                    .RemoveInstructions(5))
                .InstructionEnumeration();
        }

        private static int FakeObjectCheck(int objectCount, BeatmapObjectData beatmapObjectData)
        {
            if (!_noodleRequirement)
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
