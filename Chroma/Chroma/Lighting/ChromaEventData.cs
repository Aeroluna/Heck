namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using UnityEngine;
    using static Chroma.Plugin;

    internal static class ChromaEventDataManager
    {
        private static Dictionary<BeatmapEventData, ChromaEventData> _chromaEventDatas = new Dictionary<BeatmapEventData, ChromaEventData>();

        internal static ChromaEventData? TryGetEventData(BeatmapEventData beatmapEventData)
        {
            if (_chromaEventDatas.TryGetValue(beatmapEventData, out ChromaEventData chromaEventData))
            {
                return chromaEventData;
            }

            return null;
        }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            _chromaEventDatas = new Dictionary<BeatmapEventData, ChromaEventData>();
            foreach (BeatmapEventData beatmapEventData in beatmapData.beatmapEventsData)
            {
                try
                {
                    if (beatmapEventData is CustomBeatmapEventData customBeatmapEventData)
                    {
                        Dictionary<string, object?> customData = customBeatmapEventData.customData;
                        ChromaEventData chromaEventData = new ChromaEventData(
                            customData.Get<object>(LIGHTID),
                            customData.Get<object>(PROPAGATIONID),
                            ChromaUtils.GetColorFromData(customData),
                            customData.Get<bool?>(LOCKPOSITION).GetValueOrDefault(false),
                            customData.Get<string>(NAMEFILTER),
                            customData.Get<int?>(DIRECTION),
                            customData.Get<bool?>(COUNTERSPIN),
                            customData.Get<bool?>(RESET),
                            customData.Get<float?>(STEP),
                            customData.Get<float?>(PROP),
                            customData.Get<float?>(SPEED) ?? customData.Get<float?>(PRECISESPEED),
                            customData.Get<float?>(ROTATION),
                            customData.Get<float?>(STEPMULT).GetValueOrDefault(1f),
                            customData.Get<float?>(PROPMULT).GetValueOrDefault(1f),
                            customData.Get<float?>(SPEEDMULT).GetValueOrDefault(1f));

                        Dictionary<string, object?>? gradientObject = customData.Get<Dictionary<string, object?>>(LIGHTGRADIENT);
                        if (gradientObject != null)
                        {
                            string? easingstring = gradientObject.Get<string>(EASING);
                            Functions easing;
                            if (string.IsNullOrEmpty(easingstring))
                            {
                                easing = Functions.easeLinear;
                            }
                            else
                            {
                                easing = (Functions)Enum.Parse(typeof(Functions), easingstring);
                            }

                            chromaEventData.GradientObject = new ChromaEventData.GradientObjectData(
                                gradientObject.Get<float>(DURATION),
                                ChromaUtils.GetColorFromData(gradientObject, STARTCOLOR) ?? Color.white,
                                ChromaUtils.GetColorFromData(gradientObject, ENDCOLOR) ?? Color.white,
                                easing);
                        }

                        _chromaEventDatas.Add(beatmapEventData, chromaEventData);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Logger.Log($"Could not create ChromaEventData for event {beatmapEventData.type} at {beatmapEventData.time}", IPA.Logging.Logger.Level.Error);
                    Plugin.Logger.Log(e, IPA.Logging.Logger.Level.Error);
                }
            }
        }
    }

    internal record ChromaEventData
    {
        internal ChromaEventData(
            object? lightID,
            object? propID,
            Color? colorData,
            bool lockPosition,
            string? nameFilter,
            int? direction,
            bool? counterSpin,
            bool? reset,
            float? step,
            float? prop,
            float? speed,
            float? rotation,
            float stepMult,
            float propMult,
            float speedMult)
        {
            LightID = lightID;
            PropID = propID;
            ColorData = colorData;
            LockPosition = lockPosition;
            NameFilter = nameFilter;
            Direction = direction;
            CounterSpin = counterSpin;
            Reset = reset;
            Step = step;
            Prop = prop;
            Speed = speed;
            Rotation = rotation;
            StepMult = stepMult;
            PropMult = propMult;
            SpeedMult = speedMult;
        }

        internal object? LightID { get; }

        internal object? PropID { get; }

        internal Color? ColorData { get; }

        internal GradientObjectData? GradientObject { get; set; }

        internal bool LockPosition { get; }

        internal string? NameFilter { get; }

        internal int? Direction { get; }

        internal bool? CounterSpin { get; }

        internal bool? Reset { get; }

        internal float? Step { get; }

        internal float? Prop { get; }

        internal float? Speed { get; }

        internal float? Rotation { get; }

        internal float StepMult { get; }

        internal float PropMult { get; }

        internal float SpeedMult { get; }

        internal record GradientObjectData
        {
            internal GradientObjectData(float duration, Color startColor, Color endColor, Functions easing)
            {
                Duration = duration;
                StartColor = startColor;
                EndColor = endColor;
                Easing = easing;
            }

            internal float Duration { get; }

            internal Color StartColor { get; }

            internal Color EndColor { get; }

            internal Functions Easing { get; }
        }
    }
}
