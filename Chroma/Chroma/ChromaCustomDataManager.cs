namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck;
    using Heck.Animation;
    using UnityEngine;
    using static Chroma.Plugin;

    internal static class ChromaCustomDataManager
    {
        private static Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData> _chromaObjectDatas = new Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData>();
        private static Dictionary<BeatmapEventData, IBeatmapEventDataCustomData> _chromaEventDatas = new Dictionary<BeatmapEventData, IBeatmapEventDataCustomData>();

        internal static T? TryGetObjectData<T>(BeatmapObjectData beatmapObjectData)
            where T : ChromaObjectData
        {
            return _chromaObjectDatas.TryGetCustomData<T>(beatmapObjectData);
        }

        internal static ChromaEventData? TryGetEventData(BeatmapEventData beatmapEventData)
        {
            return _chromaEventDatas.TryGetCustomData<ChromaEventData>(beatmapEventData);
        }

        internal static void DeserializeBeatmapObjects(bool isMultiplayer, IEnumerable<BeatmapObjectData> beatmapObjectsData, CustomBeatmapData beatmapData)
        {
            if (isMultiplayer)
            {
                return;
            }

            _chromaObjectDatas = new Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData>();

            Dictionary<string, PointDefinition> pointDefinitions = beatmapData.GetBeatmapPointDefinitions();
            Dictionary<string, Track> beatmapTracks = beatmapData.GetBeatmapTracks();
            foreach (BeatmapObjectData beatmapObjectData in beatmapObjectsData)
            {
                try
                {
                    ChromaObjectData chromaObjectData;
                    Dictionary<string, object?> customData;

                    switch (beatmapObjectData)
                    {
                        case CustomNoteData customNoteData:
                            customData = customNoteData.customData;
                            chromaObjectData = new ChromaNoteData()
                            {
                                Color = ChromaUtils.GetColorFromData(customData),
                                DisableSpawnEffect = customData.Get<bool?>(DISABLESPAWNEFFECT),
                            };
                            break;

                        case CustomObstacleData customObstacleData:
                            customData = customObstacleData.customData;
                            chromaObjectData = new ChromaObjectData()
                            {
                                Color = ChromaUtils.GetColorFromData(customData),
                            };
                            break;

                        case CustomWaypointData customWaypointData:
                            customData = customWaypointData.customData;
                            chromaObjectData = new ChromaObjectData();
                            break;

                        default:
                            continue;
                    }

                    if (chromaObjectData != null)
                    {
                        Dictionary<string, object?>? animationObjectDyn = customData.Get<Dictionary<string, object?>>(ANIMATION);
                        if (animationObjectDyn != null)
                        {
                            chromaObjectData.LocalPathColor = Heck.Animation.AnimationHelper.TryGetPointData(animationObjectDyn, COLOR, pointDefinitions);
                        }

                        chromaObjectData.Track = Heck.Animation.AnimationHelper.GetTrackArray(customData, beatmapTracks);

                        _chromaObjectDatas.Add(beatmapObjectData, chromaObjectData);
                    }
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Plugin.Logger, e, beatmapObjectData);
                }
            }
        }

        internal static void DeserializeBeatmapEvents(bool _, IEnumerable<BeatmapEventData> beatmapEventsData, CustomBeatmapData beatmapData)
        {
            _chromaEventDatas = new Dictionary<BeatmapEventData, IBeatmapEventDataCustomData>();
            foreach (BeatmapEventData beatmapEventData in beatmapEventsData)
            {
                try
                {
                    if (beatmapEventData is CustomBeatmapEventData customBeatmapEventData)
                    {
                        Dictionary<string, object?> customData = customBeatmapEventData.customData;
                        ChromaEventData chromaEventData = new ChromaEventData(
                            customData.Get<object>(PROPAGATIONID),
                            ChromaUtils.GetColorFromData(customData),
                            customData.GetStringToEnum<Functions?>(EASING),
                            customData.GetStringToEnum<LerpType?>(LERPTYPE),
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
                            chromaEventData.GradientObject = new ChromaEventData.GradientObjectData(
                                gradientObject.Get<float>(DURATION),
                                ChromaUtils.GetColorFromData(gradientObject, STARTCOLOR) ?? Color.white,
                                ChromaUtils.GetColorFromData(gradientObject, ENDCOLOR) ?? Color.white,
                                gradientObject.GetStringToEnum<Functions?>(EASING) ?? Functions.easeLinear);
                        }

                        object? lightID = customData.Get<object>(LIGHTID);
                        if (lightID != null)
                        {
                            switch (lightID)
                            {
                                case List<object> lightIDobjects:
                                    chromaEventData.LightID = lightIDobjects.Select(n => Convert.ToInt32(n));
                                    break;

                                case long lightIDint:
                                    chromaEventData.LightID = new int[] { (int)lightIDint };
                                    break;
                            }
                        }

                        _chromaEventDatas.Add(beatmapEventData, chromaEventData);
                    }
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Plugin.Logger, e, beatmapEventData);
                }
            }

            // Horrible stupid logic to get next same type event per light id
            List<BeatmapEventData> beatmapEventDatas = (List<BeatmapEventData>)beatmapData.beatmapEventsData;
            for (int i = 0; i < beatmapEventDatas.Count; i++)
            {
                ChromaEventData? currentEventData = TryGetEventData(beatmapEventDatas[i]);
                if (currentEventData?.LightID != null)
                {
                    BeatmapEventType type = beatmapEventDatas[i].type;

                    if (currentEventData.NextSameTypeEvent == null)
                    {
                        currentEventData.NextSameTypeEvent = new Dictionary<int, BeatmapEventData>();
                    }

                    foreach (int id in currentEventData.LightID)
                    {
                        if (i < beatmapEventDatas.Count - 1)
                        {
                            int nextIndex = beatmapEventDatas.FindIndex(i + 1, n =>
                            {
                                if (n.type == type)
                                {
                                    ChromaEventData? nextEventData = TryGetEventData(n);
                                    if (nextEventData?.LightID != null)
                                    {
                                        return nextEventData.LightID.Contains(id);
                                    }
                                }

                                return false;
                            });

                            if (nextIndex != -1)
                            {
                                currentEventData.NextSameTypeEvent[id] = beatmapEventDatas[nextIndex];
                            }
                            else
                            {
                                nextIndex = beatmapEventDatas.FindIndex(i + 1, n =>
                                {
                                    if (n.type == type)
                                    {
                                        ChromaEventData? nextEventData = TryGetEventData(n);
                                        return nextEventData?.LightID == null;
                                    }

                                    return false;
                                });

                                if (nextIndex != -1)
                                {
                                    currentEventData.NextSameTypeEvent[id] = beatmapEventDatas[nextIndex];
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
