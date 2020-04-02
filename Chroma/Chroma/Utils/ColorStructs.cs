using UnityEngine;

namespace Chroma
{
    internal struct NamedColor
    {
        internal string name { get; }
        internal Color? color { get; }

        internal NamedColor(string name, Color? color)
        {
            if (name.Length > 13) name = name.Substring(0, 13);
            this.name = name;
            this.color = color;
        }
    }

    internal struct TimedColor
    {
        internal float time { get; }
        internal Color color { get; }

        internal TimedColor(float time, Color color)
        {
            this.time = time;
            this.color = color;
        }
    }
}