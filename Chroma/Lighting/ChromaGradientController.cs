using System.Collections.Generic;
using Chroma.Colorizer;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.Lighting
{
    [UsedImplicitly]
    internal class ChromaGradientController : ITickable
    {
        private readonly LightColorizerManager _manager;
        private readonly IBeatmapObjectSpawnController _spawnController;
        private readonly ChromaGradientEvent.Factory _factory;

        private ChromaGradientController(
            LightColorizerManager manager,
            IBeatmapObjectSpawnController spawnController,
            ChromaGradientEvent.Factory factory)
        {
            _manager = manager;
            _spawnController = spawnController;
            _factory = factory;
        }

        private IDictionary<BeatmapEventType, ChromaGradientEvent> Gradients { get; } = new Dictionary<BeatmapEventType, ChromaGradientEvent>();

        public void Tick()
        {
            foreach ((BeatmapEventType eventType, ChromaGradientEvent value) in new Dictionary<BeatmapEventType, ChromaGradientEvent>(Gradients))
            {
                Color color = value.Interpolate();
                _manager.Colorize(eventType, true, color, color, color, color);
            }
        }

        internal bool IsGradientActive(BeatmapEventType eventType)
        {
            return Gradients.ContainsKey(eventType);
        }

        internal void CancelGradient(BeatmapEventType eventType)
        {
            Gradients.Remove(eventType);
        }

        internal Color AddGradient(ChromaEventData.GradientObjectData gradientObject, BeatmapEventType id, float time)
        {
            CancelGradient(id);

            float duration = gradientObject.Duration;
            Color initcolor = gradientObject.StartColor;
            Color endcolor = gradientObject.EndColor;
            Functions easing = gradientObject.Easing;

            ChromaGradientEvent gradientEvent = _factory.Create(initcolor, endcolor, time, 60 * duration / _spawnController.currentBpm, id, easing);
            Gradients[id] = gradientEvent;
            return gradientEvent.Interpolate();
        }

        [UsedImplicitly]
        internal class ChromaGradientEvent
        {
            private readonly IAudioTimeSource _timeSource;
            private readonly ChromaGradientController _gradientController;
            private readonly Color _initcolor;
            private readonly Color _endcolor;
            private readonly float _start;
            private readonly float _duration;
            private readonly BeatmapEventType _event;
            private readonly Functions _easing;

            internal ChromaGradientEvent(
                IAudioTimeSource timeSource,
                ChromaGradientController gradientController,
                Color initcolor,
                Color endcolor,
                float start,
                float duration,
                BeatmapEventType eventType,
                Functions easing = Functions.easeLinear)
            {
                _timeSource = timeSource;
                _gradientController = gradientController;
                _initcolor = initcolor;
                _endcolor = endcolor;
                _start = start;
                _duration = duration;
                _event = eventType;
                _easing = easing;
            }

            internal Color Interpolate()
            {
                float normalTime = _timeSource.songTime - _start;
                if (normalTime < 0)
                {
                    return _initcolor;
                }

                if (normalTime <= _duration)
                {
                    return Color.LerpUnclamped(_initcolor, _endcolor, Easings.Interpolate(normalTime / _duration, _easing));
                }

                _gradientController.Gradients.Remove(_event);
                return _endcolor;
            }

            [UsedImplicitly]
            internal class Factory : PlaceholderFactory<Color, Color, float, float, BeatmapEventType, Functions, ChromaGradientEvent>
            {
            }
        }
    }
}
