using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Chroma.Lighting;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using UnityEngine;
using Zenject;
using static Chroma.ChromaController;
using static Heck.HeckController;

namespace Chroma
{
    internal class CustomDataManager
    {
        [EarlyDeserializer]
        internal static void DeserializerEarly(
            TrackBuilder trackBuilder,
            CustomBeatmapData beatmapData,
            IReadOnlyList<CustomEventData> customEventDatas,
            bool v2)
        {
            IEnumerable<CustomData>? environmentData = beatmapData.customData.Get<List<object>>(V2_ENVIRONMENT)?.Cast<CustomData>();
            if (environmentData != null)
            {
                foreach (CustomData gameObjectData in environmentData)
                {
                    string? trackName = gameObjectData.Get<string>(v2 ? V2_TRACK : TRACK);
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
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_FOG_TRACK:
                            trackBuilder.AddTrack(customEventData.customData.Get<string>(v2 ? V2_TRACK : TRACK)
                                                  ?? throw new InvalidOperationException("Track was not defined."));
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
            IReadOnlyList<CustomEventData> customEventDatas,
            bool v2)
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();

            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    ICustomEventCustomData chromaCustomEventData;

                    switch (customEventData.eventType)
                    {
                        case ASSIGN_FOG_TRACK:
                            chromaCustomEventData = new ChromaCustomEventData(customEventData.customData.GetTrack(beatmapTracks, v2));
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
            IReadOnlyList<BeatmapObjectData> beatmapObjectDatas,
            bool v2)
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();

            foreach (BeatmapObjectData beatmapObjectData in beatmapObjectDatas)
            {
                try
                {
                    ChromaObjectData chromaObjectData;
                    CustomData customData;

                    switch (beatmapObjectData)
                    {
                        case CustomNoteData customNoteData:
                            customData = customNoteData.customData;
                            chromaObjectData = new ChromaNoteData
                            {
                                Color = GetColorFromData(customData, v2),
                                SpawnEffect = customData.Get<bool?>(NOTE_SPAWN_EFFECT) ?? !customData.Get<bool?>(V2_DISABLE_SPAWN_EFFECT)
                            };
                            break;

                        case CustomObstacleData customObstacleData:
                            customData = customObstacleData.customData;
                            chromaObjectData = new ChromaObjectData
                            {
                                Color = GetColorFromData(customData, v2)
                            };
                            break;

                        default:
                            continue;
                    }

                    CustomData? animationData = customData.Get<CustomData>(v2 ? V2_ANIMATION : ANIMATION);
                    if (animationData != null)
                    {
                        chromaObjectData.LocalPathColor = animationData.GetPointData(v2 ? V2_COLOR : COLOR, pointDefinitions);
                    }

                    chromaObjectData.Track = customData.GetNullableTrackArray(beatmapTracks, v2)?.ToList();

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
            IReadOnlyList<BeatmapEventData> allBeatmapEventDatas,
            DiContainer container,
            bool v2)
        {
            List<BasicBeatmapEventData> beatmapEventDatas = allBeatmapEventDatas.OfType<BasicBeatmapEventData>().ToList();

            LegacyLightHelper? legacyLightHelper = null;
            if (v2)
            {
                legacyLightHelper = new LegacyLightHelper(beatmapEventDatas);
            }

            Dictionary<BeatmapEventData, IEventCustomData> dictionary = new();
            foreach (BasicBeatmapEventData beatmapEventData in beatmapEventDatas)
            {
                try
                {
                    CustomData customData = ((ICustomData)beatmapEventData).customData;

                    Color? color = GetColorFromData(customData, v2);
                    if (legacyLightHelper != null)
                    {
                        color ??= legacyLightHelper.GetLegacyColor(beatmapEventData);
                    }

                    ChromaEventData chromaEventData = new(
                        v2 ? customData.Get<object>(V2_PROPAGATION_ID) : null,
                        color,
                        customData.GetStringToEnum<Functions?>(v2 ? V2_EASING : EASING),
                        customData.GetStringToEnum<LerpType?>(v2 ? V2_LERP_TYPE : LERP_TYPE),
                        customData.Get<bool?>(v2 ? V2_LOCK_POSITION : LOCK_POSITION).GetValueOrDefault(false),
                        customData.Get<string>(v2 ? V2_NAME_FILTER : NAME_FILTER),
                        customData.Get<int?>(v2 ? V2_DIRECTION : DIRECTION),
                        v2 ? customData.Get<bool?>(V2_COUNTER_SPIN) : null, // TODO: YEET
                        v2 ? customData.Get<bool?>(V2_RESET) : null,
                        customData.Get<float?>(v2 ? V2_STEP : STEP),
                        customData.Get<float?>(v2 ? V2_PROP : PROP),
                        customData.Get<float?>(v2 ? V2_SPEED : SPEED) ?? (v2 ? customData.Get<float?>(V2_PRECISE_SPEED) : null),
                        customData.Get<float?>(v2 ? V2_ROTATION : RING_ROTATION),
                        v2 ? customData.Get<float?>(V2_STEP_MULT).GetValueOrDefault(1f) : 1,
                        v2 ? customData.Get<float?>(V2_PROP_MULT).GetValueOrDefault(1f) : 1,
                        v2 ? customData.Get<float?>(V2_SPEED_MULT).GetValueOrDefault(1f) : 1);

                    if (v2)
                    {
                        CustomData? gradientObject = customData.Get<CustomData>(V2_LIGHT_GRADIENT);
                        if (gradientObject != null)
                        {
                            chromaEventData.GradientObject = new ChromaEventData.GradientObjectData(
                                gradientObject.Get<float>(V2_DURATION),
                                GetColorFromData(gradientObject, V2_START_COLOR) ?? Color.white,
                                GetColorFromData(gradientObject, V2_END_COLOR) ?? Color.white,
                                gradientObject.GetStringToEnum<Functions?>(V2_EASING) ?? Functions.easeLinear);
                        }
                    }

                    object? lightID = customData.Get<object>(v2 ? V2_LIGHT_ID : LIGHT_ID);
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

                BasicBeatmapEventType type = beatmapEventDatas[i].basicBeatmapEventType;

                currentEventData.NextSameTypeEvent ??= new Dictionary<int, BasicBeatmapEventData>();

                foreach (int id in currentEventData.LightID)
                {
                    if (i >= beatmapEventDatas.Count - 1)
                    {
                        continue;
                    }

                    int nextIndex = beatmapEventDatas.FindIndex(i + 1, n =>
                    {
                        if (n.basicBeatmapEventType != type)
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
                            if (n.basicBeatmapEventType != type)
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

        private static Color? GetColorFromData(CustomData data, bool v2)
        {
            return GetColorFromData(data, v2 ? V2_COLOR : COLOR);
        }

        private static Color? GetColorFromData(CustomData data, string member = COLOR)
        {
            List<float>? color = data.Get<List<object>>(member)?.Select(Convert.ToSingle).ToList();
            if (color == null)
            {
                return null;
            }

            return new Color(color[0], color[1], color[2], color.Count > 3 ? color[3] : 1);
        }
    }
}
