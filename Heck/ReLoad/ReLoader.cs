using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck.Animation;
using Heck.Deserialize;
using Heck.HarmonyPatches.UntransformedData;
using Heck.Settings;
using IPA.Utilities;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;
#if PRE_V1_37_1
using System.Threading.Tasks;
#endif

namespace Heck.ReLoad;

public class ReLoader : ITickable
{
    private static readonly FieldAccessor<BeatmapData, ISortedList<BeatmapDataItem>>.Accessor _allBeatmapDataAccessor
        = FieldAccessor<BeatmapData, ISortedList<BeatmapDataItem>>.GetAccessor(nameof(BeatmapData._allBeatmapData));

    private static readonly FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.Accessor
        _beatmapDataItemsPerTypeAccessor
            = FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.GetAccessor(
                nameof(BeatmapData._beatmapDataItemsPerTypeAndId));

    private static readonly FieldAccessor<BeatmapData, BeatmapObjectsInTimeRowProcessor>.Accessor
        _beatmapObjectsInTimeRowProcessorAccessor
            = FieldAccessor<BeatmapData, BeatmapObjectsInTimeRowProcessor>.GetAccessor(
                nameof(BeatmapData._beatmapObjectsInTimeRowProcessor));

    private static readonly FieldAccessor<CustomBeatmapData, Version>.Accessor _versionAccessor
        = FieldAccessor<CustomBeatmapData, Version>.GetAccessor("<version>k__BackingField");

    private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _customDataAccessor
        = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<customData>k__BackingField");

    private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _beatmapCustomDataAccessor
        = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<beatmapCustomData>k__BackingField");

    private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _levelCustomDataAccessor
        = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<levelCustomData>k__BackingField");

    private static readonly FieldAccessor<CustomBeatmapData, List<BeatmapObjectData>>.Accessor
        _beatmapObjectDatasAccessor
            = FieldAccessor<CustomBeatmapData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectDatas");

    private static readonly FieldAccessor<CustomBeatmapData, List<BeatmapEventData>>.Accessor _beatmapEventDatasAccessor
        = FieldAccessor<CustomBeatmapData, List<BeatmapEventData>>.GetAccessor("_beatmapEventDatas");

    private static readonly FieldAccessor<CustomBeatmapData, List<CustomEventData>>.Accessor _customEventDatasAccessor
        = FieldAccessor<CustomBeatmapData, List<CustomEventData>>.GetAccessor("_customEventDatas");

    private readonly SiraLog _log;
    private readonly AudioTimeSyncController _audioTimeSyncController;
    private readonly ReLoaderLoader _reLoaderLoader;
#if !PRE_V1_37_1
    private readonly PauseMenuManager.InitData _pauseInitData;
#else
    private readonly IDifficultyBeatmap _difficultyBeatmap;
#endif
    private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
    private readonly IReadonlyBeatmapData _beatmapData;
    private readonly BeatmapObjectManager _beatmapObjectManager;
    private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
    private readonly BeatmapCallbacksController _beatmapCallbacksController;
    private readonly IGamePause _gamePause;
    private readonly PauseMenuManager _pauseMenuManager;
    private readonly DeserializerManager _deserializerManager;
    private readonly bool _leftHanded;
    private readonly Dictionary<string, Track> _beatmapTracks;
    private readonly DiContainer _container;
    private readonly Config.ReLoaderSettings _config;
    private readonly bool _reloadable;

    private float _songStartTime;

