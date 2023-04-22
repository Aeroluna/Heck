using Heck;
using Heck.BaseProvider;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma
{
    internal class ColorBaseProvider : IBaseProvider
    {
        [BaseProvider("baseNote0Color")]
        internal Vector4 Note0Color { get; set; }

        [BaseProvider("baseNote1Color")]
        internal Vector4 Note1Color { get; set; }

        [BaseProvider("baseSaberAColor")]
        internal Vector4 SaberAColor { get; set; }

        [BaseProvider("baseSaberBColor")]
        internal Vector4 SaberBColor { get; set; }

        [BaseProvider("baseEnvironmentColor0")]
        internal Vector4 EnvironmentColor0 { get; set; }

        [BaseProvider("baseEnvironmentColor1")]
        internal Vector4 EnvironmentColor1 { get; set; }

        [BaseProvider("baseEnvironmentColorW")]
        internal Vector4 EnvironmentColorW { get; set; }

        [BaseProvider("baseEnvironmentColor0Boost")]
        internal Vector4 EnvironmentColor0Boost { get; set; }

        [BaseProvider("baseEnvironmentColor1Boost")]
        internal Vector4 EnvironmentColor1Boost { get; set; }

        [BaseProvider("baseEnvironmentColorWBoost")]
        internal Vector4 EnvironmentColorWBoost { get; set; }

        [BaseProvider("baseObstaclesColor")]
        internal Vector4 ObstaclesColor { get; set; }
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
            [Inject(Id = HeckController.LEFT_HANDED_ID)] bool leftHanded)
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
}
