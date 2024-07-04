﻿using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Newtonsoft.Json;
#if LATEST
using _BasicEventData = BeatmapSaveDataVersion3.BasicEventData;
#else
using _BasicEventData = BeatmapSaveDataVersion3.BeatmapSaveData.BasicEventData;
#endif

namespace Chroma.EnvironmentEnhancement.Saved
{
    [JsonConverter(typeof(FeaturesDataConverter))]
    internal readonly struct Features
    {
        internal Features(bool useChromaEvents, EnvironmentEffectsFilterPreset? forcedPreset, List<Version3CustomBeatmapSaveData.BasicEventSaveData>? basicEventDatas)
        {
            UseChromaEvents = useChromaEvents;
            ForcedPreset = forcedPreset;
            BasicEventDatas = basicEventDatas;
        }

        internal bool UseChromaEvents { get; }

        internal EnvironmentEffectsFilterPreset? ForcedPreset { get; }

        internal List<Version3CustomBeatmapSaveData.BasicEventSaveData>? BasicEventDatas { get; }

        private class FeaturesDataConverter : JsonConverter<Features>
        {
            public override void WriteJson(JsonWriter writer, Features value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override Features ReadJson(JsonReader reader, Type objectType, Features existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                bool useChromaEvents = false;
                EnvironmentEffectsFilterPreset? forcedPreset = null;
                List<_BasicEventData> basicBeatmapEvents = new();
                while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value)
                    {
                        default:
                            reader.Skip();
                            break;

                        case "useChromaEvents":
                            useChromaEvents = reader.ReadAsBoolean() ?? useChromaEvents;
                            break;

                        case "forceEffectsFilter":
                            string? value = reader.ReadAsString();
                            if (value != null && Enum.TryParse(value, out EnvironmentEffectsFilterPreset result))
                            {
                                forcedPreset = result;
                            }

                            break;

                        case "basicBeatmapEvents":
                            Version3CustomBeatmapSaveData.DeserializeBasicEventArray(reader, basicBeatmapEvents);
                            break;
                    }
                }

                return new Features(useChromaEvents, forcedPreset, basicBeatmapEvents.Count > 0 ? basicBeatmapEvents.Cast<Version3CustomBeatmapSaveData.BasicEventSaveData>().ToList() : null);
            }
        }
    }
}
