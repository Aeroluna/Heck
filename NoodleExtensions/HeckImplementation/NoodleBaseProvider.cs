using Heck.BaseProvider;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace NoodleExtensions
{
    internal class NoodleBaseProvider : IBaseProvider
    {
        [BaseProvider("baseHeadLocalPosition")]
        internal Vector3 HeadLocalPosition { get; set; }

        [BaseProvider("baseLeftHandLocalPosition")]
        internal Vector3 LeftHandLocalPosition { get; set; }

        [BaseProvider("baseRightHandLocalPosition")]
        internal Vector3 RightHandLocalPosition { get; set; }

        [BaseProvider("baseHeadLocalRotation")]
        internal Vector3 HeadLocalRotation { get; set; }

        [BaseProvider("baseLeftHandLocalRotation")]
        internal Vector3 LeftHandLocalRotation { get; set; }

        [BaseProvider("baseRightHandLocalRotation")]
        internal Vector3 RightHandLocalRotation { get; set; }

        [BaseProvider("baseHeadLocalScale")]
        internal Vector3 HeadLocalScale { get; set; }

        [BaseProvider("baseLeftHandLocalScale")]
        internal Vector3 LeftHandLocalScale { get; set; }

        [BaseProvider("baseRightHandLocalScale")]
        internal Vector3 RightHandLocalScale { get; set; }
    }

    internal class PlayerTransformGetter : ITickable
    {
        private readonly NoodleBaseProvider _noodleBaseProvider;
        private readonly Transform _head;
        private readonly Transform _leftHand;
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
            _noodleBaseProvider.HeadLocalRotation = _head.localRotation.eulerAngles;
            _noodleBaseProvider.LeftHandLocalRotation = _leftHand.localRotation.eulerAngles;
            _noodleBaseProvider.RightHandLocalRotation = _rightHand.localRotation.eulerAngles;
            _noodleBaseProvider.HeadLocalScale = _head.localScale;
            _noodleBaseProvider.LeftHandLocalScale = _leftHand.localScale;
            _noodleBaseProvider.RightHandLocalScale = _rightHand.localScale;
        }
    }
}
