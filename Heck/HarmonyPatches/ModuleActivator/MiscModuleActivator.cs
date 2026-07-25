#if !PRE_V1_37_1
using Heck.Module;
using SiraUtil.Affinity;

namespace Heck.HarmonyPatches.ModuleActivator;

internal class MiscModuleActivator : IAffinity
{
    private readonly ModuleManager _moduleManager;

    internal MiscModuleActivator(ModuleManager moduleManager)
    {
        _moduleManager = moduleManager;
    }

    [AffinityPrefix]
#if LATEST
    [AffinityPatch(
        typeof(MultiplayerLevelScenesTransitionSetupData),
        nameof(MultiplayerLevelScenesTransitionSetupData.Init))]
#else
    [AffinityPatch(
        typeof(MultiplayerLevelScenesTransitionSetupDataSO),
        nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
#endif
    private void MultiplayerPrefix(in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel)
    {
        OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
        _moduleManager.Activate(beatmapKey, beatmapLevel, LevelType.Multiplayer, ref overrideEnvironmentSettings);
    }

    [AffinityPrefix]
#if LATEST
    [AffinityPatch(
        typeof(TutorialScenesTransitionSetupData),
        nameof(TutorialScenesTransitionSetupData.Init))]
#else
    [AffinityPatch(
        typeof(TutorialScenesTransitionSetupDataSO),
        nameof(TutorialScenesTransitionSetupDataSO.Init))]
#endif
    private void TutorialPrefix()
    {
        OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
        _moduleManager.Activate(default, null, LevelType.Tutorial, ref overrideEnvironmentSettings);
    }
}
#endif
