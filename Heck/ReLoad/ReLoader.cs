using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck.Animation;
using Heck.HarmonyPatches;
using Heck.ReLoad;
using Heck.Settings;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Logger = IPA.Logging.Logger;

// TODO: Fix namespaces
// ReSharper disable once CheckNamespace
namespace Heck
{
    public class ReLoader : ITickable
    {
        private static readonly FieldAccessor<BeatmapData, ISortedList<BeatmapDataItem>>.Accessor _allBeatmapDataAccessor
            = FieldAccessor<BeatmapData, ISortedList<BeatmapDataItem>>.GetAccessor("_allBeatmapData");

        private static readonly FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.Accessor _beatmapDataItemsPerTypeAccessor
            = FieldAccessor<BeatmapData, BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>>.GetAccessor("_beatmapDataItemsPerTypeAndId");

        private static readonly FieldAccessor<BeatmapData, BeatmapObjectsInTimeRowProcessor>.Accessor _beatmapObjectsInTimeRowProcessorAccessor
            = FieldAccessor<BeatmapData, BeatmapObjectsInTimeRowProcessor>.GetAccessor("_beatmapObjectsInTimeRowProcessor");

        private static readonly FieldAccessor<CustomBeatmapData, bool>.Accessor _version2_6_0AndEarlierAccessor
            = FieldAccessor<CustomBeatmapData, bool>.GetAccessor("<version2_6_0AndEarlier>k__BackingField");

        private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _customDataAccessor
            = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<customData>k__BackingField");

        private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _beatmapCustomDataAccessor
            = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<beatmapCustomData>k__BackingField");

        private static readonly FieldAccessor<CustomBeatmapData, CustomData>.Accessor _levelCustomDataAccessor
            = FieldAccessor<CustomBeatmapData, CustomData>.GetAccessor("<levelCustomData>k__BackingField");

        private static readonly FieldAccessor<AudioTimeSyncController, float>.Accessor _startSongTimeAccessor
            = FieldAccessor<AudioTimeSyncController, float>.GetAccessor("_startSongTime");

        private static readonly FieldAccessor<BeatmapObjectManager, List<IBeatmapObjectController>>.Accessor _allBeatmapObjectsAccessor
            = FieldAccessor<BeatmapObjectManager, List<IBeatmapObjectController>>.GetAccessor("_allBeatmapObjects");

        private static readonly FieldAccessor<NoteCutSoundEffectManager, MemoryPoolContainer<NoteCutSoundEffect>>.Accessor _noteCutSoundEffectPoolContainerAccessor
            = FieldAccessor<NoteCutSoundEffectManager, MemoryPoolContainer<NoteCutSoundEffect>>.GetAccessor("_noteCutSoundEffectPoolContainer");

        private static readonly FieldAccessor<NoteCutSoundEffectManager, float>.Accessor _prevNoteATimeAccessor
            = FieldAccessor<NoteCutSoundEffectManager, float>.GetAccessor("_prevNoteATime");

        private static readonly FieldAccessor<NoteCutSoundEffectManager, float>.Accessor _prevNoteBTimeAccessor
            = FieldAccessor<NoteCutSoundEffectManager, float>.GetAccessor("_prevNoteBTime");

        private static readonly FieldAccessor<BeatmapCallbacksController, Dictionary<float, CallbacksInTime>>.Accessor _callbacksInTimesAccessor
            = FieldAccessor<BeatmapCallbacksController, Dictionary<float, CallbacksInTime>>.GetAccessor("_callbacksInTimes");

