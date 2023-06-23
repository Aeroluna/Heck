using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Chroma.Extras;
using Chroma.Installers;
using Chroma.Lighting;
using Chroma.Settings;
using Heck.Animation;
using SiraUtil.Zenject;
using UnityEngine;
using static Chroma.ChromaController;

namespace Chroma
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("SongCore")]
    [BepInDependency("BSIPA_Utilities")]
    [BepInDependency("BeatSaberMarkupLanguage")]
    [BepInDependency("CustomJSONData")]
    [BepInDependency("SiraUtil")]
    [BepInDependency("Heck")]
    [BepInProcess("Beat Saber.exe")]
    internal class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; } = null!;

        private void Awake()
        {
            Log = Logger;

            BepInPlugin metadata = Info.Metadata;
            Config config = new(new ConfigFile(Path.Combine(Paths.ConfigPath, ID, metadata.GUID + ".cfg"), true, metadata));
            LightIDTableManager.InitTable();

            Zenjector zenjector = Zenjector.ConstructZenjector(Info);
            zenjector.Install<ChromaPlayerInstaller>(Location.Player);
            zenjector.Install<ChromaAppInstaller>(Location.App, config);
            zenjector.Install<ChromaMenuInstaller>(Location.Menu);

            Track.RegisterProperty<Vector4>(COLOR, V2_COLOR);
            Track.RegisterPathProperty<Vector4>(COLOR, V2_COLOR);

            // For Fog Control
            Track.RegisterProperty<float>(V2_ATTENUATION, V2_ATTENUATION);
            Track.RegisterProperty<float>(V2_OFFSET, V2_OFFSET);
            Track.RegisterProperty<float>(V2_HEIGHT_FOG_STARTY, V2_HEIGHT_FOG_STARTY);
            Track.RegisterProperty<float>(V2_HEIGHT_FOG_HEIGHT, V2_HEIGHT_FOG_HEIGHT);
        }

#pragma warning disable CA1822
        private void OnEnable()
        {
            CorePatcher.Enabled = true;
            FeaturesModule.Enabled = true;
            ColorizerModule.Enabled = true;
            EnvironmentModule.Enabled = true;

            // ChromaConfig wont set if there is no config!
            ChromaUtils.SetSongCoreCapability(CAPABILITY, !Chroma.Settings.Config.Instance.ChromaEventsDisabled);

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");
        }

        private void OnDisable()
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
