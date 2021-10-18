namespace Chroma
{
    using Heck.Animation;
    using IPA.Utilities;
    using Tweening;
    using UnityEngine;

    public class ChromaIDColorTween : ColorTween
    {
        private static readonly FieldAccessor<LightWithIdManager, bool>.Accessor _didChangeAccessor = FieldAccessor<LightWithIdManager, bool>.GetAccessor("_didChangeSomeColorsThisFrame");

        private readonly ILightWithId _lightWithId;
        private LightWithIdManager _lightWithIdManager;

        internal ChromaIDColorTween(Color fromValue, Color toValue, ILightWithId lightWithId, LightWithIdManager lightWithIdManager, int id)
        {
            Reinit(fromValue, toValue, SetColor, 0, EaseType.Linear, 0);
            _lightWithId = lightWithId;
            _lightWithIdManager = lightWithIdManager;
            Id = id;
        }

        public int Id { get; }

        public BeatmapEventData? PreviousEvent { get; set; }

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
                    throw new System.InvalidOperationException($"[{nameof(LerpType)}] not valid: [{LerpType}].");
            }
        }

        public void SetColor(Color color)
        {
            _didChangeAccessor(ref _lightWithIdManager) = true;
            if (_lightWithId.isRegistered)
            {
                _lightWithId.ColorWasSet(color);
            }
        }
    }
}
