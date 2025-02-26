﻿using NoodleExtensions.Managers;
using SiraUtil.Affinity;
#if LATEST
using _NoteSpawnData = NoteSpawnData;
using _ObstacleSpawnData = ObstacleSpawnData;
using _SliderSpawnData = SliderSpawnData;
#else
using _NoteSpawnData = BeatmapObjectSpawnMovementData.NoteSpawnData;
using _ObstacleSpawnData = BeatmapObjectSpawnMovementData.ObstacleSpawnData;
using _SliderSpawnData = BeatmapObjectSpawnMovementData.SliderSpawnData;
#endif

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing;

internal class NoodledSpawnMovementData : IAffinity
{
    private readonly SpawnDataManager _spawnDataManager;

    private NoodledSpawnMovementData(SpawnDataManager spawnDataManager)
    {
        _spawnDataManager = spawnDataManager;
    }

    [AffinityPrefix]
    [AffinityPatch(
        typeof(BeatmapObjectSpawnMovementData),
        nameof(BeatmapObjectSpawnMovementData.GetJumpingNoteSpawnData))]
    private bool NoteNoodlePatch(
        BeatmapObjectSpawnMovementData __instance,
        NoteData noteData,
        ref _NoteSpawnData __result)
    {
        return _spawnDataManager.GetJumpingNoteSpawnData(noteData, ref __result);
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(BeatmapObjectSpawnMovementData), nameof(BeatmapObjectSpawnMovementData.GetObstacleSpawnData))]
    private bool ObstacleNoodlePatch(
        ObstacleData obstacleData,
        ref _ObstacleSpawnData __result)
    {
        return _spawnDataManager.GetObstacleSpawnData(obstacleData, ref __result);
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(BeatmapObjectSpawnMovementData), nameof(BeatmapObjectSpawnMovementData.GetSliderSpawnData))]
    private bool SliderNoodlePatch(
        BeatmapObjectSpawnMovementData __instance,
        SliderData sliderData,
        ref _SliderSpawnData __result)
    {
        return _spawnDataManager.GetSliderSpawnData(sliderData, ref __result);
    }
}
