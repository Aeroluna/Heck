using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (startRotation == null) this.startRotation = savedRotation;
            else this.startRotation = Quaternion.Euler(startRotation.ElementAt(0), startRotation.ElementAt(1), startRotation.ElementAt(2));

            if (endRotation == null) this.endRotation = this.startRotation;
            else this.endRotation = Quaternion.Euler(endRotation.ElementAt(0), endRotation.ElementAt(1), endRotation.ElementAt(2));

            savedRotation = this.endRotation;

            this.easing = string.IsNullOrEmpty(easing) ? Easings.Functions.easeLinear : (Easings.Functions)Enum.Parse(typeof(Easings.Functions), easing);
        }
    }
}
