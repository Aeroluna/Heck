using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class CustomDataManager
    {
        [EarlyDeserializer]
        internal static void DeserializeEarly(
            CustomBeatmapData beatmapData,
            TrackBuilder trackBuilder,
            List<CustomEventData> customEventDatas)
        {
            bool v2 = beatmapData.version2_6_0AndEarlier;
            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            trackBuilder.AddTrack(customEventData.customData.Get<string>(v2 ? V2_TRACK : TRACK)
                                                  ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            trackBuilder.AddTrack(customEventData.customData.Get<string>(v2 ? V2_PARENT_TRACK : PARENT_TRACK)
                                                  ?? throw new InvalidOperationException("Parent track was not defined."));
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

        [ObjectsDeserializer]
        private static Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects(
            CustomBeatmapData beatmapData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            IReadOnlyList<BeatmapObjectData> beatmapObjectsDatas,
            bool leftHanded)
        {
            bool v2 = beatmapData.version2_6_0AndEarlier;
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();
            foreach (BeatmapObjectData beatmapObjectData in beatmapObjectsDatas)
            {
                CustomData customData = ((ICustomData)beatmapObjectData).customData;
                switch (beatmapObjectData)
                {
                    case CustomObstacleData customObstacleData:
                        dictionary.Add(beatmapObjectData, new NoodleObstacleData(customObstacleData, customData, pointDefinitions, beatmapTracks, v2, leftHanded));
                        break;

                    case CustomNoteData customNoteData:
                        dictionary.Add(beatmapObjectData, new NoodleNoteData(customNoteData, customData, pointDefinitions, beatmapTracks, v2, leftHanded));
                        break;

                    default:
                        dictionary.Add(beatmapObjectData, new NoodleObjectData(beatmapObjectData, customData, pointDefinitions, beatmapTracks, v2, leftHanded));
                        break;
                }
            }

            return dictionary;
        }

        [CustomEventsDeserializer]
        private static Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents(
            CustomBeatmapData beatmapData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> tracks,
            IReadOnlyList<CustomEventData> customEventDatas)
        {
            bool v2 = beatmapData.version2_6_0AndEarlier;
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in customEventDatas)
            {
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
                    Log.Logger.LogFailure(e, customEventData);
                }
            }

            return dictionary;
        }
    }
}
