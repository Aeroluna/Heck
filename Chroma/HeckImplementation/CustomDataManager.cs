using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Chroma.Extras;
using Chroma.Lighting;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using UnityEngine;
using Zenject;
using static Chroma.ChromaController;

namespace Chroma
{
    internal class CustomDataManager
    {
        [EarlyDeserializer]
        internal static void DeserializerEarly(
            TrackBuilder trackBuilder,
            CustomBeatmapData beatmapData,
            List<CustomEventData> customEventDatas)
        {
            IEnumerable<Dictionary<string, object?>>? environmentData = beatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Cast<Dictionary<string, object?>>();
            if (environmentData != null)
            {
                foreach (Dictionary<string, object?> gameObjectData in environmentData)
                {
                    string? trackName = gameObjectData.Get<string>("_track");
                    if (trackName != null)
                    {
                        trackBuilder.AddTrack(trackName);
                    }
                }
            }

            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    switch (customEventData.type)
                    {
                        case ASSIGN_FOG_TRACK:
                            trackBuilder.AddTrack(customEventData.data.Get<string>("_track") ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, customEventData);
                }
            }
        }

        [CustomEventsDeserializer]
        internal static Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents(
            Dictionary<string, Track> beatmapTracks,
            List<CustomEventData> customEventDatas)
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();

            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    ICustomEventCustomData chromaCustomEventData;

