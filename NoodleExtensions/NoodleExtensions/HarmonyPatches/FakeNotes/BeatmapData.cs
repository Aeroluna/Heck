namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(int) })]
    internal static class BeatmapDataCtor
    {
        private static void Postfix()
        {
            BeatmapDataAddBeatmapObjectData.NeedsCheck = true;
        }
    }

    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch("AddBeatmapObjectData")]
    internal static class BeatmapDataAddBeatmapObjectData
    {
        private static readonly List<object> _countGetters = new List<object>
        {
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.obstaclesCount)),
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.cuttableNotesCount)),
            AccessTools.PropertyGetter(typeof(BeatmapData), nameof(BeatmapData.bombsCount)),
        };

        private static readonly MethodInfo _fakeObjectCheck = AccessTools.Method(typeof(BeatmapDataAddBeatmapObjectData), nameof(FakeObjectCheck));
        private static bool _noodleRequirement;

        internal static bool NeedsCheck { get; set; }

        private static void Prefix(BeatmapData __instance)
        {
            if (NeedsCheck)
            {
                NeedsCheck = false;
                if (__instance is CustomBeatmapData customBeatmapData)
                {
                    IEnumerable<string>? requirements = customBeatmapData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>();
                    _noodleRequirement = requirements?.Contains(Plugin.CAPABILITY) ?? false;
                }
                else
                {
                    _noodleRequirement = false;
                }
            }
        }

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
            if (_noodleRequirement)
            {
                bool? fake = beatmapObjectData.GetDataForObject().Get<bool?>(Plugin.FAKENOTE);
                if (fake.HasValue && fake.Value)
                {
                    return objectCount;
                }
            }

            return objectCount + 1;
        }
    }
}
