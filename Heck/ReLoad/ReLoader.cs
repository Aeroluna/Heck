using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck.Animation;
using Heck.HarmonyPatches;
using Heck.Settings;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Logger = IPA.Logging.Logger;

namespace Heck.ReLoad
{
    public class ReLoader : ITickable
    {
        private static readonly FieldAccessor<BeatmapData, ISortedList<BeatmapDataItem>>.Accessor _allBeatmapDataAccessor
            = FieldAccessor<BeatmapData, ISortedList<BeatmapDataItem>>.GetAccessor(nameof(BeatmapData._allBeatmapData));

        private static readonly FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.Accessor _beatmapDataItemsPerTypeAccessor
            = FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.GetAccessor(nameof(BeatmapData._beatmapDataItemsPerTypeAndId));

        private static readonly FieldAccessor<BeatmapData, BeatmapObjectsInTimeRowProcessor>.Accessor _beatmapObjectsInTimeRowProcessorAccessor
            = FieldAccessor<BeatmapData, BeatmapObjectsInTimeRowProcessor>.GetAccessor(nameof(BeatmapData._beatmapObjectsInTimeRowProcessor));

        private static readonly FieldAccessor<CustomBeatmapData, bool>.Accessor _version2_6_0AndEarlierAccessor
            = FieldAccessor<CustomBeatmapData, bool>.GetAccessor("<version2_6_0AndEarlier>k__BackingField");

        private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _customDataAccessor
            = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<customData>k__BackingField");

        private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _beatmapCustomDataAccessor
            = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<beatmapCustomData>k__BackingField");

        private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _levelCustomDataAccessor
            = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<levelCustomData>k__BackingField");

        private static readonly FieldAccessor<CustomBeatmapData, List<BeatmapObjectData>>.Accessor _beatmapObjectDatasAccessor
            = FieldAccessor<CustomBeatmapData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectDatas");

        private static readonly FieldAccessor<CustomBeatmapData, List<BeatmapEventData>>.Accessor _beatmapEventDatasAccessor
            = FieldAccessor<CustomBeatmapData, List<BeatmapEventData>>.GetAccessor("_beatmapEventDatas");

        private static readonly FieldAccessor<CustomBeatmapData, List<CustomEventData>>.Accessor _customEventDatasAccessor
            = FieldAccessor<CustomBeatmapData, List<CustomEventData>>.GetAccessor("_customEventDatas");

        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly ReLoaderLoader _reLoaderLoader;
        private readonly IDifficultyBeatmap _difficultyBeatmap;
        private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private readonly IReadonlyBeatmapData _beatmapData;
        private readonly BeatmapObjectManager _beatmapObjectManager;
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly BeatmapCallbacksController _beatmapCallbacksController;
        private readonly IGamePause _gamePause;
        private readonly PauseMenuManager _pauseMenuManager;
        private readonly bool _leftHanded;
        private readonly Dictionary<string, Track> _beatmapTracks;
        private readonly DiContainer _container;
        private readonly Config.ReLoaderSettings _config;
        private readonly bool _reloadable;

        private float _songStartTime;

        [UsedImplicitly]
        private ReLoader(
            AudioTimeSyncController audioTimeSyncController,
            AudioTimeSyncController.InitData audioTimeSyncControllerInitData,
            IDifficultyBeatmap difficultyBeatmap,
            ReLoaderLoader reLoaderLoader,
            StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO,
            GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
            IReadonlyBeatmapData beatmapData,
            BeatmapObjectManager beatmapObjectManager,
            NoteCutSoundEffectManager noteCutSoundEffectManager,
            BeatmapCallbacksController beatmapCallbacksController,
            IGamePause gamePause,
            PauseMenuManager pauseMenuManager,
            [Inject(Id = HeckController.LEFT_HANDED_ID)] bool leftHanded,
            Dictionary<string, Track> beatmapTracks,
            DiContainer container,
            Config.ReLoaderSettings config)
        {
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
            _leftHanded = leftHanded;
            _beatmapTracks = beatmapTracks;
            _container = container;
            _config = config;
            _difficultyBeatmap = difficultyBeatmap;

            if (difficultyBeatmap is CustomDifficultyBeatmap)
            {
                _reloadable = true;
            }
            else
            {
                Log.Logger.Log("Cannot reload a non-custom map!", Logger.Level.Error);
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
                Log.Logger.Log($"Saved: [{_songStartTime}].", Logger.Level.Trace);
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
                Log.Logger.Log($"Loaded to: [{_songStartTime}].", Logger.Level.Trace);
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
            _version2_6_0AndEarlierAccessor(ref destCustomData) = _version2_6_0AndEarlierAccessor(ref sourceCustomData);
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

            _reLoaderLoader.Reload(_difficultyBeatmap);
            IReadonlyBeatmapData beatmapData = Task.Run<IReadonlyBeatmapData>(async () => await _gameplayCoreSceneSetupData.GetTransformedBeatmapDataAsync()).Result;
            FillBeatmapData(beatmapData, _beatmapData);

            HeckinGameplayCoreSceneSetupData heckinGameplayCoreSceneSetupData = (HeckinGameplayCoreSceneSetupData)_gameplayCoreSceneSetupData;
            DeserializerManager.DeserializeBeatmapData(
                (CustomBeatmapData)beatmapData,
                heckinGameplayCoreSceneSetupData.UntransformedBeatmapData,
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
            _beatmapObjectManager._allBeatmapObjects.ForEach(n =>
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
}