    [UsedImplicitly]
    private ReLoader(
        SiraLog log,
        AudioTimeSyncController audioTimeSyncController,
        AudioTimeSyncController.InitData audioTimeSyncControllerInitData,
#if !PRE_V1_37_1
        PauseMenuManager.InitData pauseInitData,
#else
        IDifficultyBeatmap difficultyBeatmap,
#endif
        ReLoaderLoader reLoaderLoader,
        StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO,
        GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
        IReadonlyBeatmapData beatmapData,
        BeatmapObjectManager beatmapObjectManager,
        NoteCutSoundEffectManager noteCutSoundEffectManager,
        BeatmapCallbacksController beatmapCallbacksController,
        IGamePause gamePause,
        PauseMenuManager pauseMenuManager,
        DeserializerManager deserializerManager,
        [Inject(Id = HeckController.LEFT_HANDED_ID)]
        bool leftHanded,
        Dictionary<string, Track> beatmapTracks,
        DiContainer container,
        Config.ReLoaderSettings config)
    {
        _log = log;
        _audioTimeSyncController = audioTimeSyncController;
        _songStartTime = audioTimeSyncControllerInitData.startSongTime;
        _reLoaderLoader = reLoaderLoader;
        _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        _beatmapData = beatmapData;
        _beatmapObjectManager = beatmapObjectManager;
        _noteCutSoundEffectManager = noteCutSoundEffectManager;
        _beatmapCallbacksController = beatmapCallbacksController;
        _gamePause = gamePause;
        _pauseMenuManager = pauseMenuManager;
        _deserializerManager = deserializerManager;
        _leftHanded = leftHanded;
        _beatmapTracks = beatmapTracks;
        _container = container;
        _config = config;
#if !PRE_V1_37_1
        _pauseInitData = pauseInitData;
        if (beatmapData is CustomBeatmapData)
#else
        _difficultyBeatmap = difficultyBeatmap;
        if (difficultyBeatmap is CustomDifficultyBeatmap)
#endif
        {
            _reloadable = true;
        }
        else
        {
            log.Error("Cannot reload a non-custom map");
            _reloadable = false;
        }
    }

    public event Action? Reloaded;

    public event Action? Rewinded;

    public void Tick()
    {
        float increment = _config.ScrubIncrement;
        if (Input.GetKeyDown(_config.SaveTime))
        {
            // Set new start time
            _songStartTime = _audioTimeSyncController.songTime;
            _log.Trace($"Saved: [{_songStartTime}]");
        }
        else if (_reloadable && Input.GetKeyDown(_config.Reload))
        {
            Reload();
            if (!Input.GetKeyDown(_config.JumpToSavedTime))
            {
                Rewind();
            }
        }

        if (Input.GetKeyDown(_config.JumpToSavedTime))
        {
            Rewind(_songStartTime);
            _log.Trace($"Loaded to: [{_songStartTime}]");
        }
        else if (Input.GetKeyDown(_config.ScrubBackwards))
        {
            Rewind(Math.Max(_audioTimeSyncController.songTime - increment, 0));
        }
        else if (Input.GetKeyDown(_config.ScrubForwards))
        {
            SetSongTime(_audioTimeSyncController.songTime + increment);
        }
    }

    // very illegal
    private static void FillBeatmapData(IReadonlyBeatmapData source, IReadonlyBeatmapData dest)
    {
        BeatmapData sourceData = (BeatmapData)source;
        BeatmapData destData = (BeatmapData)dest;
        CustomBeatmapData sourceCustomData = (CustomBeatmapData)source;
        CustomBeatmapData destCustomData = (CustomBeatmapData)dest;
        _allBeatmapDataAccessor(ref destData) = sourceData._allBeatmapData;
        _beatmapDataItemsPerTypeAccessor(ref destData) = sourceData._beatmapDataItemsPerTypeAndId;
        _beatmapObjectsInTimeRowProcessorAccessor(ref destData) = sourceData._beatmapObjectsInTimeRowProcessor;
        _versionAccessor(ref destCustomData) = _versionAccessor(ref sourceCustomData);
        _customDataAccessor(ref destCustomData) = _customDataAccessor(ref sourceCustomData);
        _beatmapCustomDataAccessor(ref destCustomData) = _beatmapCustomDataAccessor(ref sourceCustomData);
        _levelCustomDataAccessor(ref destCustomData) = _levelCustomDataAccessor(ref sourceCustomData);
        _beatmapObjectDatasAccessor(ref destCustomData) = _beatmapObjectDatasAccessor(ref sourceCustomData);
        _beatmapEventDatasAccessor(ref destCustomData) = _beatmapEventDatasAccessor(ref sourceCustomData);
        _customEventDatasAccessor(ref destCustomData) = _customEventDatasAccessor(ref sourceCustomData);
    }

