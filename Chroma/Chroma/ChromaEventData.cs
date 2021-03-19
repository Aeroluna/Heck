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
        internal static Dictionary<BeatmapEventData, ChromaEventData> ChromaEventDatas { get; private set; }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            ChromaEventDatas = new Dictionary<BeatmapEventData, ChromaEventData>();
            foreach (BeatmapEventData beatmapEventData in beatmapData.beatmapEventsData)
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
                        case 9:
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

                        case 12:
                        case 13:
                            chromaEventData = new ChromaLaserSpeedEventData()
                            {
                                LockPosition = ((bool?)Trees.at(customData, LOCKPOSITION)).GetValueOrDefault(false),
                                PreciseSpeed = ((float?)Trees.at(customData, PRECISESPEED)).GetValueOrDefault(beatmapEventData.value),
                                Direction = ((int?)Trees.at(customData, DIRECTION)).GetValueOrDefault(-1),
                            };
                            break;

                        default:
                            continue;
                    }

                    ChromaEventDatas.Add(beatmapEventData, chromaEventData);
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
