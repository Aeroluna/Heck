using System;
using Heck.Animation;
using Tweening;
using UnityEngine;

namespace Chroma.Lighting
{
    public sealed class ChromaIDColorTween : ColorTween
    {
        private readonly ILightWithId _lightWithId;
        private readonly LightWithIdManager _lightWithIdManager;

        internal ChromaIDColorTween(Color fromValue, Color toValue, ILightWithId lightWithId, LightWithIdManager lightWithIdManager, int id)
        {
            Init(fromValue, toValue, SetColor, 0, EaseType.Linear);
            _lightWithId = lightWithId;
            _lightWithIdManager = lightWithIdManager;
            Id = id;
        }

        public int Id { get; }

        public BasicBeatmapEventData? PreviousEvent { get; set; }

        public Functions HeckEasing { get; set; }

        public LerpType LerpType { get; set; }

        public override Color GetValue(float time)
        {
            time = Easings.Interpolate(time, HeckEasing);
            switch (LerpType)
            {
                case LerpType.RGB:
                    return Color.LerpUnclamped(fromValue, toValue, time);

                case LerpType.HSV:
                    Color.RGBToHSV(fromValue, out float fromH, out float fromS, out float fromV);
                    Color.RGBToHSV(toValue, out float toH, out float toS, out float toV);
                    return Color.HSVToRGB(Mathf.LerpUnclamped(fromH, toH, time), Mathf.LerpUnclamped(fromS, toS, time), Mathf.LerpUnclamped(fromV, toV, time)).ColorWithAlpha(Mathf.LerpUnclamped(fromValue.a, toValue.a, time));

                default:
                    throw new InvalidOperationException($"[{nameof(LerpType)}] not valid: [{LerpType}].");
            }
        }

        public void SetColor(Color color)
        {
            if (!_lightWithId.isRegistered)
            {
                return;
            }

            _lightWithIdManager._didChangeSomeColorsThisFrame = true;
            _lightWithId.ColorWasSet(color);
        }
    }
}
