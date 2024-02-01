using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class CustomDataManager
    {
        [EarlyDeserializer]
        internal static void DeserializeEarly(
            CustomBeatmapData beatmapData,
            IDifficultyBeatmap difficultyBeatmap,
            TrackBuilder trackBuilder)
        {
            foreach (CustomEventData customEventData in beatmapData.customEventDatas)
            {
                bool v2 = customEventData.version2_6_0AndEarlier;
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            trackBuilder.AddFromCustomData(customEventData.customData, v2);
                            break;

                        case ASSIGN_TRACK_PARENT:
                            trackBuilder.AddFromCustomData(customEventData.customData, v2 ? V2_PARENT_TRACK : PARENT_TRACK);
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, customEventData, difficultyBeatmap);
                }
            }
        }

        [ObjectsDeserializer]
        private static Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects(
            CustomBeatmapData beatmapData,
            IDifficultyBeatmap difficultyBeatmap,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool leftHanded)
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();
            foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectDatas)
            {
                CustomData customData = ((ICustomData)beatmapObjectData).customData;
                switch (beatmapObjectData)
                {
                    case CustomObstacleData customObstacleData:
                        dictionary.Add(beatmapObjectData, new NoodleObstacleData(customObstacleData, customData, difficultyBeatmap, pointDefinitions, beatmapTracks, customObstacleData.version2_6_0AndEarlier, leftHanded));
                        break;

                    case CustomNoteData customNoteData:
                        dictionary.Add(beatmapObjectData, new NoodleNoteData(customNoteData, customData, difficultyBeatmap, pointDefinitions, beatmapTracks, customNoteData.version2_6_0AndEarlier, leftHanded));
                        break;

                    case CustomSliderData customSliderData:
                        dictionary.Add(beatmapObjectData, new NoodleSliderData(customSliderData, customData, difficultyBeatmap, pointDefinitions, beatmapTracks, customSliderData.version2_6_0AndEarlier, leftHanded));
                        break;

                    default:
                        bool v2 = beatmapObjectData is IVersionable { version2_6_0AndEarlier: true };
                        dictionary.Add(beatmapObjectData, new NoodleObjectData(beatmapObjectData, customData, difficultyBeatmap, pointDefinitions, beatmapTracks, v2, leftHanded));
                        break;
                }
            }

            return dictionary;
        }

        [CustomEventsDeserializer]
        private static Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents(
            CustomBeatmapData beatmapData,
            IDifficultyBeatmap difficultyBeatmap,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> tracks)
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in beatmapData.customEventDatas)
            {
                bool v2 = customEventData.version2_6_0AndEarlier;
                try
                {
                    CustomData data = customEventData.customData;
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            dictionary.Add(customEventData, new NoodlePlayerTrackEventData(data, tracks, v2));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            dictionary.Add(customEventData, new NoodleParentTrackEventData(data, tracks, v2));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, customEventData, difficultyBeatmap);
                }
            }

            return dictionary;
        }
    }
}