    private void Reload()
    {
        bool paused = _gamePause.isPaused;
        if (!paused)
        {
            _gamePause.Pause();
        }

#if !PRE_V1_37_1
        _reLoaderLoader.Reload();
        ////_gameplayCoreSceneSetupData.LoadTransformedBeatmapData(); // you wish it was that simple
        ////IReadonlyBeatmapData beatmapData = _gameplayCoreSceneSetupData.transformedBeatmapData;
        BeatmapLevel beatmapLevel = _gameplayCoreSceneSetupData.beatmapLevel;
        BeatmapKey beatmapKey = _gameplayCoreSceneSetupData.beatmapKey;
        float beatsPerMinute = beatmapLevel.beatsPerMinute;
        EnvironmentName environmentName = beatmapLevel.GetEnvironmentName(beatmapKey.beatmapCharacteristic, beatmapKey.difficulty);
#if !PRE_V1_39_1
        EnvironmentInfoSO targetEnvironmentInfo = _gameplayCoreSceneSetupData.targetEnvironmentInfo;
#else
        EnvironmentInfoSO targetEnvironmentInfo = _gameplayCoreSceneSetupData.environmentInfo;
#endif
        IReadonlyBeatmapData beatmapData = _gameplayCoreSceneSetupData.TransformBeatmapData(
            _gameplayCoreSceneSetupData._beatmapDataLoader.LoadBeatmapData(
                _gameplayCoreSceneSetupData.beatmapLevelData,
                beatmapKey,
                beatsPerMinute,
                environmentName == targetEnvironmentInfo.serializedName,
                targetEnvironmentInfo,
#if !PRE_V1_39_1
                _gameplayCoreSceneSetupData.originalEnvironmentInfo,
#endif
                BeatmapLevelDataVersion.Original,
                _gameplayCoreSceneSetupData.gameplayModifiers,
                _gameplayCoreSceneSetupData.playerSpecificSettings));
#else
        _reLoaderLoader.Reload(_difficultyBeatmap);
        IReadonlyBeatmapData beatmapData =
            Task.Run<IReadonlyBeatmapData>(
                    async () => await _gameplayCoreSceneSetupData.GetTransformedBeatmapDataAsync())
                .Result;
#endif
        FillBeatmapData(beatmapData, _beatmapData);

        HeckGameplayCoreSceneSetupData heckGameplayCoreSceneSetupData =
            (HeckGameplayCoreSceneSetupData)_gameplayCoreSceneSetupData;
#if !PRE_V1_37_1
        _deserializerManager.DeserializeBeatmapData(
            _pauseInitData.beatmapLevel,
#else
        _deserializerManager.DeserializeBeatmapData(
            _difficultyBeatmap,
#endif
            (CustomBeatmapData)beatmapData,
            heckGameplayCoreSceneSetupData.UntransformedBeatmapData,
            _leftHanded,
            out Dictionary<string, Track> beatmapTracks,
            out HashSet<(object? Id, DeserializedData DeserializedData)> deserializedDatas);
        _beatmapTracks.Clear();
        foreach ((string key, Track value) in beatmapTracks)
        {
            _beatmapTracks.Add(key, value);
        }

        deserializedDatas.Do(n => _container.ResolveId<DeserializedData>(n.Id).Remap(n.DeserializedData));

        Reloaded?.Invoke();

        GarbageCollector.GCMode = GarbageCollector.Mode.Manual;
        GC.Collect();

        if (_pauseMenuManager.enabled)
        {
            _pauseMenuManager.ContinueButtonPressed();
        }
        else
        {
            _gamePause.Resume();
        }
    }

    private void Rewind(float songTime)
    {
        SetSongTime(songTime);
        Rewind();
    }

    private void Rewind()
    {
        _beatmapObjectManager._allBeatmapObjects.ForEach(
            n =>
            {
                ((MonoBehaviour)n).gameObject.SetActive(true);
                n.Dissolve(1f);
                ////((MonoBehaviour)n).gameObject.SetActive(false);
            });

        _noteCutSoundEffectManager._noteCutSoundEffectPoolContainer.activeItems.ForEach(n => n.StopPlayingAndFinish());
        _noteCutSoundEffectManager._prevNoteATime = 0;
        _noteCutSoundEffectManager._prevNoteBTime = 0;

        _beatmapCallbacksController._callbacksInTimes.Values.Do(n => n.lastProcessedNode = null);
        _beatmapCallbacksController._prevSongTime = 0;

        _beatmapTracks.Values.Do(n => n.NullProperties());

        Rewinded?.Invoke();
    }

    private void SetSongTime(float songTime)
    {
        _audioTimeSyncController._startSongTime = songTime;
        _audioTimeSyncController.SeekTo(0);
    }
}
