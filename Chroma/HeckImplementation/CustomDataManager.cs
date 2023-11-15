using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Chroma.Lighting;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using UnityEngine;
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
                    trackBuilder.AddManyFromCustomData(gameObjectData, v2, false);

                    CustomData? geometryData = gameObjectData.Get<CustomData?>(v2 ? V2_GEOMETRY : GEOMETRY);
                    object? materialData = geometryData?.Get<object?>(v2 ? V2_MATERIAL : MATERIAL);
                    if (materialData is CustomData materialCustomData)
                    {
                        trackBuilder.AddFromCustomData(materialCustomData, v2, false);
                    }
                }
            }

            CustomData? materialsData = beatmapData.customData.Get<CustomData>(v2 ? V2_MATERIALS : MATERIALS);
            if (materialsData != null)
            {
                foreach ((string _, object? value) in materialsData)
                {
                    if (value == null)
                    {
                        continue;
                    }

                    trackBuilder.AddFromCustomData((CustomData)value, v2, false);
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
            Dictionary<string, List<object>> pointDefinitions,
            IReadOnlyList<CustomEventData> customEventDatas)
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in customEventDatas)
            {
                bool v2 = customEventData.version2_6_0AndEarlier;
                try
                {
                    ICustomEventCustomData chromaCustomEventData;

                    switch (customEventData.eventType)
                    {
                        case ASSIGN_FOG_TRACK:
                            if (!v2)
                            {
                                continue;
                            }

                            chromaCustomEventData = new ChromaAssignFogEventData(customEventData.customData.GetTrack(beatmapTracks, v2));
                            break;

                        case ANIMATE_COMPONENT:
                            if (v2)
                            {
                                continue;
                            }

                            chromaCustomEventData = new ChromaAnimateComponentData(customEventData.customData, beatmapTracks, pointDefinitions);
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
            Dictionary<string, List<object>> pointDefinitions,
            IReadOnlyList<BeatmapObjectData> beatmapObjectDatas)
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();

            foreach (BeatmapObjectData beatmapObjectData in beatmapObjectDatas)
            {
                try
                {
                    CustomData customData = ((ICustomData)beatmapObjectData).customData;
                    switch (beatmapObjectData)
                    {
                        case CustomNoteData noteData:
                            dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, beatmapTracks, pointDefinitions, noteData.version2_6_0AndEarlier));
                            break;

                        case CustomSliderData sliderData:
                            dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, beatmapTracks, pointDefinitions, sliderData.version2_6_0AndEarlier));
                            break;

                        case CustomObstacleData obstacleData:
                            dictionary.Add(beatmapObjectData, new ChromaObjectData(customData, beatmapTracks, pointDefinitions, obstacleData.version2_6_0AndEarlier));
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
            IReadOnlyList<BeatmapEventData> allBeatmapEventDatas)
        {
            bool beatmapv2 = beatmapData.version2_6_0AndEarlier;
            List<BasicBeatmapEventData> beatmapEventDatas = allBeatmapEventDatas.OfType<BasicBeatmapEventData>().ToList();

            LegacyLightHelper? legacyLightHelper = null;
            if (beatmapv2)
            {
                legacyLightHelper = new LegacyLightHelper(beatmapEventDatas);
            }

            Dictionary<BeatmapEventData, IEventCustomData> dictionary = new();
            foreach (BasicBeatmapEventData beatmapEventData in beatmapEventDatas)
            {
                bool v2 = beatmapEventData is IVersionable { version2_6_0AndEarlier: true };

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
                if (dictionary.TryGetValue(beatmapEventData, out IEventCustomData? eventCustomData))
                {
                    chromaEventData = (ChromaEventData)eventCustomData;
                    return true;
                }

                chromaEventData = null;
                return false;
            }

            // Horrible stupid logic to get next same type event per light id
            // what am i even doing anymore
            Dictionary<int, Dictionary<int, BasicBeatmapEventData>> allNextSameTypes = new();
            for (int i = beatmapEventDatas.Count - 1; i >= 0; i--)
            {
                BasicBeatmapEventData beatmapEventData = beatmapEventDatas[i];
                if (!TryGetEventData(beatmapEventDatas[i], out ChromaEventData? currentEventData))
                {
                    continue;
                }

                int type = (int)beatmapEventData.basicBeatmapEventType;
                if (!allNextSameTypes.TryGetValue(
                        type,
                        out Dictionary<int, BasicBeatmapEventData>? nextSameTypes))
                {
                    allNextSameTypes[type] = nextSameTypes = new Dictionary<int, BasicBeatmapEventData>();
                }

                currentEventData.NextSameTypeEvent ??= new Dictionary<int, BasicBeatmapEventData>(nextSameTypes);
                IEnumerable<int>? ids = currentEventData.LightID;
                if (ids == null)
                {
                    nextSameTypes[-1] = beatmapEventData;
                }
                else
                {
                    foreach (int id in ids)
                    {
                        nextSameTypes[id] = beatmapEventData;
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
