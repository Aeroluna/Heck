using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes;

internal class PlayerTransformsHeadOffsetAdjust : IAffinity
{
    private readonly NoodlePlayerTransformManager _noodlePlayerTransformManager;

    private Transform? _parentTransform;

    private PlayerTransformsHeadOffsetAdjust(NoodlePlayerTransformManager noodlePlayerTransformManager)
    {
        _noodlePlayerTransformManager = noodlePlayerTransformManager;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(PlayerTransforms), nameof(PlayerTransforms.HeadOffsetZ))]
    private bool PrefixHeadOffset(
        Quaternion noteInverseWorldRotation,
        Vector3 ____headPseudoLocalPos,
        Transform ____originParentTransform,
        ref float __result)
    {
        // get magnitude in direction we care about rather than just z
        _parentTransform ??= _noodlePlayerTransformManager.Active
            ? _noodlePlayerTransformManager.Head
            : ____originParentTransform;
        __result = Vector3.Dot(noteInverseWorldRotation * ____headPseudoLocalPos, _parentTransform.forward);
        return false;
    }
}
