using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoodleExtensions
{
    internal class PositionData : MovementData
    {
        internal Vector3 startPosition { get; }
        internal Vector3 endPosition { get; }
        internal Easings.Functions easing { get; }
        internal bool relative { get; }

        internal PositionData(float time, float duration, IEnumerable<float> startPosition, IEnumerable<float> endPosition, string easing, bool? relative) : base(time, duration)
        {
            this.startPosition = EnumerableToVector(startPosition) ?? new Vector3(0, 0, 0);
            this.endPosition = EnumerableToVector(endPosition) ?? this.startPosition;
            this.easing = string.IsNullOrEmpty(easing) ? Easings.Functions.easeLinear : (Easings.Functions)Enum.Parse(typeof(Easings.Functions), easing);
            this.relative = relative ?? false;
        }

        internal static Vector3? EnumerableToVector(IEnumerable<float> enumerable)
        {
            if (enumerable == null) return null;
            return new Vector3(enumerable.ElementAt(0), enumerable.ElementAt(1), enumerable.ElementAt(2));
        }
    }
}