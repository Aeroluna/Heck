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

        internal PositionData(float time, float duration, IEnumerable<float> startPosition, IEnumerable<float> endPosition, string easing) : base(time, duration)
        {
            if (startPosition == null) this.startPosition = new Vector3(0, 0, 0);
            else this.startPosition = new Vector3(startPosition.ElementAt(0), startPosition.ElementAt(1), startPosition.ElementAt(2));

            if (endPosition == null) this.endPosition = this.startPosition;
            else this.endPosition = new Vector3(endPosition.ElementAt(0), endPosition.ElementAt(1), endPosition.ElementAt(2));

            this.easing = string.IsNullOrEmpty(easing) ? Easings.Functions.easeLinear : (Easings.Functions)Enum.Parse(typeof(Easings.Functions), easing);
        }
    }
}