        private static readonly FieldAccessor<BeatmapCallbacksController, float>.Accessor _prevSongTimeAccessor
            = FieldAccessor<BeatmapCallbacksController, float>.GetAccessor("_prevSongTime");

        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly ReLoaderLoader _reLoaderLoader;
        private readonly IDifficultyBeatmap _difficultyBeatmap;
        private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private readonly IReadonlyBeatmapData _beatmapData;
        private readonly BeatmapObjectManager _beatmapObjectManager;
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly BeatmapCallbacksController _beatmapCallbacksController;
        private readonly IGamePause _gamePause;
        private readonly bool _leftHanded;
        private readonly Dictionary<string, Track> _beatmapTracks;
        private readonly DiContainer _container;
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
            [Inject(Id = HeckController.LEFT_HANDED_ID)] bool leftHanded,
            Dictionary<string, Track> beatmapTracks,
            DiContainer container)
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
            _leftHanded = leftHanded;
            _beatmapTracks = beatmapTracks;
            _container = container;
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
            HeckConfig.ReLoaderSettings config = HeckConfig.Instance.ReLoader;
            float increment = config.ScrubIncrement;
            if (Input.GetKeyDown(config.SaveTime))
            {
                // Set new start time
                _songStartTime = _audioTimeSyncController.songTime;
                Log.Logger.Log($"Saved: [{_songStartTime}].", Logger.Level.Trace);
            }
            else if (_reloadable && Input.GetKeyDown(config.Reload))
            {
                Reload();
                if (!Input.GetKeyDown(config.JumpToSavedTime))
                {
                    Rewind();
                }
            }

            if (Input.GetKeyDown(config.JumpToSavedTime))
            {
                Rewind(_songStartTime);
                Log.Logger.Log($"Loaded to: [{_songStartTime}].", Logger.Level.Trace);
            }
            else if (Input.GetKeyDown(config.ScrubBackwards))
            {
                Rewind(Math.Max(_audioTimeSyncController.songTime - increment, 0));
            }
            else if (Input.GetKeyDown(config.ScrubForwards))
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
            _allBeatmapDataAccessor(ref destData) = _allBeatmapDataAccessor(ref sourceData);
            _beatmapDataItemsPerTypeAccessor(ref destData) = _beatmapDataItemsPerTypeAccessor(ref sourceData);
            _beatmapObjectsInTimeRowProcessorAccessor(ref destData) = _beatmapObjectsInTimeRowProcessorAccessor(ref sourceData);
            _version2_6_0AndEarlierAccessor(ref destCustomData) = _version2_6_0AndEarlierAccessor(ref sourceCustomData);
            _customDataAccessor(ref destCustomData) = _customDataAccessor(ref sourceCustomData);
            _beatmapCustomDataAccessor(ref destCustomData) = _beatmapCustomDataAccessor(ref sourceCustomData);
            _levelCustomDataAccessor(ref destCustomData) = _levelCustomDataAccessor(ref sourceCustomData);
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
            if (!paused)
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
            BeatmapObjectManager beatmapObjectManager = _beatmapObjectManager;
            _allBeatmapObjectsAccessor(ref beatmapObjectManager).ForEach(n =>
            {
                ((MonoBehaviour)n).gameObject.SetActive(true);
                n.Dissolve(1f);
                ((MonoBehaviour)n).gameObject.SetActive(false);
            });

            NoteCutSoundEffectManager noteCutSoundEffectManager = _noteCutSoundEffectManager;
            _noteCutSoundEffectPoolContainerAccessor(ref noteCutSoundEffectManager).activeItems.ForEach(n => n.StopPlayingAndFinish());
            _prevNoteATimeAccessor(ref noteCutSoundEffectManager) = 0;
            _prevNoteBTimeAccessor(ref noteCutSoundEffectManager) = 0;

            BeatmapCallbacksController beatmapCallbacksController = _beatmapCallbacksController;
            _callbacksInTimesAccessor(ref beatmapCallbacksController).Values.Do(n => n.lastProcessedNode = null);
            _prevSongTimeAccessor(ref beatmapCallbacksController) = 0;

            Rewinded?.Invoke();
        }

        private void SetSongTime(float songTime)
        {
            AudioTimeSyncController audioTimeSyncController = _audioTimeSyncController;
            _startSongTimeAccessor(ref audioTimeSyncController) = songTime;
            _audioTimeSyncController.SeekTo(0);
        }
    }
}
