using Chroma.EnvironmentEnhancement.Saved;
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
using UnityEngine;
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
            SavedEnvironmentLoader.Init();
            ChromaConfig.Instance = conf.Generated<ChromaConfig>();
            LightIDTableManager.InitTable();
            zenjector.Install<ChromaPlayerInstaller>(Location.Player);
            zenjector.Install<ChromaAppInstaller>(Location.App);

            Track.RegisterProperty<Vector4>(COLOR, V2_COLOR);
            Track.RegisterPathProperty<Vector4>(COLOR, V2_COLOR);

            // For Fog Control
            Track.RegisterProperty<float>(V2_ATTENUATION, V2_ATTENUATION);
            Track.RegisterProperty<float>(V2_OFFSET, V2_OFFSET);
            Track.RegisterProperty<float>(V2_HEIGHT_FOG_STARTY, V2_HEIGHT_FOG_STARTY);
            Track.RegisterProperty<float>(V2_HEIGHT_FOG_HEIGHT, V2_HEIGHT_FOG_HEIGHT);
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            CorePatcher.Enabled = true;
            FeaturesModule.Enabled = true;
            ColorizerModule.Enabled = true;
            EnvironmentModule.Enabled = true;

            // ChromaConfig wont set if there is no config!
            ChromaUtils.SetSongCoreCapability(CAPABILITY, !ChromaConfig.Instance.ChromaEventsDisabled);

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            CorePatcher.Enabled = false;
            FeaturesPatcher.Enabled = false;
            EnvironmentPatcher.Enabled = false;
            FeaturesModule.Enabled = false;
            ColorizerModule.Enabled = false;
            Deserializer.Enabled = false;

            ChromaUtils.SetSongCoreCapability(CAPABILITY, false);

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", false);
        }
#pragma warning restore CA1822
    }
}
