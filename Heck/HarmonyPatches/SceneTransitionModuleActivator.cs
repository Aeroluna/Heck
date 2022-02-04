using HarmonyLib;

namespace Heck.HarmonyPatches
{
    [HeckPatch]
    internal static class SceneTransitionModuleActivator
    {
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(MissionLevelScenesTransitionSetupDataSO),
            nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
        private static void MissionPrefix(IDifficultyBeatmap difficultyBeatmap)
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            ModuleManager.Activate(difficultyBeatmap, LevelType.Mission, ref overrideEnvironmentSettings);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(MultiplayerLevelScenesTransitionSetupDataSO),
            nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
        private static void MultiplayerPrefix(IDifficultyBeatmap difficultyBeatmap)
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            ModuleManager.Activate(difficultyBeatmap, LevelType.Multiplayer, ref overrideEnvironmentSettings);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StandardLevelScenesTransitionSetupDataSO),
            nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
        private static void StandardPrefix(IDifficultyBeatmap difficultyBeatmap, ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
        {
            ModuleManager.Activate(difficultyBeatmap, LevelType.Standard, ref overrideEnvironmentSettings);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(TutorialScenesTransitionSetupDataSO),
            nameof(TutorialScenesTransitionSetupDataSO.Init))]
        private static void TutorialPrefix()
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            ModuleManager.Activate(null, LevelType.Standard, ref overrideEnvironmentSettings);
        }
    }
}
