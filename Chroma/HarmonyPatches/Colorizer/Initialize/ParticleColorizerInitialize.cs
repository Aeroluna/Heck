using Chroma.Colorizer;
using SiraUtil.Affinity;

namespace Chroma.HarmonyPatches.Colorizer.Initialize
{
    internal class ParticleColorizerInitialize : IAffinity
    {
        private readonly ParticleColorizerManager _manager;

        private ParticleColorizerInitialize(ParticleColorizerManager manager)
        {
            _manager = manager;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ParticleSystemEventEffect), nameof(ParticleSystemEventEffect.Start))]
        private void IntializeParticleColorizer(ParticleSystemEventEffect __instance)
        {
            _manager.Create(__instance);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ParticleSystemEventEffect), nameof(ParticleSystemEventEffect.HandleBeatmapEvent))]
        private bool SkipCallback()
        {
            return false;
        }
    }
}
