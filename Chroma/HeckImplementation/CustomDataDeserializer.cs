using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Chroma.Lighting;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using Heck.Deserialize;
using IPA.Utilities;
using UnityEngine;
using static Chroma.ChromaController;

namespace Chroma;

internal class CustomDataDeserializer : IEarlyDeserializer, ICustomEventsDeserializer, IEventsDeserializer,
    IObjectsDeserializer
{
    private readonly CustomBeatmapData _beatmapData;
    private readonly float _bpm;
    private readonly Dictionary<string, List<object>> _pointDefinitions;
    private readonly TrackBuilder _trackBuilder;
    private readonly Dictionary<string, Track> _tracks;

    private CustomDataDeserializer(
        TrackBuilder trackBuilder,
        CustomBeatmapData beatmapData,
        Dictionary<string, Track> tracks,
        Dictionary<string, List<object>> pointDefinitions,
        float bpm)
    {
        _trackBuilder = trackBuilder;
        _beatmapData = beatmapData;
        _tracks = tracks;
        _pointDefinitions = pointDefinitions;
        _bpm = bpm;
    }

    public Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents()
    {
        Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
        foreach (CustomEventData customEventData in _beatmapData.customEventDatas)
        {
            bool v2 = customEventData.version.IsVersion2();
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

                        chromaCustomEventData =
                            new ChromaAssignFogEventData(customEventData.customData.GetTrack(_tracks, v2));
                        break;

                    case ANIMATE_COMPONENT:
                        if (v2)
                        {
                            continue;
                        }

                        chromaCustomEventData = new ChromaAnimateComponentData(
                            customEventData.customData,
                            _tracks,
                            _pointDefinitions);
                        break;

                    default:
                        continue;
                }

                dictionary.Add(customEventData, chromaCustomEventData);
            }
            catch (Exception e)
            {
                Plugin.Log.DeserializeFailure(e, customEventData, _bpm);
            }
        }

        return dictionary;
    }

    public void DeserializeEarly()
    {
        bool v2 = _beatmapData.version.IsVersion2();
        IEnumerable<CustomData>? environmentData =
            _beatmapData.customData.Get<List<object>>(v2 ? V2_ENVIRONMENT : ENVIRONMENT)?.Cast<CustomData>();
        if (environmentData != null)
        {
            foreach (CustomData gameObjectData in environmentData)
            {
                _trackBuilder.AddManyFromCustomData(gameObjectData, v2, false);

                CustomData? geometryData = gameObjectData.Get<CustomData?>(v2 ? V2_GEOMETRY : GEOMETRY);
                object? materialData = geometryData?.Get<object?>(v2 ? V2_MATERIAL : MATERIAL);
                if (materialData is CustomData materialCustomData)
                {
                    _trackBuilder.AddFromCustomData(materialCustomData, v2, false);
                }
            }
        }

        CustomData? materialsData = _beatmapData.customData.Get<CustomData>(v2 ? V2_MATERIALS : MATERIALS);
        if (materialsData != null)
        {
            foreach ((string _, object? value) in materialsData)
            {
                if (value == null)
                {
                    continue;
                }

                _trackBuilder.AddFromCustomData((CustomData)value, v2, false);
            }
        }

        if (!v2)
        {
            return;
        }

        foreach (CustomEventData customEventData in _beatmapData.customEventDatas)
        {
            try
            {
                switch (customEventData.eventType)
                {
                    case ASSIGN_FOG_TRACK:
                        _trackBuilder.AddFromCustomData(customEventData.customData, v2);
                        break;

                    default:
                        continue;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.DeserializeFailure(e, customEventData, _bpm);
            }
        }
    }

    public Dictionary<BeatmapEventData, IEventCustomData> DeserializeEvents()
    {
        bool beatmapv2 = _beatmapData.version.IsVersion2();
        List<BasicBeatmapEventData> beatmapEventDatas =
            _beatmapData.beatmapEventDatas.OfType<BasicBeatmapEventData>().ToList();

        LegacyLightHelper? legacyLightHelper = null;
        if (beatmapv2)
        {
            legacyLightHelper = new LegacyLightHelper(beatmapEventDatas);
        }

        Dictionary<BeatmapEventData, IEventCustomData> dictionary = new();
        foreach (BasicBeatmapEventData beatmapEventData in beatmapEventDatas)
        {
            bool v2 = beatmapEventData is IVersionable versionable && versionable.version.IsVersion2();

            try
            {
                dictionary.Add(beatmapEventData, new ChromaEventData(beatmapEventData, legacyLightHelper, v2));
            }
            catch (Exception e)
            {
                Plugin.Log.DeserializeFailure(e, beatmapEventData, _bpm);
            }
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
                foreach (int key in nextSameTypes.Keys.ToArray())
                {
                    nextSameTypes[key] = beatmapEventData;
                }
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

        bool TryGetEventData(
            BeatmapEventData beatmapEventData,
            [NotNullWhen(true)] out ChromaEventData? chromaEventData)
        {
            if (dictionary.TryGetValue(beatmapEventData, out IEventCustomData? eventCustomData))
            {
                chromaEventData = (ChromaEventData)eventCustomData;
                return true;
            }

            chromaEventData = null;
            return false;
        }
    }

    public Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects()
    {
        Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();

        foreach (BeatmapObjectData beatmapObjectData in _beatmapData.beatmapObjectDatas)
        {
            try
            {
                bool v2 = beatmapObjectData is IVersionable versionable && versionable.version.IsVersion2();
                CustomData customData = ((ICustomData)beatmapObjectData).customData;
                switch (beatmapObjectData)
                {
                    case CustomNoteData:
                    case CustomSliderData:
                        dictionary.Add(
                            beatmapObjectData,
                            new ChromaNoteData(customData, _tracks, _pointDefinitions, v2));
                        break;

                    case CustomObstacleData:
                        dictionary.Add(
                            beatmapObjectData,
                            new ChromaObjectData(customData, _tracks, _pointDefinitions, v2));
                        break;

                    default:
                        continue;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.DeserializeFailure(e, beatmapObjectData, _bpm);
            }
        }

        return dictionary;
    }

    internal static Color? GetColorFromData(CustomData data, bool v2)
    {
        return data.GetColor(v2 ? V2_COLOR : COLOR);
    }
}
