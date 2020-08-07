namespace Chroma
{
    using UnityEngine;

    internal struct TimedColor
    {
        internal TimedColor(float time, Color color)
        {
            Time = time;
            Color = color;
        }

        internal float Time { get; }

        internal Color Color { get; }
    }
}
