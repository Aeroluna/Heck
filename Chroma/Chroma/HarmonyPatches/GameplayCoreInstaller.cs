namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(GameplayCoreInstaller))]
    [HarmonyPatch("InstallBindings")]
    internal static class GameplayCoreInstallerInstallBindings
    {
        internal static bool ZenModeActive { get; private set; }

        private static void Postfix(GameplayCoreSceneSetupData ____sceneSetupData)
        {
            ZenModeActive = ____sceneSetupData.gameplayModifiers.zenMode;
        }
    }
}
