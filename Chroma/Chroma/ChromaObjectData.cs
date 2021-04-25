namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static Chroma.Plugin;

    internal static class ChromaObjectDataManager
    {
        private static Dictionary<BeatmapObjectData, ChromaObjectData> _chromaObjectDatas;
        private static Dictionary<BeatmapObjectData, ChromaNoodleData> _chromaNoodleDatas;

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
                    throw new InvalidOperationException($"ChromaObjectData was not of type {typeof(T).Name}");
                }
            }

            return default;
        }

        internal static ChromaNoodleData TryGetNoodleData(BeatmapObjectData beatmapObjectData)
        {
            if (_chromaNoodleDatas.TryGetValue(beatmapObjectData, out ChromaNoodleData chromaNoodleData))
            {
                return chromaNoodleData;
            }

            return default;
        }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            _chromaObjectDatas = new Dictionary<BeatmapObjectData, ChromaObjectData>();
            if (NoodleExtensionsInstalled)
            {
                _chromaNoodleDatas = new Dictionary<BeatmapObjectData, ChromaNoodleData>();
            }

            foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectsData)
            {
                try
                {
                    ChromaObjectData chromaObjectData;
                    dynamic customData;

                    switch (beatmapObjectData)
                    {
                        case CustomNoteData customNoteData:
                            customData = customNoteData.customData;
                            chromaObjectData = new ChromaNoteData()
                            {
                                Color = ChromaUtils.GetColorFromData(customData),
                                DisableSpawnEffect = Trees.at(customData, DISABLESPAWNEFFECT),
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

                    if (NoodleExtensionsInstalled)
                    {
                        ApplyNoodleData(customData, beatmapObjectData, beatmapData);
                    }

                    _chromaObjectDatas.Add(beatmapObjectData, chromaObjectData);
                }
                catch (Exception e)
                {
                    ChromaLogger.Log($"Could not create ChromaObjectData for object {beatmapObjectData.GetType().Name} at {beatmapObjectData.time}", IPA.Logging.Logger.Level.Error);
                    ChromaLogger.Log(e, IPA.Logging.Logger.Level.Error);
                }
            }
        }

        private static void ApplyNoodleData(dynamic dynData, BeatmapObjectData beatmapObjectData, IReadonlyBeatmapData beatmapData)
        {
            if (NoodleExtensions.NoodleController.NoodleExtensionsActive)
            {
                ChromaNoodleData chromaNoodleData = new ChromaNoodleData();

                dynamic animationObjectDyn = Trees.at(dynData, ANIMATION);
                Dictionary<string, PointDefinition> pointDefinitions = Trees.at(((CustomBeatmapData)beatmapData).customData, "pointDefinitions");

                NoodleExtensions.Animation.AnimationHelper.TryGetPointData(animationObjectDyn, COLOR, out PointDefinition localColor, pointDefinitions);

                chromaNoodleData.LocalPathColor = localColor;

                chromaNoodleData.Track = NoodleExtensions.Animation.AnimationHelper.GetTrackPreload(dynData, beatmapData);

                _chromaNoodleDatas.Add(beatmapObjectData, chromaNoodleData);
            }
        }
    }

    internal class ChromaNoteData : ChromaObjectData
    {
        internal Color? InternalColor { get; set; }

        internal bool? DisableSpawnEffect { get; set; }
    }

    internal class ChromaObjectData
    {
        internal Color? Color { get; set; }
    }

    internal class ChromaNoodleData
    {
        internal Track Track { get; set; }

        internal PointDefinition LocalPathColor { get; set; }
    }
}
