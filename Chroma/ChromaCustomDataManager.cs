using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Lighting;
using Chroma.Utils;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using UnityEngine;
using static Chroma.ChromaController;

namespace Chroma
{
    internal static class ChromaCustomDataManager
    {
        private static Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData> _chromaObjectDatas = new();
        private static Dictionary<BeatmapEventData, IBeatmapEventDataCustomData> _chromaEventDatas = new();
        private static Dictionary<CustomEventData, ICustomEventCustomData> _chromaCustomEventDatas = new();

        internal static T? TryGetObjectData<T>(BeatmapObjectData beatmapObjectData)
            where T : ChromaObjectData
        {
            return _chromaObjectDatas.TryGetCustomData<T>(beatmapObjectData);
        }

        internal static ChromaEventData? TryGetEventData(BeatmapEventData beatmapEventData)
        {
            return _chromaEventDatas.TryGetCustomData<ChromaEventData>(beatmapEventData);
        }

        internal static ChromaCustomEventData? TryGetCustomEventData(CustomEventData customEventData)
        {
            return _chromaCustomEventDatas.TryGetCustomData<ChromaCustomEventData>(customEventData);
        }

        internal static void OnBuildTracks(CustomDataDeserializer.DeserializeBeatmapEventArgs eventArgs)
        {
            TrackBuilder trackBuilder = eventArgs.TrackBuilder;

            IEnumerable<Dictionary<string, object?>>? environmentData = eventArgs.BeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Cast<Dictionary<string, object?>>();
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

            foreach (CustomEventData customEventData in eventArgs.CustomEventDatas)
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
                    CustomDataDeserializer.LogFailure(Log.Logger, e, customEventData);
                }
            }
        }

        internal static void OnDeserializeBeatmapData(CustomDataDeserializer.DeserializeBeatmapEventArgs eventArgs)
        {
            CustomBeatmapData beatmapData = eventArgs.BeatmapData;

            TrackBuilder trackBuilder = eventArgs.TrackBuilder;
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

            _chromaCustomEventDatas = new Dictionary<CustomEventData, ICustomEventCustomData>();

            Dictionary<string, Track> beatmapTracks = beatmapData.GetBeatmapTracks();
            foreach (CustomEventData customEventData in eventArgs.CustomEventDatas)
            {
                try
                {
                    ICustomEventCustomData chromaCustomEventData;

                    switch (customEventData.type)
                    {
                        case ASSIGN_FOG_TRACK:
                            chromaCustomEventData = new ChromaCustomEventData(Heck.Animation.AnimationHelper.GetTrack(customEventData.data, beatmapTracks) ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        default:
                            continue;
                    }

                    _chromaCustomEventDatas.Add(customEventData, chromaCustomEventData);
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Log.Logger, e, customEventData);
                }
            }

            // I can probably remove this, but whatevers
            if (eventArgs.IsMultiplayer)
            {
                return;
            }

            _chromaObjectDatas = new Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData>();

            Dictionary<string, PointDefinition> pointDefinitions = beatmapData.GetBeatmapPointDefinitions();
            foreach (BeatmapObjectData beatmapObjectData in eventArgs.BeatmapObjectDatas)
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
                        chromaObjectData.LocalPathColor = Heck.Animation.AnimationHelper.TryGetPointData(animationObjectDyn, COLOR, pointDefinitions);
                    }

                    chromaObjectData.Track = Heck.Animation.AnimationHelper.GetTrackArray(customData, beatmapTracks)?.ToList();

                    _chromaObjectDatas.Add(beatmapObjectData, chromaObjectData);
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Log.Logger, e, beatmapObjectData);
                }
            }

            _chromaEventDatas = new Dictionary<BeatmapEventData, IBeatmapEventDataCustomData>();
            foreach (BeatmapEventData beatmapEventData in eventArgs.BeatmapEventDatas)
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

                    _chromaEventDatas.Add(beatmapEventData, chromaEventData);
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Log.Logger, e, beatmapEventData);
                }
            }

            // Horrible stupid logic to get next same type event per light id
            List<BeatmapEventData> beatmapEventDatas = (List<BeatmapEventData>)beatmapData.beatmapEventsData;
            for (int i = 0; i < beatmapEventDatas.Count; i++)
            {
                ChromaEventData? currentEventData = TryGetEventData(beatmapEventDatas[i]);
                if (currentEventData?.LightID == null)
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

                        ChromaEventData? nextEventData = TryGetEventData(n);
                        return nextEventData?.LightID != null && nextEventData.LightID.Contains(id);
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

                            ChromaEventData? nextEventData = TryGetEventData(n);
                            return nextEventData?.LightID == null;
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
