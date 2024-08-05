#if !LATEST
using Heck.Module;
using SiraUtil.Affinity;

namespace Heck.HarmonyPatches.ModuleActivator;

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
    private void MissionPrefix(IDifficultyBeatmap difficultyBeatmap)
    {
        OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
        _moduleManager.Activate(difficultyBeatmap, LevelType.Mission, ref overrideEnvironmentSettings);
    }

    [AffinityPrefix]
    [AffinityPatch(
        typeof(MultiplayerLevelScenesTransitionSetupDataSO),
        nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
    private void MultiplayerPrefix(IDifficultyBeatmap difficultyBeatmap)
    {
        OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
        _moduleManager.Activate(difficultyBeatmap, LevelType.Multiplayer, ref overrideEnvironmentSettings);
    }

    [AffinityPrefix]
    [AffinityPatch(
        typeof(StandardLevelScenesTransitionSetupDataSO),
        nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
    private void StandardPrefix(
        IDifficultyBeatmap difficultyBeatmap,
        ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
    {
        _moduleManager.Activate(difficultyBeatmap, LevelType.Standard, ref overrideEnvironmentSettings);
    }

    [AffinityPrefix]
    [AffinityPatch(
        typeof(TutorialScenesTransitionSetupDataSO),
        nameof(TutorialScenesTransitionSetupDataSO.Init))]
    private void TutorialPrefix()
    {
        OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
        _moduleManager.Activate(null, LevelType.Standard, ref overrideEnvironmentSettings);
    }
}
#endif
