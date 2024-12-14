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
    private readonly Transform _head;
    private readonly Transform _leftHand;
    private readonly PlayerTransformBaseProvider _playerTransformBaseProvider;
    private readonly Transform _rightHand;

    [UsedImplicitly]
    private PlayerTransformGetter(PlayerTransformBaseProvider playerTransformBaseProvider, PlayerTransforms playerTransforms)
    {
        _playerTransformBaseProvider = playerTransformBaseProvider;
        _head = playerTransforms._headTransform;
        _leftHand = playerTransforms._leftHandTransform;
        _rightHand = playerTransforms._rightHandTransform;
    }

    public void Tick()
    {
        Vector3ToValues(_playerTransformBaseProvider.HeadLocalPosition, _head.localPosition);
        Vector3ToValues(_playerTransformBaseProvider.LeftHandLocalPosition, _leftHand.localPosition);
        Vector3ToValues(_playerTransformBaseProvider.RightHandLocalPosition, _rightHand.localPosition);
        QuaternionToValues(_playerTransformBaseProvider.HeadLocalRotation, _head.localRotation);
        QuaternionToValues(_playerTransformBaseProvider.LeftHandLocalRotation, _leftHand.localRotation);
        QuaternionToValues(_playerTransformBaseProvider.RightHandLocalRotation, _rightHand.localRotation);
        Vector3ToValues(_playerTransformBaseProvider.HeadPosition, _head.position);
        Vector3ToValues(_playerTransformBaseProvider.LeftHandPosition, _leftHand.position);
        Vector3ToValues(_playerTransformBaseProvider.RightHandPosition, _rightHand.position);
        QuaternionToValues(_playerTransformBaseProvider.HeadRotation, _head.rotation);
        QuaternionToValues(_playerTransformBaseProvider.LeftHandRotation, _leftHand.rotation);
        QuaternionToValues(_playerTransformBaseProvider.RightHandRotation, _rightHand.rotation);
        Vector3ToValues(_playerTransformBaseProvider.HeadLocalScale, _head.localScale);
        Vector3ToValues(_playerTransformBaseProvider.LeftHandLocalScale, _leftHand.localScale);
        Vector3ToValues(_playerTransformBaseProvider.RightHandLocalScale, _rightHand.localScale);
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
