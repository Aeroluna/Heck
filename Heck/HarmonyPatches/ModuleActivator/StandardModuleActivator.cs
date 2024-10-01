#if !PRE_V1_37_1
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Heck.Module;
using JetBrains.Annotations;
using Zenject;

namespace Heck.HarmonyPatches.ModuleActivator;

[HeckPatch]
internal class StandardModuleActivator : IInitializable
{
    // i wish auros didnt quit before adding affinity targetmethods
    private static StandardModuleActivator _instance = null!;

    private readonly ModuleManager _moduleManager;

    [UsedImplicitly]
    internal StandardModuleActivator(ModuleManager moduleManager)
    {
        _moduleManager = moduleManager;
    }

    public void Initialize()
    {
        _instance = this;
    }

    [UsedImplicitly]
    [HarmonyPrefix]
    private static void StandardPrefix(
        in BeatmapKey beatmapKey,
        BeatmapLevel beatmapLevel,
        ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
    {
        _instance._moduleManager.Activate(beatmapKey, beatmapLevel, LevelType.Standard, ref overrideEnvironmentSettings);
    }

    [UsedImplicitly]
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(StandardLevelScenesTransitionSetupDataSO)
            .GetMethods()
            .Where(n => n.Name == nameof(StandardLevelScenesTransitionSetupDataSO.Init));
    }
}
#endif
