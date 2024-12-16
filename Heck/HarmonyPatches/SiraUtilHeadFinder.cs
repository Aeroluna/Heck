using SiraUtil.Affinity;
using SiraUtil.Tools.FPFC;
using UnityEngine;

namespace Heck.HarmonyPatches;

[HeckPatch(PatchType.Features)]
internal class SiraUtilHeadFinder : IAffinity
{
    internal Transform? FpfcHeadTransform { get; private set; }

    // cant get GameTransformFPFCListener injected for some reason
    [AffinityPostfix]
    [AffinityPatch(typeof(GameTransformFPFCListener), nameof(GameTransformFPFCListener.Enabled))]
    private void Enabled(Transform ____originalHeadTransform)
    {
        FpfcHeadTransform = ____originalHeadTransform;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(GameTransformFPFCListener), nameof(GameTransformFPFCListener.Disabled))]
    private void Disabled()
    {
        FpfcHeadTransform = null;
    }
}