                    switch (customEventData.type)
                    {
                        case ASSIGN_FOG_TRACK:
                            chromaCustomEventData = new ChromaCustomEventData(customEventData.data.GetTrack(beatmapTracks) ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        default:
                            continue;
                    }

                    dictionary.Add(customEventData, chromaCustomEventData);
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, customEventData);
                }
            }

            return dictionary;
        }

        [ObjectsDeserializer]
        internal static Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects(
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, PointDefinition> pointDefinitions,
            List<BeatmapObjectData> beatmapObjectDatas)
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();

            foreach (BeatmapObjectData beatmapObjectData in beatmapObjectDatas)
            {
                try
                {
                    ChromaObjectData chromaObjectData;
                    Dictionary<string, object?> customData;

                    switch (beatmapObjectData)
                    {
                        case CustomNoteData customNoteData:
                            customData = customNoteData.customData;
                            chromaObjectData = new ChromaNoteData
                            {
                                Color = ChromaUtils.GetColorFromData(customData),
                                DisableSpawnEffect = customData.Get<bool?>(DISABLE_SPAWN_EFFECT)
                            };
                            break;

                        case CustomObstacleData customObstacleData:
                            customData = customObstacleData.customData;
                            chromaObjectData = new ChromaObjectData
                            {
                                Color = ChromaUtils.GetColorFromData(customData)
                            };
                            break;

                        case CustomWaypointData customWaypointData:
                            customData = customWaypointData.customData;
                            chromaObjectData = new ChromaObjectData();
                            break;

                        default:
                            continue;
                    }

                    Dictionary<string, object?>? animationObjectDyn = customData.Get<Dictionary<string, object?>>(ANIMATION);
                    if (animationObjectDyn != null)
                    {
                        chromaObjectData.LocalPathColor = animationObjectDyn.GetPointData(COLOR, pointDefinitions);
                    }

                    chromaObjectData.Track = customData.GetTrackArray(beatmapTracks)?.ToList();

                    dictionary.Add(beatmapObjectData, chromaObjectData);
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, beatmapObjectData);
                }
            }

            return dictionary;
        }

        [EventsDeserializer]
        internal static Dictionary<BeatmapEventData, IEventCustomData> DeserializeEvents(
            List<BeatmapEventData> beatmapEventDatas,
            DiContainer container)
        {
            container.Bind<LegacyLightHelper>().FromInstance(new LegacyLightHelper(beatmapEventDatas)).AsSingle();

            Dictionary<BeatmapEventData, IEventCustomData> dictionary = new();
            foreach (BeatmapEventData beatmapEventData in beatmapEventDatas)
            {
                try
                {
                    if (beatmapEventData is not CustomBeatmapEventData customBeatmapEventData)
                    {
                        continue;
                    }

                    Dictionary<string, object?> customData = customBeatmapEventData.customData;
                    ChromaEventData chromaEventData = new(
                        customData.Get<object>(PROPAGATION_ID),
                        ChromaUtils.GetColorFromData(customData),
                        customData.GetStringToEnum<Functions?>(EASING),
                        customData.GetStringToEnum<LerpType?>(LERP_TYPE),
                        customData.Get<bool?>(LOCK_POSITION).GetValueOrDefault(false),
                        customData.Get<string>(NAME_FILTER),
                        customData.Get<int?>(DIRECTION),
                        customData.Get<bool?>(COUNTER_SPIN),
                        customData.Get<bool?>(RESET),
                        customData.Get<float?>(STEP),
                        customData.Get<float?>(PROP),
                        customData.Get<float?>(SPEED) ?? customData.Get<float?>(PRECISE_SPEED),
                        customData.Get<float?>(ROTATION),
                        customData.Get<float?>(STEP_MULT).GetValueOrDefault(1f),
                        customData.Get<float?>(PROP_MULT).GetValueOrDefault(1f),
                        customData.Get<float?>(SPEED_MULT).GetValueOrDefault(1f));

                    Dictionary<string, object?>? gradientObject = customData.Get<Dictionary<string, object?>>(LIGHT_GRADIENT);
                    if (gradientObject != null)
                    {
                        chromaEventData.GradientObject = new ChromaEventData.GradientObjectData(
                            gradientObject.Get<float>(DURATION),
                            ChromaUtils.GetColorFromData(gradientObject, START_COLOR) ?? Color.white,
                            ChromaUtils.GetColorFromData(gradientObject, END_COLOR) ?? Color.white,
                            gradientObject.GetStringToEnum<Functions?>(EASING) ?? Functions.easeLinear);
                    }

                    object? lightID = customData.Get<object>(LIGHT_ID);
                    if (lightID != null)
                    {
                        chromaEventData.LightID = lightID switch
                        {
                            List<object> lightIDobjects => lightIDobjects.Select(Convert.ToInt32),
                            long lightIDint => new[] { (int)lightIDint },
                            _ => null
                        };
                    }

                    dictionary.Add(beatmapEventData, chromaEventData);
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, beatmapEventData);
                }
            }

            bool TryGetEventData(BeatmapEventData beatmapEventData, [NotNullWhen(true)] out ChromaEventData? chromaEventData)
            {
                if (!dictionary.TryGetValue(beatmapEventData, out IEventCustomData? eventCustomData))
                {
                    chromaEventData = null;
                    return false;
                }

                chromaEventData = (ChromaEventData)eventCustomData;
                return true;
            }

            // Horrible stupid logic to get next same type event per light id
            for (int i = 0; i < beatmapEventDatas.Count; i++)
            {
                if (!TryGetEventData(beatmapEventDatas[i], out ChromaEventData? currentEventData) || currentEventData.LightID == null)
                {
                    continue;
                }

                BeatmapEventType type = beatmapEventDatas[i].type;

                currentEventData.NextSameTypeEvent ??= new Dictionary<int, BeatmapEventData>();

                foreach (int id in currentEventData.LightID)
                {
                    if (i >= beatmapEventDatas.Count - 1)
                    {
                        continue;
                    }

                    int nextIndex = beatmapEventDatas.FindIndex(i + 1, n =>
                    {
                        if (n.type != type)
                        {
                            return false;
                        }

                        return TryGetEventData(n, out ChromaEventData? nextEventData)
                               && nextEventData.LightID != null
                               && nextEventData.LightID.Contains(id);
                    });

                    if (nextIndex != -1)
                    {
                        currentEventData.NextSameTypeEvent[id] = beatmapEventDatas[nextIndex];
                    }
                    else
                    {
                        nextIndex = beatmapEventDatas.FindIndex(i + 1, n =>
                        {
                            if (n.type != type)
                            {
                                return false;
                            }

                            return !(TryGetEventData(n, out ChromaEventData? nextEventData)
                                    && nextEventData.LightID != null);
                        });

                        if (nextIndex != -1)
                        {
                            currentEventData.NextSameTypeEvent[id] = beatmapEventDatas[nextIndex];
                        }
                    }
                }
            }

            return dictionary;
        }
    }
}
