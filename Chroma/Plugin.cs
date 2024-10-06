using System.Threading.Tasks;
using Chroma.Installers;
using Chroma.Lighting;
using Chroma.Settings;
using Heck.Animation;
using Heck.Patcher;
using IPA;
using IPA.Config.Stores;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using UnityEngine;
using static Chroma.ChromaController;
using Config = Chroma.Settings.Config;
using Logger = IPA.Logging.Logger;

namespace Chroma;

[Plugin(RuntimeOptions.DynamicInit)]
internal class Plugin
{
    private readonly Config _config;

    [UsedImplicitly]
    [Init]
    public Plugin(Logger pluginLogger, IPA.Config.Config conf, Zenjector zenjector)
    {
        Log = pluginLogger;

        ChromaSettableSettings.SetupSettableSettings();
        Task.Run(LightIDTableManager.InitTable);
        _config = conf.Generated<Config>();
        zenjector.Install<ChromaPlayerInstaller>(Location.Player);
        zenjector.Install<ChromaAppInstaller>(Location.App, _config);
        zenjector.Install<ChromaMenuInstaller>(Location.Menu);
        zenjector.UseLogger(pluginLogger);

        HeckPatchManager.Register(HARMONY_ID);

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
        // ChromaConfig wont set if there is no config!
        if (!_config.ChromaEventsDisabled)
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
        Capability.Deregister();

        // Legacy support
        LegacyCapability.Deregister();
    }
#pragma warning restore CA1822
}
