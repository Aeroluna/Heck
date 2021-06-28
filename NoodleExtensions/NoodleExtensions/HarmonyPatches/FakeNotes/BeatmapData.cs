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
            List<CodeInstruction> instructionList = instructions.ToList();

            void ModifyInstructions(int i)
            {
                instructionList.RemoveRange(i + 1, 5);

                CodeInstruction[] codeInstructions = new CodeInstruction[]
                {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, _fakeObjectCheck),
                };
                instructionList.InsertRange(i + 1, codeInstructions);

                instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
            }

            bool foundObstacles = false;
            bool foundNotes = false;
            bool foundBombs = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundObstacles &&
                    instructionList[i].opcode == OpCodes.Call &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_obstaclesCount")
                {
                    foundObstacles = true;

                    ModifyInstructions(i);
                }

                if (!foundNotes &&
                    instructionList[i].opcode == OpCodes.Call &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_cuttableNotesType")
                {
                    foundNotes = true;

                    ModifyInstructions(i);
                }

                if (!foundBombs &&
                    instructionList[i].opcode == OpCodes.Call &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_bombsCount")
                {
                    foundBombs = true;

                    ModifyInstructions(i);
                }
            }

            if (!foundObstacles)
            {
                Plugin.Logger.Log("Failed to find call to get_obstaclesCount!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundNotes)
            {
                Plugin.Logger.Log("Failed to find call to get_cuttableNotesType!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundBombs)
            {
                Plugin.Logger.Log("Failed to find call to get_bombsCount!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
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
