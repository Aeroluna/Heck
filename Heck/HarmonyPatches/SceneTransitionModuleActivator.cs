using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Heck.HarmonyPatches
{
    [HeckPatch]
    internal static class SceneTransitionModuleActivator
    {
        private static readonly MethodInfo _init = AccessTools.Method(typeof(ScenesTransitionSetupDataSO), "Init");
        private static readonly MethodInfo _activateModules = AccessTools.Method(typeof(ModuleManager), nameof(ModuleManager.Activate));

        // ldc.i4.0 = Mission;
        // ldc.i4.1 = Multiplayer;
        // ldc.i4.2 = Standard;
        // ldc.i4.3 = Tutorial
        [HarmonyPatch(
            typeof(MissionLevelScenesTransitionSetupDataSO),
            nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
        private static IEnumerable<CodeInstruction> MissionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_3),
                    new CodeMatch(OpCodes.Ldloc_S, 4), // possibly the wrong number IDK nobody uses campaigns anyways
                    new CodeMatch(OpCodes.Call, _init))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Call, _activateModules))
                .InstructionEnumeration();
        }

        [HarmonyPatch(
            typeof(MultiplayerLevelScenesTransitionSetupDataSO),
            nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
        private static IEnumerable<CodeInstruction> MultiplayerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Call, _init))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldc_I4_2),
                    new CodeInstruction(OpCodes.Call, _activateModules))
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(
            typeof(StandardLevelScenesTransitionSetupDataSO),
            nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
        private static IEnumerable<CodeInstruction> StandardTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_2),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Call, _init))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Call, _activateModules))
                .InstructionEnumeration();
        }

        [HarmonyPatch(
            typeof(TutorialScenesTransitionSetupDataSO),
            nameof(TutorialScenesTransitionSetupDataSO.Init))]
        private static IEnumerable<CodeInstruction> TutorialTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldloc_2),
                    new CodeMatch(OpCodes.Call, _init))
                .Insert(
                    new CodeInstruction(OpCodes.Ldnull),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldc_I4_3),
                    new CodeInstruction(OpCodes.Call, _activateModules))
                .InstructionEnumeration();
        }
    }
}
