using UnityEngine;

namespace Chroma
{
    public struct NamedColor
    {
        public string name;
        public Color? color;

        public NamedColor(string name, Color? color)
        {
            if (name.Length > 13) name = name.Substring(0, 13);
            this.name = name;
            this.color = color;
        }
    }
}