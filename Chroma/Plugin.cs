using Chroma.Extras;
using Chroma.Installers;
using Chroma.Lighting;
using Chroma.Settings;
using Heck.Animation;
using IPA;
using IPA.Config.Stores;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using UnityEngine;
using static Chroma.ChromaController;
using Config = Chroma.Settings.Config;
using Logger = IPA.Logging.Logger;

namespace Chroma
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        [UsedImplicitly]
        [Init]
        public Plugin(Logger pluginLogger, IPA.Config.Config conf, Zenjector zenjector)
        {
            Log = pluginLogger;

            ChromaSettableSettings.SetupSettableSettings();
            LightIDTableManager.InitTable();
            zenjector.Install<ChromaPlayerInstaller>(Location.Player);
            zenjector.Install<ChromaAppInstaller>(Location.App, conf.Generated<Config>());
            zenjector.Install<ChromaMenuInstaller>(Location.Menu);
            zenjector.UseLogger(pluginLogger);

            Track.RegisterProperty<Vector4>(COLOR, V2_COLOR);
            Track.RegisterPathProperty<Vector4>(COLOR, V2_COLOR);

            // For Fog Control
            Track.RegisterProperty<float>(V2_ATTENUATION, V2_ATTENUATION);
            Track.RegisterProperty<float>(V2_OFFSET, V2_OFFSET);
            Track.RegisterProperty<float>(V2_HEIGHT_FOG_STARTY, V2_HEIGHT_FOG_STARTY);
            Track.RegisterProperty<float>(V2_HEIGHT_FOG_HEIGHT, V2_HEIGHT_FOG_HEIGHT);
        }

        internal static Logger Log { get; private set; } = null!;

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
            if (!Config.Instance.ChromaEventsDisabled)
            {
                Capability.Register();
            }
            else
            {
                Capability.Deregister();
            }

            // Legacy support
            LegacyCapability.Register();
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

            Capability.Deregister();

            // Legacy support
            LegacyCapability.Deregister();
        }
#pragma warning restore CA1822
    }
}
