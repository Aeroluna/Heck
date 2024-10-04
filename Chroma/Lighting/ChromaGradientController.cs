using System.Collections.Generic;
using Chroma.Colorizer;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.Lighting;

[UsedImplicitly]
internal class ChromaGradientController : ITickable
{
    private readonly IBpmController _bpmController;
    private readonly LightColorizerManager _manager;
    private readonly IAudioTimeSource _timeSource;
    private readonly List<BasicBeatmapEventType> _reusableEventTypes = [];

    private ChromaGradientController(
        IAudioTimeSource timeSource,
        LightColorizerManager manager,
        IBpmController bpmController)
    {
        _timeSource = timeSource;
        _manager = manager;
        _bpmController = bpmController;
    }

    private Dictionary<BasicBeatmapEventType, ChromaGradientEvent> Gradients { get; } = new();

    public void Tick()
    {
        foreach ((BasicBeatmapEventType eventType, ChromaGradientEvent value) in Gradients)
        {
            Color color = value.Interpolate(out bool onLast);
            _manager.Colorize(eventType, true, color, color, color, color);
            if (onLast)
            {
                _reusableEventTypes.Add(eventType);
            }
        }

        foreach (BasicBeatmapEventType basicBeatmapEventType in _reusableEventTypes)
        {
            Gradients.Remove(basicBeatmapEventType);
        }

        _reusableEventTypes.Clear();
    }

    internal Color AddGradient(ChromaEventData.GradientObjectData gradientObject, BasicBeatmapEventType id, float time)
    {
        CancelGradient(id);

        float duration = gradientObject.Duration;
        Color initColor = gradientObject.StartColor;
        Color endColor = gradientObject.EndColor;
        Functions easing = gradientObject.Easing;

        ChromaGradientEvent gradientEvent = new(
            _timeSource,
            initColor,
            endColor,
            time,
            (60 * duration) / _bpmController.currentBpm,
            easing);
        Color color = gradientEvent.Interpolate(out bool onLast);
        if (!onLast)
        {
            Gradients[id] = gradientEvent;
        }

        return color;
    }

    internal void CancelGradient(BasicBeatmapEventType eventType)
    {
        Gradients.Remove(eventType);
    }

    internal bool IsGradientActive(BasicBeatmapEventType eventType)
    {
        return Gradients.ContainsKey(eventType);
    }

    [UsedImplicitly]
    internal readonly struct ChromaGradientEvent
    {
        private readonly float _duration;
        private readonly Functions _easing;
        private readonly Color _endcolor;
        private readonly Color _initcolor;
        private readonly float _start;
        private readonly IAudioTimeSource _timeSource;

        internal ChromaGradientEvent(
            IAudioTimeSource timeSource,
            Color initcolor,
            Color endcolor,
            float start,
            float duration,
            Functions easing = Functions.easeLinear)
        {
            _timeSource = timeSource;
            _initcolor = initcolor;
            _endcolor = endcolor;
            _start = start;
            _duration = duration;
            _easing = easing;
        }

        internal Color Interpolate(out bool onLast)
        {
            float normalTime = _timeSource.songTime - _start;
            if (normalTime < 0)
            {
                onLast = false;
                return _initcolor;
            }

            if (normalTime <= _duration)
            {
                onLast = false;
                return Color.LerpUnclamped(_initcolor, _endcolor, Easings.Interpolate(normalTime / _duration, _easing));
            }

            onLast = true;
            return _endcolor;
        }
    }
}
