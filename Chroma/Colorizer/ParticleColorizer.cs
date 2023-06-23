using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.Colorizer
{
    [UsedImplicitly]
    public sealed class ParticleColorizerManager
    {
        private readonly ParticleColorizer.Factory _factory;

        private ParticleColorizerManager(ParticleColorizer.Factory factory)
        {
            _factory = factory;
        }

        public Dictionary<BasicBeatmapEventType, List<ParticleColorizer>> Colorizers { get; } = new();

        internal void Create(ParticleSystemEventEffect particleSystemEventEffect)
        {
            BasicBeatmapEventType type = particleSystemEventEffect._colorEvent;
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
        private readonly ParticleSystemEventEffect _particleSystemEventEffect;

        private readonly MultipliedColorSO _lightColor0;
        private readonly MultipliedColorSO _lightColor1;
        private readonly MultipliedColorSO _highlightColor0;
        private readonly MultipliedColorSO _highlightColor1;

        private LightColorizer? _lightColorizer;

        private int _previousValue;

        private ParticleColorizer(
            ParticleSystemEventEffect particleSystemEventEffect,
            LightColorizerManager lightColorizerManager)
        {
            _particleSystemEventEffect = particleSystemEventEffect;
            _lightColor0 = (MultipliedColorSO)particleSystemEventEffect._lightColor0;
            _lightColor1 = (MultipliedColorSO)particleSystemEventEffect._lightColor1;
            _highlightColor0 = (MultipliedColorSO)particleSystemEventEffect._highlightColor0;
            _highlightColor1 = (MultipliedColorSO)particleSystemEventEffect._highlightColor1;

            // not sure when the light colorizer will be made...
            lightColorizerManager.CreateLightColorizerContract(particleSystemEventEffect._colorEvent, AssignLightColorizer);
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
                    _particleSystemEventEffect._particleColor = _particleSystemEventEffect._offColor;
                    _particleSystemEventEffect.RefreshParticles();
                    break;

                case 1:
                case 5:
                    color = GetNormalColor(_previousValue);
                    _particleSystemEventEffect._particleColor = color;
                    _particleSystemEventEffect._offColor = color.ColorWithAlpha(0);
                    _particleSystemEventEffect.RefreshParticles();
                    break;

                case 2:
                case 6:
                    color = GetHighlightColor(_previousValue);
                    _particleSystemEventEffect._highlightColor = color;
                    _particleSystemEventEffect._offColor = color.ColorWithAlpha(0);
                    afterHighlightColor = GetNormalColor(_previousValue);
                    _particleSystemEventEffect._afterHighlightColor = afterHighlightColor;

                    _particleSystemEventEffect._particleColor = Color.Lerp(afterHighlightColor, color, _particleSystemEventEffect._highlightValue);
                    _particleSystemEventEffect.RefreshParticles();
                    break;

                case 3:
                case 7:
                case -1:
                    color = GetHighlightColor(_previousValue);
                    _particleSystemEventEffect._highlightColor = color;
                    _particleSystemEventEffect._offColor = color.ColorWithAlpha(0);
                    _particleSystemEventEffect._particleColor = color;
                    afterHighlightColor = _particleSystemEventEffect._offColor;
                    _particleSystemEventEffect._afterHighlightColor = afterHighlightColor;

                    _particleSystemEventEffect._particleColor = Color.Lerp(afterHighlightColor, color, _particleSystemEventEffect._highlightValue);
                    _particleSystemEventEffect.RefreshParticles();
                    break;
            }
        }

        public Color GetNormalColor(int beatmapEventValue)
        {
            if (!IsColor0(beatmapEventValue))
            {
                return FollowedColorizer.Color[1] * _lightColor1._multiplierColor;
            }

            return FollowedColorizer.Color[0] * _lightColor0._multiplierColor;
        }

        public Color GetHighlightColor(int beatmapEventValue)
        {
            if (!IsColor0(beatmapEventValue))
            {
                return FollowedColorizer.Color[1] * _highlightColor1._multiplierColor;
            }

            return FollowedColorizer.Color[0] * _highlightColor0._multiplierColor;
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

        private void Callback(BasicBeatmapEventData beatmapEventData)
        {
            _previousValue = beatmapEventData.value;
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<ParticleSystemEventEffect, ParticleColorizer>
        {
        }
    }
}
