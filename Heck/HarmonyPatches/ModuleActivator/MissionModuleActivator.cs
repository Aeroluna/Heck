#if LATEST
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Heck.Module;
using JetBrains.Annotations;
using Zenject;

namespace Heck.HarmonyPatches.ModuleActivator;

[HeckPatch]
internal class MissionModuleActivator : IInitializable
{
    private static MissionModuleActivator _instance = null!;

    private readonly ModuleManager _moduleManager;

    [UsedImplicitly]
    internal MissionModuleActivator(ModuleManager moduleManager)
    {
        _moduleManager = moduleManager;
    }

    public void Initialize()
    {
        _instance = this;
    }

    [UsedImplicitly]
    [HarmonyPrefix]
    private static void MissionPrefix(in BeatmapKey beatmapKey, BeatmapLevel beatmapLevel)
    {
        OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
        _instance._moduleManager.Activate(beatmapKey, beatmapLevel, LevelType.Mission, ref overrideEnvironmentSettings);
    }

    [UsedImplicitly]
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(MissionLevelScenesTransitionSetupDataSO)
            .GetMethods()
            .Where(n => n.Name == nameof(MissionLevelScenesTransitionSetupDataSO.Init));
    }
}
#endif
