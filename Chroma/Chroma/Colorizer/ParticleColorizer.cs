namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using IPA.Utilities;
    using UnityEngine;

    public class ParticleColorizer
    {
        // ParticleSystemEventEffect still doesn't support boost colors!
        private const int COLOR_FIELDS = 2;

        private static readonly FieldAccessor<MultipliedColorSO, Color>.Accessor _multiplierColorAccessor = FieldAccessor<MultipliedColorSO, Color>.GetAccessor("_multiplierColor");
        private static readonly FieldAccessor<MultipliedColorSO, SimpleColorSO>.Accessor _baseColorAccessor = FieldAccessor<MultipliedColorSO, SimpleColorSO>.GetAccessor("_baseColor");

        private static readonly FieldAccessor<ParticleSystemEventEffect, Color>.Accessor _particleColorAccessor = FieldAccessor<ParticleSystemEventEffect, Color>.GetAccessor("_particleColor");
        private static readonly FieldAccessor<ParticleSystemEventEffect, Color>.Accessor _offColorAccessor = FieldAccessor<ParticleSystemEventEffect, Color>.GetAccessor("_offColor");
        private static readonly FieldAccessor<ParticleSystemEventEffect, Color>.Accessor _highlightColorAccessor = FieldAccessor<ParticleSystemEventEffect, Color>.GetAccessor("_highlightColor");
        private static readonly FieldAccessor<ParticleSystemEventEffect, float>.Accessor _highlightValueAccessor = FieldAccessor<ParticleSystemEventEffect, float>.GetAccessor("_highlightValue");
        private static readonly FieldAccessor<ParticleSystemEventEffect, Color>.Accessor _afterHighlightColorAccessor = FieldAccessor<ParticleSystemEventEffect, Color>.GetAccessor("_afterHighlightColor");

        private readonly ParticleSystemEventEffect _particleSystemEventEffect;
        private readonly BeatmapEventType _eventType;

        private readonly SimpleColorSO[] _simpleColorSOs = new SimpleColorSO[COLOR_FIELDS];
        private readonly MultipliedColorSO[] _multipliedColorSOs = new MultipliedColorSO[COLOR_FIELDS];
        private readonly MultipliedColorSO[] _multipliedHighlightColorSOs = new MultipliedColorSO[COLOR_FIELDS];

        internal ParticleColorizer(ParticleSystemEventEffect particleSystemEventEffect, BeatmapEventType beatmapEventType)
        {
            _particleSystemEventEffect = particleSystemEventEffect;
            _eventType = beatmapEventType;
            InitializeSO("_lightColor0", 0);
            InitializeSO("_highlightColor0", 0, true);
            InitializeSO("_lightColor1", 1);
            InitializeSO("_highlightColor1", 1, true);

            Colorizers.Add(beatmapEventType, this);

            LightColorizer.LightColorChanged += OnLightColorChanged;
        }

        public static Dictionary<BeatmapEventType, ParticleColorizer> Colorizers { get; } = new Dictionary<BeatmapEventType, ParticleColorizer>();

        internal int PreviousValue { get; set; }

        internal void UnsubscribeEvent()
        {
            LightColorizer.LightColorChanged -= OnLightColorChanged;
        }

        private void OnLightColorChanged(BeatmapEventType eventType, Color[] colors)
        {
            if (eventType == _eventType)
            {
                for (int i = 0; i < COLOR_FIELDS; i++)
                {
                    _simpleColorSOs[i].SetColor(colors[i]);
                }

                ParticleSystemEventEffect particleSystemEventEffect = _particleSystemEventEffect;
                Color color;
                Color afterHighlightColor;
                switch (PreviousValue)
                {
                    case 0:
                        _particleColorAccessor(ref particleSystemEventEffect) = _offColorAccessor(ref particleSystemEventEffect);
                        particleSystemEventEffect.RefreshParticles();
                        break;

                    case 1:
                    case 5:
                        color = (PreviousValue == 1) ? _multipliedColorSOs[0] : _multipliedColorSOs[1];
                        _particleColorAccessor(ref particleSystemEventEffect) = color;
                        _offColorAccessor(ref particleSystemEventEffect) = color.ColorWithAlpha(0);
                        particleSystemEventEffect.RefreshParticles();
                        break;

                    case 2:
                    case 6:
                        color = (PreviousValue == 2) ? _multipliedHighlightColorSOs[0] : _multipliedHighlightColorSOs[1];
                        _highlightColorAccessor(ref particleSystemEventEffect) = color;
                        _offColorAccessor(ref particleSystemEventEffect) = color.ColorWithAlpha(0);
                        afterHighlightColor = (PreviousValue == 2) ? _multipliedColorSOs[0] : _multipliedColorSOs[1];
                        _afterHighlightColorAccessor(ref particleSystemEventEffect) = afterHighlightColor;

                        _particleColorAccessor(ref particleSystemEventEffect) = Color.Lerp(afterHighlightColor, color, _highlightValueAccessor(ref particleSystemEventEffect));
                        particleSystemEventEffect.RefreshParticles();
                        break;

                    case 3:
                    case 7:
                    case -1:
                        color = (PreviousValue == 3) ? _multipliedHighlightColorSOs[0] : _multipliedHighlightColorSOs[1];
                        _highlightColorAccessor(ref particleSystemEventEffect) = color;
                        _offColorAccessor(ref particleSystemEventEffect) = color.ColorWithAlpha(0);
                        _particleColorAccessor(ref particleSystemEventEffect) = color;
                        afterHighlightColor = _offColorAccessor(ref particleSystemEventEffect);
                        _afterHighlightColorAccessor(ref particleSystemEventEffect) = afterHighlightColor;

                        _particleColorAccessor(ref particleSystemEventEffect) = Color.Lerp(afterHighlightColor, color, _highlightValueAccessor(ref particleSystemEventEffect));
                        particleSystemEventEffect.RefreshParticles();
                        break;
                }
            }
        }

        private void InitializeSO(string id, int index, bool highlight = false)
        {
            ParticleSystemEventEffect particleSystemEventEffect = _particleSystemEventEffect;
            FieldAccessor<ParticleSystemEventEffect, ColorSO>.Accessor colorSOAcessor = FieldAccessor<ParticleSystemEventEffect, ColorSO>.GetAccessor(id);

            MultipliedColorSO lightMultSO = (MultipliedColorSO)colorSOAcessor(ref particleSystemEventEffect);

            Color multiplierColor = _multiplierColorAccessor(ref lightMultSO);
            SimpleColorSO lightSO = _baseColorAccessor(ref lightMultSO);

            MultipliedColorSO mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
            _multiplierColorAccessor(ref mColorSO) = multiplierColor;

            SimpleColorSO sColorSO;
            if (_simpleColorSOs[index] == null)
            {
                sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                sColorSO.SetColor(lightSO.color);
                _simpleColorSOs[index] = sColorSO;
            }
            else
            {
                sColorSO = _simpleColorSOs[index];
            }

            _baseColorAccessor(ref mColorSO) = sColorSO;

            if (highlight)
            {
                _multipliedHighlightColorSOs[index] = mColorSO;
            }
            else
            {
                _multipliedColorSOs[index] = mColorSO;
            }

            colorSOAcessor(ref particleSystemEventEffect) = mColorSO;
        }
    }
}
