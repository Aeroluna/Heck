namespace Chroma
{
    using System.Collections.Generic;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static Chroma.Plugin;

    internal static class ChromaObjectDataManager
    {
        internal static Dictionary<BeatmapObjectData, ChromaObjectData> ChromaObjectDatas { get; private set; }

        internal static Dictionary<BeatmapObjectData, ChromaNoodleData> ChromaNoodleDatas { get; private set; }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            ChromaObjectDatas = new Dictionary<BeatmapObjectData, ChromaObjectData>();
            if (NoodleExtensionsInstalled)
            {
                ChromaNoodleDatas = new Dictionary<BeatmapObjectData, ChromaNoodleData>();
            }

            foreach (IReadonlyBeatmapLineData beatmapLineData in beatmapData.beatmapLinesData)
            {
                foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
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

                    ChromaObjectDatas.Add(beatmapObjectData, chromaObjectData);
                }
            }
        }

        private static void ApplyNoodleData(dynamic dynData, BeatmapObjectData beatmapObjectData, IReadonlyBeatmapData beatmapData)
        {
            if (NoodleExtensions.NoodleController.NoodleExtensionsActive)
            {
                ChromaNoodleData chromaNoodleData = new ChromaNoodleData();

                dynamic animationObjectDyn = Trees.at(dynData, "_animation");
                Dictionary<string, PointDefinition> pointDefinitions = Trees.at(((CustomBeatmapData)beatmapData).customData, "pointDefinitions");

                NoodleExtensions.Animation.AnimationHelper.TryGetPointData(animationObjectDyn, COLOR, out PointDefinition localColor, pointDefinitions);

                chromaNoodleData.LocalPathColor = localColor;

                chromaNoodleData.Track = NoodleExtensions.Animation.AnimationHelper.GetTrackPreload(dynData, beatmapData);

                ChromaNoodleDatas.Add(beatmapObjectData, chromaNoodleData);
            }
        }
    }

    internal class ChromaNoteData : ChromaObjectData
    {
        internal Color? Color0 { get; set; }

        internal Color? Color1 { get; set; }

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
