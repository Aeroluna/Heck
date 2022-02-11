using BeatSaberMarkupLanguage.GameplaySetup;
using Chroma.Animation;
using Chroma.Extras;
using Chroma.Installers;
using Chroma.Lighting;
using Chroma.Settings;
using Heck;
using Heck.Animation;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using static Chroma.ChromaController;
using Logger = IPA.Logging.Logger;

namespace Chroma
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        [UsedImplicitly]
        [Init]
        public Plugin(Logger pluginLogger, Config conf, Zenjector zenjector)
        {
            Log.Logger = new HeckLogger(pluginLogger);
            ChromaSettableSettings.SetupSettableSettings();
            ChromaConfig.Instance = conf.Generated<ChromaConfig>();
            LightIDTableManager.InitTable();
            zenjector.Install<PlayerInstaller>(Location.Player);
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            TrackBuilder.TrackCreated += AnimationHelper.OnTrackCreated;
            CorePatcher.Enabled = true;
            FeaturesModule.Enabled = true;
            ColorizerModule.Enabled = true;

            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            TrackBuilder.TrackCreated -= AnimationHelper.OnTrackCreated;
            CorePatcher.Enabled = false;
            FeaturesPatcher.Enabled = false;
            FeaturesModule.Enabled = false;
            ColorizerModule.Enabled = false;
            Deserializer.Enabled = false;

            GameplaySetup.instance.RemoveTab("Chroma");
            ChromaUtils.SetSongCoreCapability(CAPABILITY, false);

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", false);
        }
#pragma warning restore CA1822
    }
}
