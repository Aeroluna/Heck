using System.Reflection;
using Heck;
using Heck.Animation;
using IPA;
using JetBrains.Annotations;
using SongCore;
using static NoodleExtensions.NoodleController;
using AnimationHelper = NoodleExtensions.Animation.AnimationHelper;

namespace NoodleExtensions
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
#pragma warning disable CA1822
        [UsedImplicitly]
        [Init]
        public void Init(IPA.Logging.Logger pluginLogger)
        {
            Log.Logger = new HeckLogger(pluginLogger);
            HeckPatchDataManager.InitPatches(HarmonyInstance, Assembly.GetExecutingAssembly());
        }

        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            Collections.RegisterCapability(CAPABILITY);
            HarmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());

            TrackBuilder.TrackCreated += AnimationHelper.OnTrackCreated;
            CustomDataDeserializer.BuildTracks += NoodleCustomDataManager.OnBuildTracks;
            CustomDataDeserializer.DeserializeBeatmapData += NoodleCustomDataManager.OnDeserializeBeatmapData;
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            Collections.DeregisterizeCapability(CAPABILITY);
            HarmonyInstanceCore.UnpatchAll(HARMONY_ID_CORE);
            HarmonyInstanceCore.UnpatchAll(HARMONY_ID);

            TrackBuilder.TrackCreated -= AnimationHelper.OnTrackCreated;
            CustomDataDeserializer.BuildTracks -= NoodleCustomDataManager.OnBuildTracks;
            CustomDataDeserializer.DeserializeBeatmapData -= NoodleCustomDataManager.OnDeserializeBeatmapData;
        }
#pragma warning restore CA1822
    }
}
