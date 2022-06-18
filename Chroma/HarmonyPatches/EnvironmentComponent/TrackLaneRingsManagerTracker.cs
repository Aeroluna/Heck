using Chroma.EnvironmentEnhancement;
using SiraUtil.Affinity;
using Zenject;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    internal class TrackLaneRingsManagerTracker : IAffinity
    {
        private readonly LazyInject<ComponentInitializer> _componentInitializer;

        private TrackLaneRingsManagerTracker(LazyInject<ComponentInitializer> componentInitializer)
        {
            _componentInitializer = componentInitializer;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(TrackLaneRingsManager), nameof(TrackLaneRingsManager.Awake))]
        private bool Prefix()
        {
            return !_componentInitializer.Value.SkipAwake;
        }
    }
}
