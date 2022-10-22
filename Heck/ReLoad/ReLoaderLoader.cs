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

        [UsedImplicitly]
#pragma warning disable 8618
        private ReLoaderLoader(
            CustomLevelLoader customLevelLoader)
#pragma warning restore 8618
        {
            _customLevelLoader = customLevelLoader;
        }

        public void Reload(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap is not
                    CustomDifficultyBeatmap { level: CustomPreviewBeatmapLevel customPreviewBeatmapLevel } customDifficultyBeatmap)
            {
                Log.Logger.Log("Cannot reload non-custom map.", Logger.Level.Error);
                return;
            }

            Log.Logger.Log("Reloaded beatmap.", Logger.Level.Trace);

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
