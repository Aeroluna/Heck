using Heck.BaseProvider;
using Heck.HarmonyPatches;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck.BaseProviders;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class PlayerTransformBaseProvider : IBaseProvider
{
    internal float[] HeadLocalPosition { get; set; } = new float[3];

    [QuaternionBase]
    internal float[] HeadLocalRotation { get; set; } = new float[4];

    internal float[] HeadLocalScale { get; set; } = new float[3];

    internal float[] HeadPosition { get; set; } = new float[3];

    [QuaternionBase]
    internal float[] HeadRotation { get; set; } = new float[4];

    internal float[] LeftHandLocalPosition { get; set; } = new float[3];

    [QuaternionBase]
    internal float[] LeftHandLocalRotation { get; set; } = new float[4];

    internal float[] LeftHandLocalScale { get; set; } = new float[3];

    internal float[] LeftHandPosition { get; set; } = new float[3];

    [QuaternionBase]
    internal float[] LeftHandRotation { get; set; } = new float[4];

    internal float[] RightHandLocalPosition { get; set; } = new float[3];

    [QuaternionBase]
    internal float[] RightHandLocalRotation { get; set; } = new float[4];

    internal float[] RightHandLocalScale { get; set; } = new float[3];

    internal float[] RightHandPosition { get; set; } = new float[3];

    [QuaternionBase]
    internal float[] RightHandRotation { get; set; } = new float[4];
}

internal class PlayerTransformGetter : ITickable
{
    private readonly PlayerTransformBaseProvider _playerTransformBaseProvider;
    private readonly PlayerTransforms _playerTransforms;
    private readonly PlayerVRControllersManager _playerVRControllersManager;
    private readonly SiraUtilHeadFinder _siraUtilHeadFinder;

    [UsedImplicitly]
    private PlayerTransformGetter(
        PlayerTransformBaseProvider playerTransformBaseProvider,
        PlayerTransforms playerTransforms,
        PlayerVRControllersManager playerVRControllersManager,
        SiraUtilHeadFinder siraUtilHeadFinder)
    {
        _playerTransformBaseProvider = playerTransformBaseProvider;
        _playerTransforms = playerTransforms;
        _playerVRControllersManager = playerVRControllersManager;
        _siraUtilHeadFinder = siraUtilHeadFinder;
    }

    public void Tick()
    {
        Transform head = _siraUtilHeadFinder.FpfcHeadTransform ?? _playerTransforms._headTransform;

        // _playerTransforms._leftHandTransform points to the saber instead of the hand in 1.34+
        Transform leftHand = _playerVRControllersManager.leftHandVRController.transform;
        Transform rightHand = _playerVRControllersManager.rightHandVRController.transform;

        Vector3ToValues(_playerTransformBaseProvider.HeadLocalPosition, head.localPosition);
        Vector3ToValues(_playerTransformBaseProvider.LeftHandLocalPosition, leftHand.localPosition);
        Vector3ToValues(_playerTransformBaseProvider.RightHandLocalPosition, rightHand.localPosition);
        QuaternionToValues(_playerTransformBaseProvider.HeadLocalRotation, head.localRotation);
        QuaternionToValues(_playerTransformBaseProvider.LeftHandLocalRotation, leftHand.localRotation);
        QuaternionToValues(_playerTransformBaseProvider.RightHandLocalRotation, rightHand.localRotation);
        Vector3ToValues(_playerTransformBaseProvider.HeadPosition, head.position);
        Vector3ToValues(_playerTransformBaseProvider.LeftHandPosition, leftHand.position);
        Vector3ToValues(_playerTransformBaseProvider.RightHandPosition, rightHand.position);
        QuaternionToValues(_playerTransformBaseProvider.HeadRotation, head.rotation);
        QuaternionToValues(_playerTransformBaseProvider.LeftHandRotation, leftHand.rotation);
        QuaternionToValues(_playerTransformBaseProvider.RightHandRotation, rightHand.rotation);
        Vector3ToValues(_playerTransformBaseProvider.HeadLocalScale, head.localScale);
        Vector3ToValues(_playerTransformBaseProvider.LeftHandLocalScale, leftHand.localScale);
        Vector3ToValues(_playerTransformBaseProvider.RightHandLocalScale, rightHand.localScale);
    }

    private static void QuaternionToValues(float[] array, Quaternion quaternion)
    {
        array[0] = quaternion.x;
        array[1] = quaternion.y;
        array[2] = quaternion.z;
        array[3] = quaternion.w;
    }

    private static void Vector3ToValues(float[] array, Vector3 vector)
    {
        array[0] = vector.x;
        array[1] = vector.y;
        array[2] = vector.z;
    }
}
