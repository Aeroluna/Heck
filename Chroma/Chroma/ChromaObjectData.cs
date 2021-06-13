namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using UnityEngine;
    using static Chroma.Plugin;

    internal static class ChromaObjectDataManager
    {
        private static Dictionary<BeatmapObjectData, ChromaObjectData> _chromaObjectDatas;

        internal static T TryGetObjectData<T>(BeatmapObjectData beatmapObjectData)
        {
            if (_chromaObjectDatas.TryGetValue(beatmapObjectData, out ChromaObjectData chromaObjectData))
            {
                if (chromaObjectData is T t)
                {
                    return t;
                }
                else
                {
                    throw new InvalidOperationException($"ChromaObjectData was not of correct type. Expected: {typeof(T).Name}, was: {chromaObjectData.GetType().Name}");
                }
            }

            return default;
        }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            _chromaObjectDatas = new Dictionary<BeatmapObjectData, ChromaObjectData>();

            foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectsData)
            {
                try
                {
                    ChromaObjectData chromaObjectData;
                    Dictionary<string, object> customData;

                    switch (beatmapObjectData)
                    {
                        case CustomNoteData customNoteData:
                            customData = customNoteData.customData;
                            chromaObjectData = new ChromaNoteData()
                            {
                                Color = ChromaUtils.GetColorFromData(customData),
                                DisableSpawnEffect = customData.Get<bool?>(DISABLESPAWNEFFECT),
                            };
                            break;

                        case CustomObstacleData customObstacleData:
                            customData = customObstacleData.customData;
                            chromaObjectData = new ChromaObjectData()
                            {
                                Color = ChromaUtils.GetColorFromData(customData),
                            };
                            break;

                        case CustomWaypointData customWaypointData:
                            customData = customWaypointData.customData;
                            chromaObjectData = new ChromaObjectData();
                            break;

                        default:
                            continue;
                    }

                    if (chromaObjectData != null)
                    {
                        Dictionary<string, object> animationObjectDyn = customData.Get<Dictionary<string, object>>(ANIMATION);
                        if (animationObjectDyn != null)
                        {
                            Dictionary<string, PointDefinition> pointDefinitions = ((CustomBeatmapData)beatmapData).customData.Get<Dictionary<string, PointDefinition>>("pointDefinitions");

                            Heck.Animation.AnimationHelper.TryGetPointData(animationObjectDyn, COLOR, out PointDefinition localColor, pointDefinitions);

                            chromaObjectData.LocalPathColor = localColor;
                        }

                        chromaObjectData.Track = Heck.Animation.AnimationHelper.GetTrack(customData, beatmapData);

                        _chromaObjectDatas.Add(beatmapObjectData, chromaObjectData);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Logger.Log($"Could not create ChromaObjectData for object {beatmapObjectData.GetType().Name} at {beatmapObjectData.time}", IPA.Logging.Logger.Level.Error);
                    Plugin.Logger.Log(e, IPA.Logging.Logger.Level.Error);
                }
            }
        }
    }

    internal class ChromaNoteData : ChromaObjectData
    {
        internal bool? DisableSpawnEffect { get; set; }
    }

    internal class ChromaObjectData
    {
        internal Color? Color { get; set; }

        internal Track Track { get; set; }

        internal PointDefinition LocalPathColor { get; set; }
    }
}
