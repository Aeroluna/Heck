using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage.GameplaySetup;
using Chroma.Lighting;
using Chroma.Lighting.EnvironmentEnhancement;
using Chroma.Settings;
using Chroma.Utils;
using CustomJSONData;
using Heck;
using Heck.Animation;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using static Chroma.ChromaController;

namespace Chroma
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
#pragma warning disable CA1822
        [UsedImplicitly]
        [Init]
        public void Init(IPA.Logging.Logger pluginLogger, Config conf)
        {
            Log.Logger = new HeckLogger(pluginLogger);
            ChromaSettableSettings.SetupSettableSettings();
            ChromaConfig.Instance = conf.Generated<ChromaConfig>();
            HeckPatchDataManager.InitPatches(HarmonyInstance, Assembly.GetExecutingAssembly());
            LightIDTableManager.InitTable();
            EnvironmentEnhancementManager.SaveLookupIDDLL();
        }

        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            HarmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());

            SiraUtilInstalled = PluginManager.EnabledPlugins.Any(x => x.Id == "SiraUtil");

            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);

            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            CustomEventCallbackController.didInitEvent += ChromaFogController.CustomEventCallbackInit;
            TrackBuilder.TrackCreated += AnimationHelper.OnTrackCreated;
            CustomDataDeserializer.BuildTracks += ChromaCustomDataManager.OnBuildTracks;
            CustomDataDeserializer.DeserializeBeatmapData += ChromaCustomDataManager.OnDeserializeBeatmapData;

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            HarmonyInstanceCore.UnpatchAll(HARMONY_ID);

            GameplaySetup.instance.RemoveTab("Chroma");

            ChromaUtils.SetSongCoreCapability(CAPABILITY, false);

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            CustomEventCallbackController.didInitEvent -= ChromaFogController.CustomEventCallbackInit;
            TrackBuilder.TrackCreated -= AnimationHelper.OnTrackCreated;
            CustomDataDeserializer.BuildTracks -= ChromaCustomDataManager.OnBuildTracks;
            CustomDataDeserializer.DeserializeBeatmapData -= ChromaCustomDataManager.OnDeserializeBeatmapData;

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", false);
        }
#pragma warning restore CA1822
    }
}
