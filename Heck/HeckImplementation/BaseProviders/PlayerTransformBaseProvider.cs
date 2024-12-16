using Heck.BaseProvider;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck.BaseProviders;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class PlayerTransformBaseProvider : IBaseProvider
{
    internal float[] HeadLocalPosition { get; set; } = new float[3];

    internal float[] HeadLocalRotation { get; set; } = new float[3];

    internal float[] HeadLocalScale { get; set; } = new float[3];

    internal float[] HeadPosition { get; set; } = new float[3];

    internal float[] HeadRotation { get; set; } = new float[3];

    internal float[] LeftHandLocalPosition { get; set; } = new float[3];

    internal float[] LeftHandLocalRotation { get; set; } = new float[3];

    internal float[] LeftHandLocalScale { get; set; } = new float[3];

    internal float[] LeftHandPosition { get; set; } = new float[3];

    internal float[] LeftHandRotation { get; set; } = new float[3];

    internal float[] RightHandLocalPosition { get; set; } = new float[3];

    internal float[] RightHandLocalRotation { get; set; } = new float[3];

    internal float[] RightHandLocalScale { get; set; } = new float[3];

    internal float[] RightHandPosition { get; set; } = new float[3];

    internal float[] RightHandRotation { get; set; } = new float[3];
}

internal class PlayerTransformGetter : ITickable
{
    private readonly PlayerTransformBaseProvider _playerTransformBaseProvider;
    private readonly PlayerTransforms _playerTransforms;

    [UsedImplicitly]
    private PlayerTransformGetter(PlayerTransformBaseProvider playerTransformBaseProvider, PlayerTransforms playerTransforms)
    {
        _playerTransformBaseProvider = playerTransformBaseProvider;
        _playerTransforms = playerTransforms;
    }

    public void Tick()
    {
        Transform head = _playerTransforms._headTransform;
        Transform leftHand = _playerTransforms._leftHandTransform;
        Transform rightHand = _playerTransforms._rightHandTransform;
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
        Vector3 euler = quaternion.eulerAngles;
        array[0] = euler.x;
        array[1] = euler.y;
        array[2] = euler.z;
    }

    private static void Vector3ToValues(float[] array, Vector3 vector)
    {
        array[0] = vector.x;
        array[1] = vector.y;
        array[2] = vector.z;
    }
}
