using SiraUtil.Affinity;
using SiraUtil.Tools.FPFC;
using UnityEngine;

namespace Heck.HarmonyPatches;

[HeckPatch(PatchType.Features)]
internal class SiraUtilHeadFinder : IAffinity
{
    private const string _noodleRoomOffset = "NoodleRoomOffset";

    internal Transform? FpfcHeadTransform { get; private set; }

    // cant get GameTransformFPFCListener injected for some reason
    [AffinityPrefix]
    [AffinityPatch(typeof(GameTransformFPFCListener), nameof(GameTransformFPFCListener.Enabled))]
    private bool Enabled(GameTransformFPFCListener __instance)
    {
        FpfcHeadTransform = __instance._originalHeadTransform;
        return __instance._originalHeadTransform?.parent?.name != _noodleRoomOffset;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(GameTransformFPFCListener), nameof(GameTransformFPFCListener.Disabled))]
    private bool Disabled(GameTransformFPFCListener __instance)
    {
        FpfcHeadTransform = null;
        return __instance._originalHeadTransform?.parent?.name != _noodleRoomOffset;
    }
}
