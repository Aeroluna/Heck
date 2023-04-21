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
    }

    internal class PlayerTransformGetter : ITickable
    {
        private readonly NoodleBaseProvider _noodleBaseProvider;
        private readonly Transform _head;
        private readonly Transform _leftHand;
        private readonly Transform _rightHand;

        [UsedImplicitly]
        private PlayerTransformGetter(NoodleBaseProvider noodleBaseProvider)
        {
            _noodleBaseProvider = noodleBaseProvider;
            _head = GameObject.Find("VRGameCore/MainCamera").transform;
            _leftHand = GameObject.Find("VRGameCore/LeftHand").transform;
            _rightHand = GameObject.Find("VRGameCore/RightHand").transform;
        }

        public void Tick()
        {
            _noodleBaseProvider.HeadLocalPosition = _head.localPosition;
            _noodleBaseProvider.LeftHandLocalPosition = _leftHand.localPosition;
            _noodleBaseProvider.RightHandLocalPosition = _rightHand.localPosition;
        }
    }
}
