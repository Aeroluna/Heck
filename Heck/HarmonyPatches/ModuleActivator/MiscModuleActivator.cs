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
    [AffinityPatch(
        typeof(MultiplayerLevelScenesTransitionSetupDataSO),
        nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
    private void MultiplayerPrefix(in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel)
    {
        OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
        _moduleManager.Activate(beatmapKey, beatmapLevel, LevelType.Multiplayer, ref overrideEnvironmentSettings);
    }

    [AffinityPrefix]
    [AffinityPatch(
        typeof(TutorialScenesTransitionSetupDataSO),
        nameof(TutorialScenesTransitionSetupDataSO.Init))]
    private void TutorialPrefix()
    {
        OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
        _moduleManager.Activate(default, null, LevelType.Tutorial, ref overrideEnvironmentSettings);
    }
}
#endif
