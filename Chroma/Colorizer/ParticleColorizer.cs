using System;
using System.Collections.Generic;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.Colorizer
{
    [UsedImplicitly]
    public class ParticleColorizerManager
    {
        private static readonly FieldAccessor<ParticleSystemEventEffect, BeatmapEventType>.Accessor _eventAccessor = FieldAccessor<ParticleSystemEventEffect, BeatmapEventType>.GetAccessor("_colorEvent");

        private readonly ParticleColorizer.Factory _factory;

        private ParticleColorizerManager(ParticleColorizer.Factory factory)
        {
            _factory = factory;
        }

        public Dictionary<BeatmapEventType, List<ParticleColorizer>> Colorizers { get; } = new();

        internal void Create(ParticleSystemEventEffect particleSystemEventEffect)
        {
            BeatmapEventType type = _eventAccessor(ref particleSystemEventEffect);
            if (!Colorizers.TryGetValue(type, out List<ParticleColorizer> colorizers))
            {
                colorizers = new List<ParticleColorizer>();
                Colorizers.Add(type, colorizers);
            }

            colorizers.Add(_factory.Create(particleSystemEventEffect));
        }
    }

    [UsedImplicitly]
    public sealed class ParticleColorizer : IDisposable
    {
        private static readonly FieldAccessor<ParticleSystemEventEffect, ColorSO>.Accessor _lightColor0Accessor = FieldAccessor<ParticleSystemEventEffect, ColorSO>.GetAccessor("_lightColor0");
        private static readonly FieldAccessor<ParticleSystemEventEffect, ColorSO>.Accessor _lightColor1Accessor = FieldAccessor<ParticleSystemEventEffect, ColorSO>.GetAccessor("_lightColor1");
        private static readonly FieldAccessor<ParticleSystemEventEffect, ColorSO>.Accessor _highlightColor0Accessor = FieldAccessor<ParticleSystemEventEffect, ColorSO>.GetAccessor("_highlightColor0");
        private static readonly FieldAccessor<ParticleSystemEventEffect, ColorSO>.Accessor _highlightColor1Accessor = FieldAccessor<ParticleSystemEventEffect, ColorSO>.GetAccessor("_highlightColor1");

        private static readonly FieldAccessor<ParticleSystemEventEffect, Color>.Accessor _particleColorAccessor = FieldAccessor<ParticleSystemEventEffect, Color>.GetAccessor("_particleColor");
        private static readonly FieldAccessor<ParticleSystemEventEffect, Color>.Accessor _offColorAccessor = FieldAccessor<ParticleSystemEventEffect, Color>.GetAccessor("_offColor");
        private static readonly FieldAccessor<ParticleSystemEventEffect, Color>.Accessor _highlightColorAccessor = FieldAccessor<ParticleSystemEventEffect, Color>.GetAccessor("_highlightColor");
        private static readonly FieldAccessor<ParticleSystemEventEffect, float>.Accessor _highlightValueAccessor = FieldAccessor<ParticleSystemEventEffect, float>.GetAccessor("_highlightValue");
        private static readonly FieldAccessor<ParticleSystemEventEffect, Color>.Accessor _afterHighlightColorAccessor = FieldAccessor<ParticleSystemEventEffect, Color>.GetAccessor("_afterHighlightColor");

        private static readonly FieldAccessor<ParticleSystemEventEffect, BeatmapEventType>.Accessor _eventAccessor = FieldAccessor<ParticleSystemEventEffect, BeatmapEventType>.GetAccessor("_colorEvent");

        private static readonly FieldAccessor<MultipliedColorSO, Color>.Accessor _multiplierColorAccessor = FieldAccessor<MultipliedColorSO, Color>.GetAccessor("_multiplierColor");

        private ParticleSystemEventEffect _particleSystemEventEffect;

        private MultipliedColorSO _lightColor0;
        private MultipliedColorSO _lightColor1;
        private MultipliedColorSO _highlightColor0;
        private MultipliedColorSO _highlightColor1;

        private LightColorizer? _lightColorizer;

        private int _previousValue;

        private ParticleColorizer(
            ParticleSystemEventEffect particleSystemEventEffect,
            LightColorizerManager lightColorizerManager)
        {
            _particleSystemEventEffect = particleSystemEventEffect;
            _lightColor0 = (MultipliedColorSO)_lightColor0Accessor(ref particleSystemEventEffect);
            _lightColor1 = (MultipliedColorSO)_lightColor1Accessor(ref particleSystemEventEffect);
            _highlightColor0 = (MultipliedColorSO)_highlightColor0Accessor(ref particleSystemEventEffect);
            _highlightColor1 = (MultipliedColorSO)_highlightColor1Accessor(ref particleSystemEventEffect);

            // not sure when the light colorizer will be made...
            lightColorizerManager.CreateLightColorizerContract(_eventAccessor(ref particleSystemEventEffect), AssignLightColorizer);
        }

        private LightColorizer FollowedColorizer => _lightColorizer ?? throw new InvalidOperationException($"{nameof(_lightColorizer)} was null.");

        public void Dispose()
        {
            if (_lightColorizer == null)
            {
                return;
            }

            _lightColorizer.ChromaLightSwitchEventEffect.BeatmapEventDidTrigger -= Callback;
            _lightColorizer.ChromaLightSwitchEventEffect.DidRefresh -= Refresh;
        }

        // Day 124789 of particles not having color boost code
        public void Refresh()
        {
            Color color;
            Color afterHighlightColor;
            switch (_previousValue)
            {
                case 0:
                    _particleColorAccessor(ref _particleSystemEventEffect) = _offColorAccessor(ref _particleSystemEventEffect);
                    _particleSystemEventEffect.RefreshParticles();
                    break;

                case 1:
                case 5:
                    color = GetNormalColor(_previousValue);
                    _particleColorAccessor(ref _particleSystemEventEffect) = color;
                    _offColorAccessor(ref _particleSystemEventEffect) = color.ColorWithAlpha(0);
                    _particleSystemEventEffect.RefreshParticles();
                    break;

                case 2:
                case 6:
                    color = GetHighlightColor(_previousValue);
                    _highlightColorAccessor(ref _particleSystemEventEffect) = color;
                    _offColorAccessor(ref _particleSystemEventEffect) = color.ColorWithAlpha(0);
                    afterHighlightColor = GetNormalColor(_previousValue);
                    _afterHighlightColorAccessor(ref _particleSystemEventEffect) = afterHighlightColor;

                    _particleColorAccessor(ref _particleSystemEventEffect) = Color.Lerp(afterHighlightColor, color, _highlightValueAccessor(ref _particleSystemEventEffect));
                    _particleSystemEventEffect.RefreshParticles();
                    break;

                case 3:
                case 7:
                case -1:
                    color = GetHighlightColor(_previousValue);
                    _highlightColorAccessor(ref _particleSystemEventEffect) = color;
                    _offColorAccessor(ref _particleSystemEventEffect) = color.ColorWithAlpha(0);
                    _particleColorAccessor(ref _particleSystemEventEffect) = color;
                    afterHighlightColor = _offColorAccessor(ref _particleSystemEventEffect);
                    _afterHighlightColorAccessor(ref _particleSystemEventEffect) = afterHighlightColor;

                    _particleColorAccessor(ref _particleSystemEventEffect) = Color.Lerp(afterHighlightColor, color, _highlightValueAccessor(ref _particleSystemEventEffect));
                    _particleSystemEventEffect.RefreshParticles();
                    break;
            }
        }

        public Color GetNormalColor(int beatmapEventValue)
        {
            if (!IsColor0(beatmapEventValue))
            {
                return FollowedColorizer.Color[1] * _multiplierColorAccessor(ref _lightColor1);
            }

            return FollowedColorizer.Color[0] * _multiplierColorAccessor(ref _lightColor0);
        }

        public Color GetHighlightColor(int beatmapEventValue)
        {
            if (!IsColor0(beatmapEventValue))
            {
                return FollowedColorizer.Color[1] * _multiplierColorAccessor(ref _highlightColor1);
            }

            return FollowedColorizer.Color[0] * _multiplierColorAccessor(ref _highlightColor0);
        }

        private static bool IsColor0(int value)
        {
            return value is 1 or 2 or 3 or 4 or 0 or -1;
        }

        private void AssignLightColorizer(LightColorizer lightColorizer)
        {
            _lightColorizer = lightColorizer;
            lightColorizer.ChromaLightSwitchEventEffect.BeatmapEventDidTrigger += Callback;
            lightColorizer.ChromaLightSwitchEventEffect.DidRefresh += Refresh;
        }

        private void Callback(BeatmapEventData beatmapEventData)
        {
            _previousValue = beatmapEventData.value;
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<ParticleSystemEventEffect, ParticleColorizer>
        {
        }
    }
}
