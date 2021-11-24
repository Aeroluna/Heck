using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayCoreInstaller))]
    [HarmonyPatch("InstallBindings")]
    internal static class GameplayCoreInstallerInstallBindings
    {
        internal static bool ZenModeActive { get; private set; }

        [UsedImplicitly]
        private static void Postfix(GameplayCoreSceneSetupData ____sceneSetupData)
        {
            ZenModeActive = ____sceneSetupData.gameplayModifiers.zenMode;
        }
    }
}
