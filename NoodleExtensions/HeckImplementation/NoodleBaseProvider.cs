using Heck.BaseProvider;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace NoodleExtensions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class NoodleBaseProvider : IBaseProvider
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
    private readonly NoodleBaseProvider _noodleBaseProvider;
    private readonly Transform _rightHand;

    [UsedImplicitly]
    private PlayerTransformGetter(NoodleBaseProvider noodleBaseProvider, PlayerTransforms playerTransforms)
    {
        _noodleBaseProvider = noodleBaseProvider;
        _head = playerTransforms._headTransform;
        _leftHand = playerTransforms._leftHandTransform;
        _rightHand = playerTransforms._rightHandTransform;
    }

    public void Tick()
    {
        _noodleBaseProvider.HeadLocalPosition = _head.localPosition;
        _noodleBaseProvider.LeftHandLocalPosition = _leftHand.localPosition;
        _noodleBaseProvider.RightHandLocalPosition = _rightHand.localPosition;
        _noodleBaseProvider.HeadLocalRotation = _head.localRotation;
        _noodleBaseProvider.LeftHandLocalRotation = _leftHand.localRotation;
        _noodleBaseProvider.RightHandLocalRotation = _rightHand.localRotation;
        _noodleBaseProvider.HeadPosition = _head.position;
        _noodleBaseProvider.LeftHandPosition = _leftHand.position;
        _noodleBaseProvider.RightHandPosition = _rightHand.position;
        _noodleBaseProvider.HeadRotation = _head.rotation;
        _noodleBaseProvider.LeftHandRotation = _leftHand.rotation;
        _noodleBaseProvider.RightHandRotation = _rightHand.rotation;
        _noodleBaseProvider.HeadLocalScale = _head.localScale;
        _noodleBaseProvider.LeftHandLocalScale = _leftHand.localScale;
        _noodleBaseProvider.RightHandLocalScale = _rightHand.localScale;
    }
}
