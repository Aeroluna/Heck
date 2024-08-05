using UnityEngine;

namespace Chroma.Colorizer;

public abstract class ObjectColorizer
{
    private Color? _color;

    public Color Color => _color ?? GlobalColorGetter ?? OriginalColorGetter;

    protected abstract Color? GlobalColorGetter { get; }

    protected virtual Color OriginalColorGetter => OriginalColor;

    protected Color OriginalColor { get; set; }

    public void Colorize(Color? color)
    {
        _color = color;
        Refresh();
    }

    internal abstract void Refresh();
}
