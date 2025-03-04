using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes;

internal class PlayerTransformsNoodlePatch : IAffinity
{
    private readonly NoodlePlayerTransformManager _noodlePlayerTransformManager;

    private Transform? _parentTransform;

    private PlayerTransformsNoodlePatch(NoodlePlayerTransformManager noodlePlayerTransformManager)
    {
        _noodlePlayerTransformManager = noodlePlayerTransformManager;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(PlayerTransforms), nameof(PlayerTransforms.Update))]
    private bool UpdateWithNoodle(
#if LATEST
        BeatmapKey? ____beatmapKey,
        Transform ____originParentTransform,
        ref Vector3 ____headPseudoLocalZOnlyPos,
#endif
        bool ____overrideHeadPos,
        Transform ____headTransform,
        Transform ____rightHandTransform,
        Transform ____leftHandTransform,
        ref Vector3 ____headWorldPos,
        ref Quaternion ____headWorldRot,
        ref Vector3 ____headPseudoLocalPos,
        ref Quaternion ____headPseudoLocalRot,
        ref Vector3 ____rightHandPseudoLocalPos,
        ref Quaternion ____rightHandPseudoLocalRot,
        ref Vector3 ____leftHandPseudoLocalPos,
        ref Quaternion ____leftHandPseudoLocalRot)
    {
        if (____overrideHeadPos || !_noodlePlayerTransformManager.Active)
        {
            return true;
        }

        ____headWorldPos = ____headTransform.position;
        ____headWorldRot = ____headTransform.rotation;

        ____headPseudoLocalPos = _noodlePlayerTransformManager.Head.InverseTransformPoint(____headWorldPos);
        ____headPseudoLocalRot = _noodlePlayerTransformManager.Head.InverseTransformRotation(____headWorldRot);
        ____rightHandPseudoLocalPos = _noodlePlayerTransformManager.RightHand.InverseTransformPoint(____rightHandTransform.position);
        ____rightHandPseudoLocalRot = _noodlePlayerTransformManager.RightHand.InverseTransformRotation(____rightHandTransform.rotation);
        ____leftHandPseudoLocalPos = _noodlePlayerTransformManager.LeftHand.InverseTransformPoint(____leftHandTransform.position);
        ____leftHandPseudoLocalRot = _noodlePlayerTransformManager.LeftHand.InverseTransformRotation(____leftHandTransform.rotation);

#if LATEST
        if (____beatmapKey != null && ____beatmapKey.Value.beatmapCharacteristic.containsRotationEvents)
        {
            return false;
        }

        ____headPseudoLocalZOnlyPos = HeadOffsetZ(____headPseudoLocalPos, ____originParentTransform) * _parentTransform!.forward;
#endif

        return false;
    }

#if !LATEST
    [AffinityPrefix]
    [AffinityPatch(typeof(PlayerTransforms), nameof(PlayerTransforms.HeadOffsetZ))]
    private bool PrefixHeadOffset(
        Quaternion noteInverseWorldRotation,
        Vector3 ____headPseudoLocalPos,
        Transform ____originParentTransform,
        ref float __result)
    {
        __result = HeadOffsetZ(noteInverseWorldRotation * ____headPseudoLocalPos, ____originParentTransform);
        return false;
    }
#endif

    private float HeadOffsetZ(Vector3 headPsuedoLocalPos, Transform originParentTransform)
    {
        // get magnitude in direction we care about rather than just z
        _parentTransform ??= _noodlePlayerTransformManager.Active
            ? _noodlePlayerTransformManager.Head
            : originParentTransform;
        return Vector3.Dot(headPsuedoLocalPos, _parentTransform.forward);
    }
}
