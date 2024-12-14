using Heck.BaseProvider;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck.BaseProviders;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class ColorBaseProvider : IBaseProvider
{
    internal float[] EnvironmentColor0 { get; } = new float[4];

    internal float[] EnvironmentColor0Boost { get; } = new float[4];

    internal float[] EnvironmentColor1 { get; } = new float[4];

    internal float[] EnvironmentColor1Boost { get; } = new float[4];

    internal float[] EnvironmentColorW { get; } = new float[4];

    internal float[] EnvironmentColorWBoost { get; } = new float[4];

    internal float[] Note0Color { get; } = new float[4];

    internal float[] Note1Color { get; } = new float[4];

    internal float[] ObstaclesColor { get; } = new float[4];

    internal float[] SaberAColor { get; } = new float[4];

    internal float[] SaberBColor { get; } = new float[4];
}

internal class ColorSchemeGetter : IInitializable
{
    private readonly ColorBaseProvider _colorBaseProvider;
    private readonly ColorScheme _colorScheme;
    private readonly bool _leftHanded;

    [UsedImplicitly]
    private ColorSchemeGetter(
        ColorBaseProvider colorBaseProvider,
        ColorScheme colorScheme,
        [Inject(Id = HeckController.LEFT_HANDED_ID)]
        bool leftHanded)
    {
        _colorBaseProvider = colorBaseProvider;
        _colorScheme = colorScheme;
        _leftHanded = leftHanded;
    }

    public void Initialize()
    {
        ColorToValues(_colorBaseProvider.Note0Color, _leftHanded ? _colorScheme.saberBColor : _colorScheme.saberAColor);
        ColorToValues(_colorBaseProvider.Note0Color, _leftHanded ? _colorScheme.saberBColor : _colorScheme.saberAColor);
        ColorToValues(_colorBaseProvider.Note1Color, _leftHanded ? _colorScheme.saberAColor : _colorScheme.saberBColor);
        ColorToValues(_colorBaseProvider.SaberAColor, _colorScheme.saberAColor);
        ColorToValues(_colorBaseProvider.SaberBColor, _colorScheme.saberBColor);
        ColorToValues(_colorBaseProvider.EnvironmentColor0, _colorScheme.environmentColor0);
        ColorToValues(_colorBaseProvider.EnvironmentColor1, _colorScheme.environmentColor1);
        ColorToValues(_colorBaseProvider.EnvironmentColorW, _colorScheme.environmentColorW);
        ColorToValues(_colorBaseProvider.EnvironmentColor0Boost, _colorScheme.environmentColor0Boost);
        ColorToValues(_colorBaseProvider.EnvironmentColor1Boost, _colorScheme.environmentColor1Boost);
        ColorToValues(_colorBaseProvider.EnvironmentColorWBoost, _colorScheme.environmentColorWBoost);
        ColorToValues(_colorBaseProvider.ObstaclesColor, _colorScheme.obstaclesColor);
    }

    private static void ColorToValues(float[] array, Color color)
    {
        array[0] = color.r;
        array[1] = color.g;
        array[2] = color.b;
        array[3] = color.a;
    }
}
