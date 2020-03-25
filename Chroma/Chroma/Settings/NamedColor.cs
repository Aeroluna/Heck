using UnityEngine;

namespace Chroma
{
    internal struct NamedColor
    {
        internal string name { get; private set; }
        internal Color? color { get; private set; }

        internal NamedColor(string name, Color? color)
        {
            if (name.Length > 13) name = name.Substring(0, 13);
            this.name = name;
            this.color = color;
        }
    }
}