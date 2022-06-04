using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    [HeckPatch(PatchType.Features)]
    internal class NoodleBeatmapObjectsInTimeRowProcessor : BeatmapObjectsInTimeRowProcessor
    {
        private static readonly ConstructorInfo _original = AccessTools.FirstConstructor(typeof(BeatmapObjectsInTimeRowProcessor), _ => true);
        private static readonly ConstructorInfo _noodle = AccessTools.FirstConstructor(typeof(NoodleBeatmapObjectsInTimeRowProcessor), _ => true);

        internal NoodleBeatmapObjectsInTimeRowProcessor(int numberOfLines, BeatmapData beatmapData)
            : base(numberOfLines)
        {
            BeatmapData = beatmapData;
        }

        internal BeatmapData BeatmapData { get; }

        internal static bool GetV2(BeatmapObjectsInTimeRowProcessor processor)
        {
            return processor is NoodleBeatmapObjectsInTimeRowProcessor { BeatmapData: CustomBeatmapData { version2_6_0AndEarlier: true } };
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BeatmapData), MethodType.Constructor, typeof(int))]
        private static IEnumerable<CodeInstruction> Replace(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _original))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .SetOperandAndAdvance(_noodle)
                .InstructionEnumeration();
        }
    }
}
