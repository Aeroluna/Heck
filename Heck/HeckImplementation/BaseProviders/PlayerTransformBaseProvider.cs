using Heck.BaseProvider;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck.BaseProviders;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class PlayerTransformBaseProvider : IBaseProvider
{
    internal Vector3 HeadLocalPosition { get; set; }

    internal Quaternion HeadLocalRotation { get; set; }

    internal Vector3 HeadLocalScale { get; set; }

    internal Vector3 HeadPosition { get; set; }

    internal Quaternion HeadRotation { get; set; }

    internal Vector3 LeftHandLocalPosition { get; set; }

    internal Quaternion LeftHandLocalRotation { get; set; }

    internal Vector3 LeftHandLocalScale { get; set; }

    internal Vector3 LeftHandPosition { get; set; }

    internal Quaternion LeftHandRotation { get; set; }

    internal Vector3 RightHandLocalPosition { get; set; }

    internal Quaternion RightHandLocalRotation { get; set; }

    internal Vector3 RightHandLocalScale { get; set; }

    internal Vector3 RightHandPosition { get; set; }

    internal Quaternion RightHandRotation { get; set; }
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
        _playerTransformBaseProvider.HeadLocalPosition = _head.localPosition;
        _playerTransformBaseProvider.LeftHandLocalPosition = _leftHand.localPosition;
        _playerTransformBaseProvider.RightHandLocalPosition = _rightHand.localPosition;
        _playerTransformBaseProvider.HeadLocalRotation = _head.localRotation;
        _playerTransformBaseProvider.LeftHandLocalRotation = _leftHand.localRotation;
        _playerTransformBaseProvider.RightHandLocalRotation = _rightHand.localRotation;
        _playerTransformBaseProvider.HeadPosition = _head.position;
        _playerTransformBaseProvider.LeftHandPosition = _leftHand.position;
        _playerTransformBaseProvider.RightHandPosition = _rightHand.position;
        _playerTransformBaseProvider.HeadRotation = _head.rotation;
        _playerTransformBaseProvider.LeftHandRotation = _leftHand.rotation;
        _playerTransformBaseProvider.RightHandRotation = _rightHand.rotation;
        _playerTransformBaseProvider.HeadLocalScale = _head.localScale;
        _playerTransformBaseProvider.LeftHandLocalScale = _leftHand.localScale;
        _playerTransformBaseProvider.RightHandLocalScale = _rightHand.localScale;
    }
}
