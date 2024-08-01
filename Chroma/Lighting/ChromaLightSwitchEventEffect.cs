using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Colorizer;
using Chroma.Extras;
using Chroma.Modules;
using Heck.Animation;
using Heck.Deserialize;
using JetBrains.Annotations;
using SiraUtil.Logging;
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

    public sealed class ChromaLightSwitchEventEffect : IDisposable
    {
        private readonly SiraLog _log;
        private readonly LightWithIdManager _lightManager;
        private readonly LightIDTableManager _tableManager;
        private readonly SongTimeTweeningManager _tweeningManager;
        private readonly BeatmapCallbacksController _callbacksController;
        private readonly ColorManager _colorManager;
        private readonly DeserializedData _deserializedData;
        private readonly ChromaGradientController? _gradientController;
        private readonly FeaturesModule _featuresModule;

        private readonly BeatmapDataCallbackWrapper _basicCallbackWrapper;
        private readonly BeatmapDataCallbackWrapper _boostCallbackWrapper;

        private readonly float _offColorIntensity;
        private readonly bool _lightOnStart;

        private readonly Color _lightColor0Mult;
        private readonly Color _lightColor1Mult;
        private readonly Color _highlightColor0Mult;
        private readonly Color _highlightColor1Mult;
        private readonly Color _lightColor0BoostMult;
        private readonly Color _lightColor1BoostMult;
        private readonly Color _highlightColor0BoostMult;
        private readonly Color _highlightColor1BoostMult;

        private bool _usingBoostColors;

        [UsedImplicitly]
        private ChromaLightSwitchEventEffect(
            SiraLog log,
            LightSwitchEventEffect lightSwitchEventEffect,
            LightWithIdManager lightManager,
            LightIDTableManager tableManager,
            SongTimeTweeningManager tweeningManager,
            LightColorizerManager lightColorizerManager,
            BeatmapCallbacksController callbacksController,
            ColorManager colorManager,
            [Inject(Id = ChromaController.ID)] DeserializedData deserializedData,
            [InjectOptional] ChromaGradientController? gradientController,
            FeaturesModule featuresModule)
        {
            LightSwitchEventEffect = lightSwitchEventEffect;
            _log = log;
            _lightManager = lightManager;
            _tableManager = tableManager;
            _tweeningManager = tweeningManager;
            _callbacksController = callbacksController;
            _colorManager = colorManager;
            _deserializedData = deserializedData;
            _gradientController = gradientController;
            _featuresModule = featuresModule;

            EventType = lightSwitchEventEffect._event;
            LightsID = lightSwitchEventEffect._lightsID;
            _offColorIntensity = lightSwitchEventEffect._offColorIntensity;
            _lightOnStart = lightSwitchEventEffect._lightOnStart;

            Initialize(lightSwitchEventEffect._lightColor0, ref _lightColor0Mult);
            Initialize(lightSwitchEventEffect._lightColor1, ref _lightColor1Mult);
            Initialize(lightSwitchEventEffect._highlightColor0, ref _highlightColor0Mult);
            Initialize(lightSwitchEventEffect._highlightColor1, ref _highlightColor1Mult);
            Initialize(lightSwitchEventEffect._lightColor0Boost, ref _lightColor0BoostMult);
            Initialize(lightSwitchEventEffect._lightColor1Boost, ref _lightColor1BoostMult);
            Initialize(lightSwitchEventEffect._highlightColor0Boost, ref _highlightColor0BoostMult);
            Initialize(lightSwitchEventEffect._highlightColor1Boost, ref _highlightColor1BoostMult);

            Colorizer = lightColorizerManager.Create(this);
            lightColorizerManager.CompleteContracts(this);

            _basicCallbackWrapper = callbacksController.AddBeatmapCallback<BasicBeatmapEventData>(BasicCallback, BasicBeatmapEventData.SubtypeIdentifier(EventType));
            _boostCallbackWrapper = callbacksController.AddBeatmapCallback<ColorBoostBeatmapEventData>(BoostCallback);
            return;

            void Initialize(ColorSO colorSO, ref Color color)
            {
                color = colorSO switch
                {
                    MultipliedColorSO lightMultSO => lightMultSO._multiplierColor,
                    SimpleColorSO => Color.white,
                    _ => throw new InvalidOperationException($"Unhandled ColorSO type: [{colorSO.GetType().Name}].")
                };
            }
        }

        public event Action<BasicBeatmapEventData>? BeatmapEventDidTrigger;

        public event Action? DidRefresh;

        public BasicBeatmapEventType EventType { get; }

        public int LightsID { get; }

        public LightSwitchEventEffect LightSwitchEventEffect { get; }

        public Dictionary<ILightWithId, ChromaIDColorTween> ColorTweens { get; } = new();

        public LightColorizer Colorizer { get; }

        public void Dispose()
        {
            _callbacksController.RemoveBeatmapCallback(_basicCallbackWrapper);
            _callbacksController.RemoveBeatmapCallback(_boostCallbackWrapper);
        }

        public Color GetNormalColor(int beatmapEventValue)
        {
            switch (BeatmapEventDataLightsExtensions.GetLightColorTypeFromEventDataValue(beatmapEventValue))
            {
                ////case EnvironmentColorType.Color0:
                default:
                    if (_usingBoostColors)
                    {
                        return Colorizer.Color[2] * _lightColor0BoostMult;
                    }

                    return Colorizer.Color[0] * _lightColor0Mult;

                case EnvironmentColorType.Color1:
                    if (_usingBoostColors)
                    {
                        return Colorizer.Color[3] * _lightColor1BoostMult;
                    }

                    return Colorizer.Color[1] * _lightColor1Mult;

                case EnvironmentColorType.ColorW:
                    return _colorManager.ColorForType(EnvironmentColorType.ColorW, _usingBoostColors);
            }
        }

        public Color GetHighlightColor(int beatmapEventValue)
        {
            switch (BeatmapEventDataLightsExtensions.GetLightColorTypeFromEventDataValue(beatmapEventValue))
            {
                ////case EnvironmentColorType.Color0:
                default:
                    if (_usingBoostColors)
                    {
                        return Colorizer.Color[2] * _highlightColor0BoostMult;
                    }

                    return Colorizer.Color[0] * _highlightColor0Mult;

                case EnvironmentColorType.Color1:
                    if (_usingBoostColors)
                    {
                        return Colorizer.Color[3] * _highlightColor1BoostMult;
                    }

                    return Colorizer.Color[1] * _highlightColor1Mult;

                case EnvironmentColorType.ColorW:
                    return _colorManager.ColorForType(EnvironmentColorType.ColorW, _usingBoostColors);
            }
        }

        public void Refresh(bool hard, IEnumerable<ILightWithId>? selectLights, BasicBeatmapEventData? beatmapEventData = null, Functions? easing = null, LerpType? lerpType = null)
        {
            // TODO: This makes significant amount of allocation.
            IEnumerable<ChromaIDColorTween> selectTweens = selectLights == null ? ColorTweens.Values : GetColorTweens(selectLights);

            foreach (ChromaIDColorTween tween in selectTweens)
            {
                BasicBeatmapEventData previousEvent;
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
                    case 9:
                    case 4:
                    case 8:
                    case 12:
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
                    case 10:
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
                    case 11:
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

                continue;

                // this code is UGLY
                void CheckNextEventForFadeBetter()
                {
                    _deserializedData.Resolve(previousEvent, out ChromaEventData? eventData);
                    Dictionary<int, BasicBeatmapEventData>? nextSameTypesDict = eventData?.NextSameTypeEvent;
                    BasicBeatmapEventData? nextSameTypeEvent = null;
                    if (!_featuresModule.Active || nextSameTypesDict == null)
                    {
                        nextSameTypeEvent = previousEvent.nextSameTypeEventData;
                    }
                    else if (nextSameTypesDict.TryGetValue(tween.Id, out BasicBeatmapEventData? value))
                    {
                        nextSameTypeEvent = value;
                    }
                    else if (nextSameTypesDict.TryGetValue(-1, out BasicBeatmapEventData? nullVal))
                    {
                        nextSameTypeEvent = nullVal;
                    }

                    if (nextSameTypeEvent == null || !nextSameTypeEvent.HasLightFadeEventDataValue())
                    {
                        return;
                    }

                    float nextFloatValue = nextSameTypeEvent.floatValue;
                    int nextValue = nextSameTypeEvent.value;
                    EnvironmentColorType nextColorType = BeatmapEventDataLightsExtensions.GetLightColorTypeFromEventDataValue(nextSameTypeEvent.value);
                    Color nextColor;

                    _deserializedData.Resolve(nextSameTypeEvent, out ChromaEventData? nextEventData);
                    Color? nextColorData = nextEventData?.ColorData;
                    if (nextColorType != EnvironmentColorType.ColorW &&
                        _featuresModule.Active &&
                        nextColorData.HasValue)
                    {
                        Color multiplierColor;
                        if (_usingBoostColors)
                        {
                            multiplierColor = nextColorType == EnvironmentColorType.Color1 ? _lightColor1BoostMult : _lightColor0BoostMult;
                        }
                        else
                        {
                            multiplierColor = nextColorType == EnvironmentColorType.Color1 ? _lightColor1Mult : _lightColor0Mult;
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
                    else if (!previousEvent.HasFixedDurationLightSwitchEventDataValue())
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
            }

            DidRefresh?.Invoke();
        }

        private IEnumerable<ChromaIDColorTween> GetColorTweens(IEnumerable<ILightWithId> selectLights)
        {
            foreach (ILightWithId light in selectLights)
            {
                if (ColorTweens.TryGetValue(light, out ChromaIDColorTween? tween))
                {
                    yield return tween;
                }
            }
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
                    _tableManager.GetActiveTableValueReverse(LightsID, id) ?? 0);

                ColorTweens[lightWithId] = tween;
                tween.ForceOnUpdate();
            }
            else
            {
                _log.Error("Attempted to register duplicate ILightWithId");
            }
        }

        internal void UnregisterLight(ILightWithId lightWithId)
        {
            if (!ColorTweens.TryGetValue(lightWithId, out ChromaIDColorTween tween))
            {
                return;
            }

            tween.Kill();
            ColorTweens.Remove(lightWithId);
        }

        // TODO: Something in there makes significant amount of allocation.
        private void BasicCallback(BasicBeatmapEventData beatmapEventData)
        {
            IEnumerable<ILightWithId>? selectLights = null;
            Functions? easing = null;
            LerpType? lerpType = null;

            // fun fun chroma stuff
            if (_featuresModule.Active)
            {
                if (_gradientController == null)
                {
                    throw new InvalidOperationException("Chroma Features requires the gradient controller.");
                }

                if (_deserializedData.Resolve(beatmapEventData, out ChromaEventData? chromaData))
                {
                    Color? color = null;

                    // propID is now DEPRECATED!!!!!!!!
                    object? propID = chromaData.PropID;
                    if (propID != null)
                    {
                        if (propID is List<object> propIDobjects)
                        {
                            var ids = new List<int>();
                            foreach (var obj in propIDobjects)
                            {
                                ids.Add(Convert.ToInt32(obj));
                            }
                            selectLights = Colorizer.GetPropagationLightWithIds(ids);
                        }
                        else if (propID is long propIDlong)
                        {
                            selectLights = Colorizer.GetPropagationLightWithIds(new[] { (int)propIDlong });
                        }
                    }
                    else if (chromaData.LightID != null)
                    {
                        selectLights = Colorizer.GetLightWithIds(chromaData.LightID);
                    }

                    // fck gradients
                    ChromaEventData.GradientObjectData? gradientObject = chromaData.GradientObject;
                    if (gradientObject != null)
                    {
                        color = _gradientController.AddGradient(gradientObject, beatmapEventData.basicBeatmapEventType, beatmapEventData.time);
                    }

                    Color? colorData = chromaData.ColorData;
                    if (colorData.HasValue)
                    {
                        color = colorData;
                        _gradientController.CancelGradient(beatmapEventData.basicBeatmapEventType);
                    }

                    if (color.HasValue)
                    {
                        Color finalColor = color.Value;
                        Colorizer.Colorize(false, finalColor, finalColor, finalColor, finalColor);
                    }
                    else if (!_gradientController.IsGradientActive(beatmapEventData.basicBeatmapEventType))
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

        private void BoostCallback(ColorBoostBeatmapEventData beatmapEventData)
        {
            bool flag = beatmapEventData.boostColorsAreOn;
            if (flag == _usingBoostColors)
            {
                return;
            }

            _usingBoostColors = flag;
            Refresh(false, null);
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<LightSwitchEventEffect, ChromaLightSwitchEventEffect>
        {
        }
    }
}
