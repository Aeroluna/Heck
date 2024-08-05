using Heck;
using Heck.BaseProvider;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal class ColorBaseProvider : IBaseProvider
{
    internal Vector4 EnvironmentColor0 { get; set; }

    internal Vector4 EnvironmentColor0Boost { get; set; }

    internal Vector4 EnvironmentColor1 { get; set; }

    internal Vector4 EnvironmentColor1Boost { get; set; }

    internal Vector4 EnvironmentColorW { get; set; }

    internal Vector4 EnvironmentColorWBoost { get; set; }

    internal Vector4 Note0Color { get; set; }

    internal Vector4 Note1Color { get; set; }

    internal Vector4 ObstaclesColor { get; set; }

    internal Vector4 SaberAColor { get; set; }

    internal Vector4 SaberBColor { get; set; }
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
        _colorBaseProvider.Note0Color = _leftHanded ? _colorScheme.saberBColor : _colorScheme.saberAColor;
        _colorBaseProvider.Note1Color = _leftHanded ? _colorScheme.saberAColor : _colorScheme.saberBColor;
        _colorBaseProvider.SaberAColor = _colorScheme.saberAColor;
        _colorBaseProvider.SaberBColor = _colorScheme.saberBColor;
        _colorBaseProvider.EnvironmentColor0 = _colorScheme.environmentColor0;
        _colorBaseProvider.EnvironmentColor1 = _colorScheme.environmentColor1;
        _colorBaseProvider.EnvironmentColorW = _colorScheme.environmentColorW;
        _colorBaseProvider.EnvironmentColor0Boost = _colorScheme.environmentColor0Boost;
        _colorBaseProvider.EnvironmentColor1Boost = _colorScheme.environmentColor1Boost;
        _colorBaseProvider.EnvironmentColorWBoost = _colorScheme.environmentColorWBoost;
        _colorBaseProvider.ObstaclesColor = _colorScheme.obstaclesColor;
    }
}
