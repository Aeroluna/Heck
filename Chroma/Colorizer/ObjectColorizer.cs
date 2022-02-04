using UnityEngine;

namespace Chroma.Colorizer
{
    public abstract class ObjectColorizer
    {
        private Color? _color;

        public Color Color => _color ?? GlobalColorGetter ?? OriginalColorGetter;

        protected Color OriginalColor { get; set; }

        protected abstract Color? GlobalColorGetter { get; }

        protected virtual Color OriginalColorGetter => OriginalColor;

        public void Colorize(Color? color)
        {
            _color = color;
            Refresh();
        }

        internal abstract void Refresh();
    }
}
