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

namespace Chroma
{
    internal class CustomDataManager
    {
        [EarlyDeserializer]
        internal static void DeserializerEarly(
            TrackBuilder trackBuilder,
            CustomBeatmapData beatmapData,
            IReadOnlyList<CustomEventData> customEventDatas)
        {
            bool v2 = beatmapData.version2_6_0AndEarlier;
            IEnumerable<CustomData>? environmentData = beatmapData.customData.Get<List<object>>(v2 ? V2_ENVIRONMENT : ENVIRONMENT)?.Cast<CustomData>();
            if (environmentData != null)
            {
                foreach (CustomData gameObjectData in environmentData)
                {
                    trackBuilder.AddFromCustomData(gameObjectData, v2, false);
                }
            }

            if (!v2)
            {
                return;
            }

            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_FOG_TRACK:
                            trackBuilder.AddFromCustomData(customEventData.customData, v2);
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
            CustomBeatmapData beatmapData,
            Dictionary<string, Track> beatmapTracks,
            IReadOnlyList<CustomEventData> customEventDatas)
        {
            bool v2 = beatmapData.version2_6_0AndEarlier;

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
            CustomBeatmapData beatmapData,
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, PointDefinition> pointDefinitions,
            IReadOnlyList<BeatmapObjectData> beatmapObjectDatas)
        {
            bool v2 = beatmapData.version2_6_0AndEarlier;
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();

            foreach (BeatmapObjectData beatmapObjectData in beatmapObjectDatas)
            {
                try
                {
                    CustomData customData = ((ICustomData)beatmapObjectData).customData;
                    switch (beatmapObjectData)
                    {
                        case CustomNoteData:
                            dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, beatmapTracks, pointDefinitions, v2));
                            break;

                        case CustomObstacleData:
                            dictionary.Add(beatmapObjectData, new ChromaObjectData(customData, beatmapTracks, pointDefinitions, v2));
                            break;

                        default:
                            continue;
                    }
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
            CustomBeatmapData beatmapData,
            IReadOnlyList<BeatmapEventData> allBeatmapEventDatas,
            DiContainer container)
        {
            bool v2 = beatmapData.version2_6_0AndEarlier;
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
                    dictionary.Add(beatmapEventData, new ChromaEventData(beatmapEventData, legacyLightHelper, v2));
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

        internal static Color? GetColorFromData(CustomData data, bool v2)
        {
            return GetColorFromData(data, v2 ? V2_COLOR : COLOR);
        }

        internal static Color? GetColorFromData(CustomData data, string member = COLOR)
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
