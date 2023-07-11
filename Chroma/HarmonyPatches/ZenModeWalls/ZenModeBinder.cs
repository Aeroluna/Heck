using Chroma.Settings;
using IPA.Utilities;
using SiraUtil.Affinity;
using Zenject;

namespace Chroma.HarmonyPatches.ZenModeWalls
{
    internal class ZenModeBinder : IAffinity
    {
        private readonly Config _config;

        private ZenModeBinder(Config config)
        {
            _config = config;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
        private void Postfix(GameplayCoreInstaller __instance, GameplayCoreSceneSetupData ____sceneSetupData)
        {
            if (!_config.ForceZenWallsEnabled)
            {
                return;
            }

            MonoInstallerBase installerBase = __instance;
            DiContainer container = installerBase.Container;
            container.Bind<bool>().WithId("zenMode").FromInstance(____sceneSetupData.gameplayModifiers.zenMode);
        }
    }
}
