using SiraUtil.Affinity;

namespace Heck.HarmonyPatches
{
    internal class SceneTransitionModuleActivator : IAffinity
    {
        private readonly ModuleManager _moduleManager;

        internal SceneTransitionModuleActivator(ModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(MissionLevelScenesTransitionSetupDataSO),
            nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
        private void MissionPrefix(IDifficultyBeatmap difficultyBeatmap, IPreviewBeatmapLevel previewBeatmapLevel)
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            _moduleManager.Activate(difficultyBeatmap, previewBeatmapLevel, LevelType.Mission, ref overrideEnvironmentSettings);
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(MultiplayerLevelScenesTransitionSetupDataSO),
            nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
        private void MultiplayerPrefix(IDifficultyBeatmap difficultyBeatmap, IPreviewBeatmapLevel previewBeatmapLevel)
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            _moduleManager.Activate(difficultyBeatmap, previewBeatmapLevel, LevelType.Multiplayer, ref overrideEnvironmentSettings);
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(StandardLevelScenesTransitionSetupDataSO),
            nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
        private void StandardPrefix(
            IDifficultyBeatmap difficultyBeatmap,
            IPreviewBeatmapLevel previewBeatmapLevel,
            ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
        {
            _moduleManager.Activate(difficultyBeatmap, previewBeatmapLevel, LevelType.Standard, ref overrideEnvironmentSettings);
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(TutorialScenesTransitionSetupDataSO),
            nameof(TutorialScenesTransitionSetupDataSO.Init))]
        private void TutorialPrefix()
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            _moduleManager.Activate(null, null, LevelType.Standard, ref overrideEnvironmentSettings);
        }
    }
}
