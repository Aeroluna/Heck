using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using Newtonsoft.Json;

namespace Chroma.EnvironmentEnhancement.Saved
{
    [JsonConverter(typeof(FeaturesDataConverter))]
    internal readonly struct Features
    {
        internal Features(bool useChromaEvents, EnvironmentEffectsFilterPreset? forcedPreset, List<CustomBeatmapSaveData.BasicEventData>? basicEventDatas)
        {
            UseChromaEvents = useChromaEvents;
            ForcedPreset = forcedPreset;
            BasicEventDatas = basicEventDatas;
        }

        internal bool UseChromaEvents { get; }

        internal EnvironmentEffectsFilterPreset? ForcedPreset { get; }

        internal List<CustomBeatmapSaveData.BasicEventData>? BasicEventDatas { get; }

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
                List<BeatmapSaveData.BasicEventData> basicBeatmapEvents = new();
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
                            CustomBeatmapSaveData.DeserializeBasicEventArray(reader, basicBeatmapEvents);
                            break;
                    }
                }

                return new Features(useChromaEvents, forcedPreset, basicBeatmapEvents.Count > 0 ? basicBeatmapEvents.Cast<CustomBeatmapSaveData.BasicEventData>().ToList() : null);
            }
        }
    }
}
