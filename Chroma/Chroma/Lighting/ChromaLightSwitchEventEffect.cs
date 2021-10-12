namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Colorizer;
    using Chroma.Utils;
    using IPA.Utilities;
    using Tweening;
    using UnityEngine;
    using static ChromaEventDataManager;

    // I originally meant to write this class so everything would be simpler.......
    // fuck boost colors fuck highlight colors
    public class ChromaLightSwitchEventEffect : LightSwitchEventEffect
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
        private static readonly FieldAccessor<LightSwitchEventEffect, bool>.Accessor _lightOnStartAccessor = FieldAccessor<LightSwitchEventEffect, bool>.GetAccessor("_lightOnStart");
        private static readonly FieldAccessor<LightSwitchEventEffect, int>.Accessor _lightsIDAccessor = FieldAccessor<LightSwitchEventEffect, int>.GetAccessor("_lightsID");
        private static readonly FieldAccessor<LightSwitchEventEffect, BeatmapEventType>.Accessor _eventAccessor = FieldAccessor<LightSwitchEventEffect, BeatmapEventType>.GetAccessor("_event");
        private static readonly FieldAccessor<LightSwitchEventEffect, BeatmapEventType>.Accessor _colorBoostEventAccessor = FieldAccessor<LightSwitchEventEffect, BeatmapEventType>.GetAccessor("_colorBoostEvent");
        private static readonly FieldAccessor<LightSwitchEventEffect, LightWithIdManager>.Accessor _lightManagerAccessor = FieldAccessor<LightSwitchEventEffect, LightWithIdManager>.GetAccessor("_lightManager");
        private static readonly FieldAccessor<LightSwitchEventEffect, IBeatmapObjectCallbackController>.Accessor _beatmapObjectCallbackControllerAccessor = FieldAccessor<LightSwitchEventEffect, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");
        private static readonly FieldAccessor<LightSwitchEventEffect, SongTimeTweeningManager>.Accessor _tweeningManagerAccessor = FieldAccessor<LightSwitchEventEffect, SongTimeTweeningManager>.GetAccessor("_tweeningManager");

        private readonly Dictionary<ILightWithId, ChromaIDColorTween> _colorTweens = new Dictionary<ILightWithId, ChromaIDColorTween>();
        private LightColorizer? _lightColorizer;

        private ColorSO? _originalLightColor0;
        private ColorSO? _originalLightColor1;
        private ColorSO? _originalLightColor0Boost;
        private ColorSO? _originalLightColor1Boost;

        public Dictionary<ILightWithId, ChromaIDColorTween> ColorTweens => _colorTweens;

        public BeatmapEventType EventType => _event;

        public LightColorizer LightColorizer => _lightColorizer ?? throw new InvalidOperationException($"[{nameof(_lightColorizer)}] was null.");

        public override void Awake()
        {
        }

        public override void Start()
        {
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent += HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;
        }

        public override void OnDestroy()
        {
            if (_beatmapObjectCallbackController != null)
            {
                _beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;
            }

            _tweeningManager?.KillAllTweens(this);

            LightColorizer.Colorizers.Remove(_event);
        }

        public override void HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger(BeatmapEventData beatmapEventData)
        {
            if (beatmapEventData.type == _event)
            {
                IEnumerable<ILightWithId>? selectLights = null;

                // fun fun chroma stuff
                if (ChromaController.ChromaIsActive)
                {
                    ChromaEventData? chromaData = TryGetEventData(beatmapEventData);
                    if (chromaData != null)
                    {
                        Color? color = null;

                        // legacy was a mistake
                        color = LegacyLightHelper.GetLegacyColor(beatmapEventData) ?? color;

                        if (chromaData.LightID != null)
                        {
                            selectLights = LightColorizer.GetLightWithIds(chromaData.LightID);
                        }

                        // propID is now DEPRECATED!!!!!!!!
                        object? propID = chromaData.PropID;
                        if (propID != null)
                        {
                            switch (propID)
                            {
                                case List<object> propIDobjects:
                                    selectLights = LightColorizer.GetPropagationLightWithIds(propIDobjects.Select(n => Convert.ToInt32(n)));

                                    break;

                                case long propIDlong:
                                    selectLights = LightColorizer.GetPropagationLightWithIds(new int[] { (int)propIDlong });

                                    break;
                            }
                        }

                        // fck gradients
                        ChromaEventData.GradientObjectData? gradientObject = chromaData.GradientObject;
                        if (gradientObject != null)
                        {
                            color = ChromaGradientController.AddGradient(gradientObject, beatmapEventData.type, beatmapEventData.time);
                        }

                        Color? colorData = chromaData.ColorData;
                        if (colorData.HasValue)
                        {
                            color = colorData;
                            ChromaGradientController.CancelGradient(beatmapEventData.type);
                        }

                        if (color.HasValue)
                        {
                            Color finalColor = color.Value;
                            LightColorizer.Colorize(false, finalColor, finalColor, finalColor, finalColor);
                        }
                        else if (!ChromaGradientController.IsGradientActive(beatmapEventData.type))
                        {
                            LightColorizer.Colorize(false, null, null, null, null);
                        }
                    }
                }

                Refresh(true, selectLights, beatmapEventData);
            }
            else if (beatmapEventData.type == _colorBoostEvent)
            {
                bool flag = beatmapEventData.value == 1;
                if (flag != _usingBoostColors)
                {
                    _usingBoostColors = flag;
                    Refresh(false, null);
                }
            }
        }

        public void Refresh(bool hard, IEnumerable<ILightWithId>? selectLights, BeatmapEventData? beatmapEventData = null)
        {
            IEnumerable<ChromaIDColorTween> selectTweens;
            if (selectLights == null)
            {
                selectTweens = _colorTweens.Values;
            }
            else
            {
                selectTweens = selectLights.Select(n => _colorTweens[n]);
            }

            bool boost = _usingBoostColors;
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

                void CheckNextEventForFade()
                {
                    ChromaEventData? eventData = TryGetEventData(previousEvent);
                    Dictionary<int, BeatmapEventData>? nextSameTypesDict = eventData?.NextSameTypeEvent;
                    BeatmapEventData? nextSameTypeEvent;
                    if (ChromaController.ChromaIsActive && (nextSameTypesDict?.ContainsKey(tween.Id) ?? false))
                    {
                        nextSameTypeEvent = nextSameTypesDict[tween.Id];
                    }
                    else
                    {
                        nextSameTypeEvent = previousEvent.nextSameTypeEvent;
                    }

                    if (nextSameTypeEvent != null && (nextSameTypeEvent.value == 4 || nextSameTypeEvent.value == 8))
                    {
                        float nextFloatValue = nextSameTypeEvent.floatValue;
                        int nextValue = nextSameTypeEvent.value;
                        Color nextColor = GetOriginalColor(nextValue, boost);

                        ChromaEventData? nextEventData = TryGetEventData(nextSameTypeEvent);
                        Color? nextColorData = nextEventData?.ColorData;
                        if (ChromaController.ChromaIsActive && nextColorData.HasValue)
                        {
                            nextColor = nextColorData.Value.MultAlpha(nextColor.a);
                        }

                        nextColor = nextColor.MultAlpha(nextFloatValue);
                        Color prevColor = tween.toValue;
                        if (previousValue == 0)
                        {
                            prevColor = nextColor.ColorWithAlpha(0f);
                        }
                        else if (!IsFixedDurationLightSwitch(previousValue))
                        {
                            prevColor = GetNormalColor(previousValue, boost).MultAlpha(previousFloatValue);
                        }

                        tween.fromValue = prevColor;
                        tween.toValue = nextColor;
                        tween.ForceOnUpdate();

                        if (hard)
                        {
                            tween.SetStartTimeAndEndTime(previousEvent.time, nextSameTypeEvent.time);
                            tween.easeType = EaseType.Linear;
                            _tweeningManager.ResumeTween(tween, this);
                        }
                    }
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
                            Color color = GetNormalColor(0, boost).ColorWithAlpha(offAlpha);
                            tween.fromValue = color;
                            tween.toValue = color;
                            tween.SetColor(color);
                            CheckNextEventForFade();
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

                            Color color = GetNormalColor(previousValue, boost).MultAlpha(previousFloatValue);
                            tween.fromValue = color;
                            tween.toValue = color;
                            tween.SetColor(color);
                            CheckNextEventForFade();
                        }

                        break;

                    case 2:
                    case 6:
                        {
                            Color colorFrom = GetHighlightColor(previousValue, boost).MultAlpha(previousFloatValue);
                            Color colorTo = GetNormalColor(previousValue, boost).MultAlpha(previousFloatValue);
                            tween.fromValue = colorFrom;
                            tween.toValue = colorTo;
                            tween.ForceOnUpdate();

                            if (hard)
                            {
                                tween.duration = 0.6f;
                                tween.easeType = EaseType.OutCubic;
                                _tweeningManager.RestartTween(tween, this);
                            }
                        }

                        break;

                    case 3:
                    case 7:
                    case -1:
                        {
                            Color colorFrom = GetHighlightColor(previousValue, boost).MultAlpha(previousFloatValue);
                            Color colorTo = GetNormalColor(previousValue, boost).ColorWithAlpha(_offColorIntensity * previousFloatValue);
                            tween.fromValue = colorFrom;
                            tween.toValue = colorTo;
                            tween.ForceOnUpdate();

                            if (hard)
                            {
                                tween.duration = 1.5f;
                                tween.easeType = EaseType.OutExpo;
                                _tweeningManager.RestartTween(tween, this);
                            }
                        }

                        break;
                }
            }
        }

        internal void CopyValues(LightSwitchEventEffect lightSwitchEventEffect)
        {
            _lightColor0 = _lightColor0Accessor(ref lightSwitchEventEffect);
            _lightColor1 = _lightColor1Accessor(ref lightSwitchEventEffect);
            _highlightColor0 = _highlightColor0Accessor(ref lightSwitchEventEffect);
            _highlightColor1 = _highlightColor1Accessor(ref lightSwitchEventEffect);
            _lightColor0Boost = _lightColor0BoostAccessor(ref lightSwitchEventEffect);
            _lightColor1Boost = _lightColor1BoostAccessor(ref lightSwitchEventEffect);
            _highlightColor0Boost = _highlightColor0BoostAccessor(ref lightSwitchEventEffect);
            _highlightColor1Boost = _highlightColor1BoostAccessor(ref lightSwitchEventEffect);
            _offColorIntensity = _offColorIntensityAccessor(ref lightSwitchEventEffect);
            _lightOnStart = _lightOnStartAccessor(ref lightSwitchEventEffect);
            _lightsID = _lightsIDAccessor(ref lightSwitchEventEffect);
            _event = _eventAccessor(ref lightSwitchEventEffect);
            _colorBoostEvent = _colorBoostEventAccessor(ref lightSwitchEventEffect);

            LightSwitchEventEffect thisSwitch = this;
            _lightManagerAccessor(ref thisSwitch) = _lightManagerAccessor(ref lightSwitchEventEffect);
            _beatmapObjectCallbackControllerAccessor(ref thisSwitch) = _beatmapObjectCallbackControllerAccessor(ref lightSwitchEventEffect);
            _tweeningManagerAccessor(ref thisSwitch) = _tweeningManagerAccessor(ref lightSwitchEventEffect);

            LightColorizer lightColorizer = new LightColorizer(this, _event, _lightManager);
            _lightColorizer = lightColorizer;
            _originalLightColor0 = _lightColor0;
            _originalLightColor0Boost = _lightColor0Boost;
            _originalLightColor1 = _lightColor1;
            _originalLightColor1Boost = _lightColor1Boost;
            lightColorizer.InitializeSOs(ref _lightColor0, ref _highlightColor0, ref _lightColor1, ref _highlightColor1, ref _lightColor0Boost, ref _highlightColor0Boost, ref _lightColor1Boost, ref _highlightColor1Boost);

            List<ILightWithId> lights = lightColorizer.Lights;
            for (int i = 0; i < lights.Count; i++)
            {
                RegisterLight(lights[i], (int)_event, i);
            }

            Color color = _lightOnStart ? _lightColor0 : _lightColor0.color.ColorWithAlpha(_offColorIntensity);
            SetColor(color);
        }

        internal void RegisterLight(ILightWithId lightWithId, int type, int id)
        {
            if (!_colorTweens.ContainsKey(lightWithId))
            {
                _colorTweens[lightWithId] = new ChromaIDColorTween(Color.black, Color.black, lightWithId, _lightManager, LightIDTableManager.GetActiveTableValueReverse(type, id) ?? 0);
            }
            else
            {
                Plugin.Logger.Log($"Attempted to register duplicate ILightWithId.", IPA.Logging.Logger.Level.Error);
            }
        }

        private Color GetOriginalColor(int beatmapEventValue, bool colorBoost)
        {
            if (colorBoost)
            {
                if (!IsColor0(beatmapEventValue))
                {
                    return _originalLightColor1Boost!.color;
                }

                return _originalLightColor0Boost!.color;
            }
            else
            {
                if (!IsColor0(beatmapEventValue))
                {
                    return _originalLightColor1!.color;
                }

                return _originalLightColor0!.color;
            }
        }
    }
}
