using System;
using HMUI;
using SiraUtil.Affinity;

namespace Heck.HarmonyPatches;

internal class FlowCoordinatorTransitionListener : IAffinity
{
    internal event Action<FlowCoordinator>? TransitionFinished;

    [AffinityPostfix]
    [AffinityPatch(typeof(FlowCoordinator), nameof(FlowCoordinator.TransitionDidFinish))]
    private void Postfix(FlowCoordinator __instance)
    {
        TransitionFinished?.Invoke(__instance);
    }
}
