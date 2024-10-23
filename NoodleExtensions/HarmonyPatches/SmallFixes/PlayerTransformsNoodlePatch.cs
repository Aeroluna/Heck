using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes;

internal class PlayerTransformsNoodlePatch : IAffinity
{
    private readonly NoodlePlayerTransformManager _noodlePlayerTransformManager;

    private PlayerTransformsNoodlePatch(NoodlePlayerTransformManager noodlePlayerTransformManager)
    {
        _noodlePlayerTransformManager = noodlePlayerTransformManager;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(PlayerTransforms), nameof(PlayerTransforms.Update))]
    private bool UpdateWithNoodle(
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

        return false;
    }
}
