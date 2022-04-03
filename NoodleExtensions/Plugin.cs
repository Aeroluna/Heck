using Heck;
using Heck.Animation;
using IPA;
using IPA.Logging;
using JetBrains.Annotations;
using NoodleExtensions.Animation;
using NoodleExtensions.Installers;
using SiraUtil.Zenject;
using SongCore;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        [UsedImplicitly]
        [Init]
        public Plugin(Logger pluginLogger, Zenjector zenjector)
        {
            Log.Logger = new HeckLogger(pluginLogger);

            zenjector.Install<NoodlePlayerInstaller>(Location.Player);
            zenjector.Expose<NoteCutCoreEffectsSpawner>("Gameplay");
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            Collections.RegisterCapability(CAPABILITY);
            TrackBuilder.TrackCreated += AnimationHelper.OnTrackCreated;
            CorePatcher.Enabled = true;
            FeaturesModule.Enabled = true;
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            Collections.DeregisterizeCapability(CAPABILITY);
            TrackBuilder.TrackCreated -= AnimationHelper.OnTrackCreated;
            CorePatcher.Enabled = false;
            FeaturesPatcher.Enabled = false;
            FeaturesModule.Enabled = false;
            Deserializer.Enabled = false;
        }
#pragma warning restore CA1822
    }
}
