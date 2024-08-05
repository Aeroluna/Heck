using Chroma.Lighting;
using SiraUtil.Affinity;

namespace Chroma.HarmonyPatches.Colorizer.Initialize;

internal class LightColorizerInitialize : IAffinity
{
    private readonly ChromaLightSwitchEventEffect.Factory _factory;

    private LightColorizerInitialize(ChromaLightSwitchEventEffect.Factory factory)
    {
        _factory = factory;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(LightSwitchEventEffect), nameof(LightSwitchEventEffect.Start))]
    private bool IntializeChromaLightSwitchEventEffect(LightSwitchEventEffect __instance)
    {
        _factory.Create(__instance);
        return false;
    }
}
