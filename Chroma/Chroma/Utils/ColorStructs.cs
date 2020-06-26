namespace Chroma
{
    using UnityEngine;

    internal struct NamedColor
    {
        internal NamedColor(string name, Color? color)
        {
            if (name.Length > 13)
            {
                name = name.Substring(0, 13);
            }

            Name = name;
            Color = color;
        }

        internal string Name { get; }

        internal Color? Color { get; }
    }

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
