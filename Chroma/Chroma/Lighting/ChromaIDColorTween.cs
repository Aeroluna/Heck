namespace Chroma
{
    using IPA.Utilities;
    using Tweening;
    using UnityEngine;

    public class ChromaIDColorTween : ColorTween
    {
        private static readonly FieldAccessor<LightWithIdManager, bool>.Accessor _didChangeAccessor = FieldAccessor<LightWithIdManager, bool>.GetAccessor("_didChangeSomeColorsThisFrame");

        private readonly ILightWithId _lightWithId;
        private LightWithIdManager _lightWithIdManager;

        internal ChromaIDColorTween(Color fromValue, Color toValue, ILightWithId lightWithId, LightWithIdManager lightWithIdManager)
        {
            Reinit(fromValue, toValue, SetColor, 0, EaseType.Linear, 0);
            _lightWithId = lightWithId;
            _lightWithIdManager = lightWithIdManager;
        }

        public BeatmapEventData? PreviousEvent { get; set; }

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
