namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static Chroma.Plugin;

    internal static class ChromaEventDataManager
    {
        private static Dictionary<BeatmapEventData, ChromaEventData> _chromaEventDatas;

        internal static T TryGetEventData<T>(BeatmapEventData beatmapEventData)
        {
            if (_chromaEventDatas.TryGetValue(beatmapEventData, out ChromaEventData chromaEventData))
            {
                if (chromaEventData is T t)
                {
                    return t;
                }
                else
                {
                    throw new InvalidOperationException($"ChromaEventData was not of correct type. Expected: {typeof(T).Name}, was: {chromaEventData.GetType().Name}");
                }
            }

            return default;
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
                        ChromaEventData chromaEventData;
                        dynamic customData = customBeatmapEventData.customData;

                        switch ((int)beatmapEventData.type)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                ChromaLightEventData chromaLightEventData = new ChromaLightEventData()
                                {
                                    LightID = Trees.at(customData, LIGHTID),
                                    PropID = Trees.at(customData, PROPAGATIONID),
                                    ColorData = ChromaUtils.GetColorFromData(customData),
                                };

                                dynamic gradientObject = Trees.at(customData, LIGHTGRADIENT);
                                if (gradientObject != null)
                                {
                                    string easingstring = (string)Trees.at(gradientObject, EASING);
                                    Functions easing;
                                    if (string.IsNullOrEmpty(easingstring))
                                    {
                                        easing = Functions.easeLinear;
                                    }
                                    else
                                    {
                                        easing = (Functions)Enum.Parse(typeof(Functions), easingstring);
                                    }

                                    chromaLightEventData.GradientObject = new ChromaLightEventData.GradientObjectData()
                                    {
                                        Duration = (float)Trees.at(gradientObject, DURATION),
                                        StartColor = ChromaUtils.GetColorFromData(gradientObject, STARTCOLOR),
                                        EndColor = ChromaUtils.GetColorFromData(gradientObject, ENDCOLOR),
                                        Easing = easing,
                                    };
                                }

                                chromaEventData = chromaLightEventData;

                                break;

                            case 8:
                                chromaEventData = new ChromaRingRotationEventData()
                                {
                                    NameFilter = Trees.at(customData, NAMEFILTER),
                                    Direction = (int?)Trees.at(customData, DIRECTION),
                                    CounterSpin = Trees.at(customData, COUNTERSPIN),
                                    Reset = Trees.at(customData, RESET),
                                    Step = (float?)Trees.at(customData, STEP),
                                    Prop = (float?)Trees.at(customData, PROP),
                                    Speed = (float?)Trees.at(customData, SPEED),
                                    Rotation = (float?)Trees.at(customData, ROTATION),
                                    StepMult = ((float?)Trees.at(customData, STEPMULT)).GetValueOrDefault(1f),
                                    PropMult = ((float?)Trees.at(customData, PROPMULT)).GetValueOrDefault(1f),
                                    SpeedMult = ((float?)Trees.at(customData, SPEEDMULT)).GetValueOrDefault(1f),
                                };
                                break;

                            case 9:
                                chromaEventData = new ChromaRingStepEventData()
                                {
                                    Step = (float?)Trees.at(customData, STEP),
                                };
                                break;

                            case 12:
                            case 13:
                                chromaEventData = new ChromaLaserSpeedEventData()
                                {
                                    LockPosition = ((bool?)Trees.at(customData, LOCKPOSITION)).GetValueOrDefault(false),
                                    PreciseSpeed = ((float?)Trees.at(customData, SPEED)).GetValueOrDefault(((float?)Trees.at(customData, PRECISESPEED)).GetValueOrDefault(beatmapEventData.value)),
                                    Direction = ((int?)Trees.at(customData, DIRECTION)).GetValueOrDefault(-1),
                                };
                                break;

                            default:
                                continue;
                        }

                        if (chromaEventData != null)
                        {
                            _chromaEventDatas.Add(beatmapEventData, chromaEventData);
                        }
                    }
                }
                catch (Exception e)
                {
                    ChromaLogger.Log($"Could not create ChromaEventData for event {beatmapEventData.type} at {beatmapEventData.time}", IPA.Logging.Logger.Level.Error);
                    ChromaLogger.Log(e, IPA.Logging.Logger.Level.Error);
                }
            }
        }
    }

    internal class ChromaLightEventData : ChromaEventData
    {
        internal object LightID { get; set; }

        internal object PropID { get; set; }

        internal Color? ColorData { get; set; }

        internal GradientObjectData GradientObject { get; set; }

        internal class GradientObjectData
        {
            internal float Duration { get; set; }

            internal Color StartColor { get; set; }

            internal Color EndColor { get; set; }

            internal Functions Easing { get; set; }
        }
    }

    internal class ChromaRingRotationEventData : ChromaEventData
    {
        internal string NameFilter { get; set; }

        internal int? Direction { get; set; }

        internal bool? CounterSpin { get; set; }

        internal bool? Reset { get; set; }

        internal float? Step { get; set; }

        internal float? Prop { get; set; }

        internal float? Speed { get; set; }

        internal float? Rotation { get; set; }

        internal float StepMult { get; set; }

        internal float PropMult { get; set; }

        internal float SpeedMult { get; set; }
    }

    internal class ChromaRingStepEventData : ChromaEventData
    {
        internal float? Step { get; set; }
    }

    internal class ChromaLaserSpeedEventData : ChromaEventData
    {
        internal bool LockPosition { get; set; }

        internal float PreciseSpeed { get; set; }

        internal int Direction { get; set; }
    }

    internal class ChromaEventData
    {
    }
}
