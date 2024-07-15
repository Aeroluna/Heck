using JetBrains.Annotations;
#if !LATEST
using System;
using System.Linq;
using BeatmapSaveDataVersion3;
using IPA.Utilities;
using SiraUtil.Logging;
#endif

namespace Heck.ReLoad
{
    public class ReLoaderLoader
    {
#if LATEST
        private readonly BeatmapDataLoader _beatmapDataLoader;

        [UsedImplicitly]
        public ReLoaderLoader(BeatmapDataLoader beatmapDataLoader)
        {
            _beatmapDataLoader = beatmapDataLoader;
        }

        public void Reload()
        {
            _beatmapDataLoader._lastUsedBeatmapDataCache = default;
        }
#else
        private static readonly FieldAccessor<CustomDifficultyBeatmap, BeatmapSaveData>.Accessor _beatmapSaveDataAccessor
            = FieldAccessor<CustomDifficultyBeatmap, BeatmapSaveData>.GetAccessor("<beatmapSaveData>k__BackingField");

        private readonly SiraLog _log;
        private readonly CustomLevelLoader _customLevelLoader;
        private readonly BeatmapDataCache _beatmapDataCache;

        [UsedImplicitly]
#pragma warning disable 8618
        private ReLoaderLoader(
            SiraLog log,
            CustomLevelLoader customLevelLoader,
            BeatmapDataCache beatmapDataCache)
#pragma warning restore 8618
        {
            _log = log;
            _customLevelLoader = customLevelLoader;
            _beatmapDataCache = beatmapDataCache;
        }

        public void Reload(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap is not
                    CustomDifficultyBeatmap { level: CustomPreviewBeatmapLevel customPreviewBeatmapLevel } customDifficultyBeatmap)
            {
                _log.Error("Cannot ReLoad non-custom map");
                return;
            }

            _log.Trace("ReLoaded beatmap");

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
#endif
    }
}
