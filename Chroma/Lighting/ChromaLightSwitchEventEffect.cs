using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Colorizer;
using Chroma.Extras;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using Tweening;
using UnityEngine;
using Zenject;

namespace Chroma.Lighting
{
    public enum LerpType
    {
        RGB,
        HSV
    }

    [UsedImplicitly]
    public sealed class ChromaLightSwitchEventEffect : IDisposable
    {
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _lightColor0Accessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_lightColor0");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _lightColor1Accessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_lightColor1");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _highlightColor0Accessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_highlightColor0");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _highlightColor1Accessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_highlightColor1");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _lightColor0BoostAccessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_lightColor0Boost");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _lightColor1BoostAccessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_lightColor1Boost");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _highlightColor0BoostAccessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_highlightColor0Boost");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _highlightColor1BoostAccessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_highlightColor1Boost");
        private static readonly FieldAccessor<LightSwitchEventEffect, float>.Accessor _offColorIntensityAccessor = FieldAccessor<LightSwitchEventEffect, float>.GetAccessor("_offColorIntensity");
        private static readonly FieldAccessor<LightSwitchEventEffect, BeatmapEventType>.Accessor _eventAccessor = FieldAccessor<LightSwitchEventEffect, BeatmapEventType>.GetAccessor("_event");
        private static readonly FieldAccessor<LightSwitchEventEffect, BeatmapEventType>.Accessor _colorBoostEventAccessor = FieldAccessor<LightSwitchEventEffect, BeatmapEventType>.GetAccessor("_colorBoostEvent");
        private static readonly FieldAccessor<LightSwitchEventEffect, bool>.Accessor _lightOnStartAccessor = FieldAccessor<LightSwitchEventEffect, bool>.GetAccessor("_lightOnStart");

        private static readonly FieldAccessor<MultipliedColorSO, Color>.Accessor _multiplierColorAccessor = FieldAccessor<MultipliedColorSO, Color>.GetAccessor("_multiplierColor");

        private readonly LightWithIdManager _lightManager;
        private readonly SongTimeTweeningManager _tweeningManager;
        private readonly IBeatmapObjectCallbackController _beatmapObjectCallbackController;
        private readonly CustomData _customData;
        private readonly ChromaGradientController? _gradientController;
        private readonly LegacyLightHelper _legacyLightHelper;

        private readonly BeatmapEventType _colorBoostEvent;
        private readonly float _offColorIntensity;
        private readonly bool _lightOnStart;

        private MultipliedColorSO _lightColor0;
        private MultipliedColorSO _lightColor1;
        private MultipliedColorSO _highlightColor0;
        private MultipliedColorSO _highlightColor1;
        private MultipliedColorSO _lightColor0Boost;
        private MultipliedColorSO _lightColor1Boost;
        private MultipliedColorSO _highlightColor0Boost;
        private MultipliedColorSO _highlightColor1Boost;

        private bool _usingBoostColors;

        private ChromaLightSwitchEventEffect(
            LightSwitchEventEffect lightSwitchEventEffect,
            LightWithIdManager lightManager,
            SongTimeTweeningManager tweeningManager,
            LightColorizerManager lightColorizerManager,
            IBeatmapObjectCallbackController beatmapObjectCallbackController,
            [Inject(Id = ChromaController.ID)] CustomData customData,
            [InjectOptional] ChromaGradientController? gradientController,
            LegacyLightHelper legacyLightHelper)
        {
            LightSwitchEventEffect = lightSwitchEventEffect;
            _lightManager = lightManager;
            _tweeningManager = tweeningManager;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
            _customData = customData;
            _gradientController = gradientController;
            _legacyLightHelper = legacyLightHelper;

            EventType = _eventAccessor(ref lightSwitchEventEffect);
            _colorBoostEvent = _colorBoostEventAccessor(ref lightSwitchEventEffect);
            _offColorIntensity = _offColorIntensityAccessor(ref lightSwitchEventEffect);
            _lightOnStart = _lightOnStartAccessor(ref lightSwitchEventEffect);

            _lightColor0 = (MultipliedColorSO)_lightColor0Accessor(ref lightSwitchEventEffect);
            _lightColor1 = (MultipliedColorSO)_lightColor1Accessor(ref lightSwitchEventEffect);
            _highlightColor0 = (MultipliedColorSO)_highlightColor0Accessor(ref lightSwitchEventEffect);
            _highlightColor1 = (MultipliedColorSO)_highlightColor1Accessor(ref lightSwitchEventEffect);
            _lightColor0Boost = (MultipliedColorSO)_lightColor0BoostAccessor(ref lightSwitchEventEffect);
            _lightColor1Boost = (MultipliedColorSO)_lightColor1BoostAccessor(ref lightSwitchEventEffect);
            _highlightColor0Boost = (MultipliedColorSO)_highlightColor0BoostAccessor(ref lightSwitchEventEffect);
            _highlightColor1Boost = (MultipliedColorSO)_highlightColor1BoostAccessor(ref lightSwitchEventEffect);

            Colorizer = lightColorizerManager.Create(this);
            lightColorizerManager.CompleteContracts(this);

            beatmapObjectCallbackController.beatmapEventDidTriggerEvent += Callback;
        }

        public event Action<BeatmapEventData>? BeatmapEventDidTrigger;

        public event Action? DidRefresh;

        public BeatmapEventType EventType { get; }

        public LightSwitchEventEffect LightSwitchEventEffect { get; }

        public Dictionary<ILightWithId, ChromaIDColorTween> ColorTweens { get; } = new();

        public LightColorizer Colorizer { get; }

        public static bool IsColor0(int value)
        {
            return value is 1 or 2 or 3 or 4 or 0 or -1;
        }

        public void Dispose()
        {
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= Callback;
        }

        public Color GetNormalColor(int beatmapEventValue)
        {
            if (_usingBoostColors)
            {
                if (!IsColor0(beatmapEventValue))
                {
                    return Colorizer.Color[3] * _multiplierColorAccessor(ref _lightColor1Boost);
                }

                return Colorizer.Color[2] * _multiplierColorAccessor(ref _lightColor0Boost);
            }

            if (!IsColor0(beatmapEventValue))
            {
                return Colorizer.Color[1] * _multiplierColorAccessor(ref _lightColor1);
            }

            return Colorizer.Color[0] * _multiplierColorAccessor(ref _lightColor0);
        }

        public Color GetHighlightColor(int beatmapEventValue)
        {
            if (_usingBoostColors)
            {
                if (!IsColor0(beatmapEventValue))
                {
                    return Colorizer.Color[3] * _multiplierColorAccessor(ref _highlightColor1Boost);
                }

                return Colorizer.Color[2] * _multiplierColorAccessor(ref _highlightColor0Boost);
            }

            if (!IsColor0(beatmapEventValue))
            {
                return Colorizer.Color[1] * _multiplierColorAccessor(ref _highlightColor1);
            }

            return Colorizer.Color[0] * _multiplierColorAccessor(ref _highlightColor0);
        }

        public void Refresh(bool hard, IEnumerable<ILightWithId>? selectLights, BeatmapEventData? beatmapEventData = null, Functions? easing = null, LerpType? lerpType = null)
        {
            IEnumerable<ChromaIDColorTween> selectTweens = selectLights == null ? ColorTweens.Values
                : selectLights.Where(n => ColorTweens.ContainsKey(n)).Select(n => ColorTweens[n]);

            foreach (ChromaIDColorTween tween in selectTweens)
            {
                BeatmapEventData previousEvent;
                if (hard)
                {
                    tween.PreviousEvent = beatmapEventData ?? throw new ArgumentNullException(nameof(beatmapEventData), "Argument must not be null for hard refresh.");
                    previousEvent = beatmapEventData;
                }
                else
                {
                    if (tween.PreviousEvent == null)
                    {
                        // No previous event loaded, cant refresh.
                        return;
                    }

                    previousEvent = tween.PreviousEvent;
                }

                int previousValue = previousEvent.value;
                float previousFloatValue = previousEvent.floatValue;

                // this code is UGLY
                void CheckNextEventForFadeBetter()
                {
                    _customData.Resolve(previousEvent, out ChromaEventData? eventData);
                    Dictionary<int, BeatmapEventData>? nextSameTypesDict = eventData?.NextSameTypeEvent;
                    BeatmapEventData? nextSameTypeEvent;
                    if (ChromaController.FeaturesPatcher.Enabled && (nextSameTypesDict?.ContainsKey(tween.Id) ?? false))
                    {
                        nextSameTypeEvent = nextSameTypesDict[tween.Id];
                    }
                    else
                    {
                        nextSameTypeEvent = previousEvent.nextSameTypeEvent;
                    }

                    if (nextSameTypeEvent is not { value: 4 or 8 })
                    {
                        return;
                    }

                    float nextFloatValue = nextSameTypeEvent.floatValue;
                    int nextValue = nextSameTypeEvent.value;
                    Color nextColor;

                    _customData.Resolve(nextSameTypeEvent, out ChromaEventData? nextEventData);
                    Color? nextColorData = nextEventData?.ColorData;
                    if (ChromaController.FeaturesPatcher.Enabled && nextColorData.HasValue)
                    {
                        Color multiplierColor;
                        if (_usingBoostColors)
                        {
                            if (!IsColor0(nextValue))
                            {
                                multiplierColor = _multiplierColorAccessor(ref _highlightColor1Boost);
                            }

                            multiplierColor = _multiplierColorAccessor(ref _highlightColor0Boost);
                        }
                        else
                        {
                            if (!IsColor0(nextValue))
                            {
                                multiplierColor = _multiplierColorAccessor(ref _highlightColor1);
                            }

                            multiplierColor = _multiplierColorAccessor(ref _highlightColor0);
                        }

                        nextColor = nextColorData.Value * multiplierColor;
                    }
                    else
                    {
                        nextColor = LightSwitchEventEffect.GetNormalColor(nextValue, _usingBoostColors);
                    }

                    nextColor = nextColor.MultAlpha(nextFloatValue);
                    Color prevColor = tween.toValue;
                    if (previousValue == 0)
                    {
                        prevColor = nextColor.ColorWithAlpha(0f);
                    }
                    else if (previousValue is not (2 or 6 or 3 or 7 or -1))
                    {
                        prevColor = GetNormalColor(previousValue).MultAlpha(previousFloatValue);
                    }

                    tween.fromValue = prevColor;
                    tween.toValue = nextColor;
                    tween.ForceOnUpdate();

                    if (!hard)
                    {
                        return;
                    }

                    tween.SetStartTimeAndEndTime(previousEvent.time, nextSameTypeEvent.time);
                    tween.HeckEasing = easing ?? Functions.easeLinear;
                    tween.LerpType = lerpType ?? LerpType.RGB;
                    _tweeningManager.ResumeTween(tween, LightSwitchEventEffect);
                }

                switch (previousValue)
                {
                    case 0:
                        {
                            if (hard)
                            {
                                tween.Kill();
                            }

                            // we just always default color0
                            float offAlpha = _offColorIntensity * previousFloatValue;
                            Color color = GetNormalColor(0).ColorWithAlpha(offAlpha);
                            tween.fromValue = color;
                            tween.toValue = color;
                            tween.SetColor(color);
                            CheckNextEventForFadeBetter();
                        }

                        break;

                    case 1:
                    case 5:
                    case 4:
                    case 8:
                        {
                            if (hard)
                            {
                                tween.Kill();
                            }

                            Color color = GetNormalColor(previousValue).MultAlpha(previousFloatValue);
                            tween.fromValue = color;
                            tween.toValue = color;
                            tween.SetColor(color);
                            CheckNextEventForFadeBetter();
                        }

                        break;

                    case 2:
                    case 6:
                        {
                            Color colorFrom = GetHighlightColor(previousValue).MultAlpha(previousFloatValue);
                            Color colorTo = GetNormalColor(previousValue).MultAlpha(previousFloatValue);
                            tween.fromValue = colorFrom;
                            tween.toValue = colorTo;
                            tween.ForceOnUpdate();

                            if (hard)
                            {
                                tween.duration = 0.6f;
                                tween.HeckEasing = easing ?? Functions.easeOutCubic;
                                tween.LerpType = lerpType ?? LerpType.RGB;
                                _tweeningManager.RestartTween(tween, LightSwitchEventEffect);
                            }
                        }

                        break;

                    case 3:
                    case 7:
                    case -1:
                        {
                            Color colorFrom = GetHighlightColor(previousValue).MultAlpha(previousFloatValue);
                            Color colorTo = GetNormalColor(previousValue).ColorWithAlpha(_offColorIntensity * previousFloatValue);
                            tween.fromValue = colorFrom;
                            tween.toValue = colorTo;
                            tween.ForceOnUpdate();

                            if (hard)
                            {
                                tween.duration = 1.5f;
                                tween.HeckEasing = easing ?? Functions.easeOutExpo;
                                tween.LerpType = lerpType ?? LerpType.RGB;
                                _tweeningManager.RestartTween(tween, LightSwitchEventEffect);
                            }
                        }

                        break;
                }
            }

            DidRefresh?.Invoke();
        }

        internal void RegisterLight(ILightWithId lightWithId, int id)
        {
            if (!ColorTweens.ContainsKey(lightWithId))
            {
                Color color = GetNormalColor(0);
                if (!_lightOnStart)
                {
                    color = color.ColorWithAlpha(_offColorIntensity);
                }

                ChromaIDColorTween tween = new(
                    color,
                    color,
                    lightWithId,
                    _lightManager,
                    LightIDTableManager.GetActiveTableValueReverse((int)EventType, id) ?? 0);

                ColorTweens[lightWithId] = tween;
                tween.ForceOnUpdate();
            }
            else
            {
                Log.Logger.Log("Attempted to register duplicate ILightWithId.", IPA.Logging.Logger.Level.Error);
            }
        }

        private void Callback(BeatmapEventData beatmapEventData)
        {
            if (beatmapEventData.type == EventType)
            {
                IEnumerable<ILightWithId>? selectLights = null;
                Functions? easing = null;
                LerpType? lerpType = null;

                // fun fun chroma stuff
                if (ChromaController.FeaturesPatcher.Enabled)
                {
                    if (_gradientController == null)
                    {
                        throw new InvalidOperationException("Chroma Features requires the gradient controller.");
                    }

                    if (_customData.Resolve(beatmapEventData, out ChromaEventData? chromaData))
                    {
                        Color? color = null;

                        // legacy was a mistake
                        color = _legacyLightHelper.GetLegacyColor(beatmapEventData) ?? color;

                        if (chromaData.LightID != null)
                        {
                            selectLights = Colorizer.GetLightWithIds(chromaData.LightID);
                        }

                        // propID is now DEPRECATED!!!!!!!!
                        object? propID = chromaData.PropID;
                        if (propID != null)
                        {
                            selectLights = propID switch
                            {
                                List<object> propIDobjects => Colorizer.GetPropagationLightWithIds(
                                    propIDobjects.Select(Convert.ToInt32)),
                                long propIDlong => Colorizer.GetPropagationLightWithIds(new[] { (int)propIDlong }),
                                _ => selectLights
                            };
                        }

                        // fck gradients
                        ChromaEventData.GradientObjectData? gradientObject = chromaData.GradientObject;
                        if (gradientObject != null)
                        {
                            color = _gradientController.AddGradient(gradientObject, beatmapEventData.type, beatmapEventData.time);
                        }

                        Color? colorData = chromaData.ColorData;
                        if (colorData.HasValue)
                        {
                            color = colorData;
                            _gradientController.CancelGradient(beatmapEventData.type);
                        }

                        if (color.HasValue)
                        {
                            Color finalColor = color.Value;
                            Colorizer.Colorize(false, finalColor, finalColor, finalColor, finalColor);
                        }
                        else if (!_gradientController.IsGradientActive(beatmapEventData.type))
                        {
                            Colorizer.Colorize(false, null, null, null, null);
                        }

                        easing = chromaData.Easing;
                        lerpType = chromaData.LerpType;
                    }
                }

                // Particle colorizer cant use BeatmapObjectCallbackController event because the LightSwitchEventEffect must activate first
                BeatmapEventDidTrigger?.Invoke(beatmapEventData);

                Refresh(true, selectLights, beatmapEventData, easing, lerpType);
            }
            else if (beatmapEventData.type == _colorBoostEvent)
            {
                bool flag = beatmapEventData.value == 1;
                if (flag == _usingBoostColors)
                {
                    return;
                }

                _usingBoostColors = flag;
                Refresh(false, null);
            }
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<LightSwitchEventEffect, ChromaLightSwitchEventEffect>
        {
        }
    }
}
