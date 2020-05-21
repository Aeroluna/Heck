using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoodleExtensions
{
    internal class RotationData : MovementData
    {
        internal static Quaternion savedRotation;
        internal Quaternion startRotation { get; }
        internal Quaternion endRotation { get; }
        internal Easings.Functions easing { get; }

        internal RotationData(float time, float duration, IEnumerable<float> startRotation, IEnumerable<float> endRotation, string easing) : base(time, duration)
        {
            this.startRotation = EnumerableToQuaternion(startRotation) ?? savedRotation;
            this.endRotation = EnumerableToQuaternion(endRotation) ?? this.startRotation;
            savedRotation = this.endRotation;
            this.easing = string.IsNullOrEmpty(easing) ? Easings.Functions.easeLinear : (Easings.Functions)Enum.Parse(typeof(Easings.Functions), easing);
        }

        internal static Quaternion? EnumerableToQuaternion(IEnumerable<float> enumerable)
        {
            if (enumerable == null) return null;
            return Quaternion.Euler(enumerable.ElementAt(0), enumerable.ElementAt(1), enumerable.ElementAt(2));
        }
    }
}