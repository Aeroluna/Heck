using System;
using System.Linq;
using BeatmapSaveDataVersion3;
using IPA.Logging;
using IPA.Utilities;
using JetBrains.Annotations;

namespace Heck.ReLoad
{
    public class ReLoaderLoader
    {
        private static readonly FieldAccessor<CustomDifficultyBeatmap, BeatmapSaveData>.Accessor _beatmapSaveDataAccessor
            = FieldAccessor<CustomDifficultyBeatmap, BeatmapSaveData>.GetAccessor("<beatmapSaveData>k__BackingField");

        private readonly CustomLevelLoader _customLevelLoader;
        private readonly BeatmapDataCache _beatmapDataCache;

        [UsedImplicitly]
#pragma warning disable 8618
        private ReLoaderLoader(
            CustomLevelLoader customLevelLoader,
            BeatmapDataCache beatmapDataCache)
#pragma warning restore 8618
        {
            _customLevelLoader = customLevelLoader;
            _beatmapDataCache = beatmapDataCache;
        }

        public void Reload(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap is not
                    CustomDifficultyBeatmap { level: CustomPreviewBeatmapLevel customPreviewBeatmapLevel } customDifficultyBeatmap)
            {
                Log.Logger.Log("Cannot ReLoad non-custom map.", Logger.Level.Error);
                return;
            }

            Log.Logger.Log("ReLoaded beatmap.", Logger.Level.Trace);

            _beatmapDataCache.difficultyBeatmap = null;

            BeatmapDifficulty levelDiff = customDifficultyBeatmap.difficulty;
            StandardLevelInfoSaveData standardLevelInfoSaveData = customPreviewBeatmapLevel.standardLevelInfoSaveData;
            BeatmapCharacteristicSO levelCharacteristic = customDifficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic;
            StandardLevelInfoSaveData.DifficultyBeatmap standardLevelInfoSaveDataDifficultyBeatmap = standardLevelInfoSaveData.difficultyBeatmapSets.First(
                x => x.beatmapCharacteristicName == levelCharacteristic.serializedName).difficultyBeatmaps.First(
                x => x.difficulty == levelDiff.ToString());

            Tuple<BeatmapSaveData, BeatmapDataBasicInfo> tuple = _customLevelLoader.LoadBeatmapDataBasicInfo(
                customPreviewBeatmapLevel.customLevelPath,
                standardLevelInfoSaveDataDifficultyBeatmap.beatmapFilename,
                standardLevelInfoSaveData);

            _beatmapSaveDataAccessor(ref customDifficultyBeatmap) = tuple.Item1;
        }
    }
}
