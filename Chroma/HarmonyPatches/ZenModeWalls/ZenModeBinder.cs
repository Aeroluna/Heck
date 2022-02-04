using Chroma.Settings;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using Zenject;

namespace Chroma.HarmonyPatches.ZenModeWalls
{
    [HeckPatch]
    [HarmonyPatch(typeof(GameplayCoreInstaller))]
    internal static class ZenModeBinder
    {
        private static readonly PropertyAccessor<MonoInstallerBase, DiContainer>.Getter _containerAccessor =
            PropertyAccessor<MonoInstallerBase, DiContainer>.GetGetter("Container");

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameplayCoreInstaller.InstallBindings))]
        private static void Postfix(GameplayCoreInstaller __instance, GameplayCoreSceneSetupData ____sceneSetupData)
        {
            if (!ChromaConfig.Instance.ForceZenWallsEnabled)
            {
                return;
            }

            MonoInstallerBase installerBase = __instance;
            DiContainer container = _containerAccessor(ref installerBase);
            container.Bind<bool>().WithId("zenMode").FromInstance(____sceneSetupData.gameplayModifiers.zenMode);
        }
    }
}
