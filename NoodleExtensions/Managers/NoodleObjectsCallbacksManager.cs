using System;
using System.Collections.Generic;
using System.Linq;
using Heck;
using JetBrains.Annotations;
using Zenject;

namespace NoodleExtensions.Managers
{
    [UsedImplicitly]
    internal class NoodleObjectsCallbacksManager : IDisposable
    {
        private readonly IReadonlyBeatmapData _beatmapData;
        private readonly SpawnDataManager _spawnDataManager;
        private readonly DeserializedData _deserializedData;
        private readonly float _startFilterTime;
        private readonly Reloader? _reloader;

        private readonly CallbacksInTime _callbacksInTime = new(0);

        private LinkedListNode<BeatmapDataItem>? _firstNode;
        private float _prevSongtime = float.MinValue;

        private NoodleObjectsCallbacksManager(
            BeatmapCallbacksController.InitData initData,
            SpawnDataManager spawnDataManager,
            [Inject(Id = NoodleController.ID)] DeserializedData deserializedData,
            [InjectOptional] Reloader? reloader)
        {
            _spawnDataManager = spawnDataManager;
            _deserializedData = deserializedData;
            _reloader = reloader;
            _beatmapData = initData.beatmapData;
            _startFilterTime = initData.startFilterTime;
            Init();

            if (reloader == null)
            {
                return;
            }

            reloader.Reloaded += OnReload;
            reloader.Rewinded += OnRewind;
        }

        public void Dispose()
        {
            if (_reloader == null)
            {
                return;
            }

            _reloader.Reloaded -= OnReload;
            _reloader.Rewinded -= OnRewind;
        }

        internal void ManualUpdate(float songTime)
        {
            if (songTime <= _prevSongtime)
            {
                return;
            }

            for (LinkedListNode<BeatmapDataItem>? linkedListNode = (_callbacksInTime.lastProcessedNode != null)
                    ? _callbacksInTime.lastProcessedNode.Next
                    : _firstNode;
                linkedListNode != null;
                linkedListNode = linkedListNode.Next)
            {
                BeatmapObjectData value2 = (BeatmapObjectData)linkedListNode.Value;
                if (!_deserializedData.Resolve(value2, out NoodleObjectData? noodleData))
                {
                    throw new InvalidOperationException($"Failed to get ahead time for [{value2.GetType()}] at [{value2.time}].");
                }

                if (value2.time - noodleData.InternalAheadTime > songTime)
                {
                    break;
                }

                float filterTime = value2 switch
                {
                    NoteData noteData => noteData.time,
                    ObstacleData obstacleData => obstacleData.time + obstacleData.duration,
                    SliderData sliderData => sliderData.tailTime,
                    _ => 0
                };

                if (value2.time >= _startFilterTime && songTime < filterTime)
                {
                    _callbacksInTime.CallCallbacks(value2);
                }

                _callbacksInTime.lastProcessedNode = linkedListNode;
            }

            _prevSongtime = songTime;
        }

        internal BeatmapDataCallbackWrapper AddBeatmapCallback<T>(float aheadTime, BeatmapDataCallback<T> callback)
            where T : BeatmapDataItem
        {
            BeatmapDataCallbackWrapper<T> beatmapDataCallbackWrapper = new(callback, aheadTime);
            _callbacksInTime.AddCallback(beatmapDataCallbackWrapper);
            return beatmapDataCallbackWrapper;
        }

        private void Init()
        {
            IEnumerable<BeatmapDataItem> objectDatas = _beatmapData.GetBeatmapDataItems<NoteData>()
                .Cast<BeatmapObjectData>()
                .Concat(_beatmapData.GetBeatmapDataItems<ObstacleData>())
                .OrderBy(beatmapObjectData =>
                {
                    if (!_deserializedData.Resolve(beatmapObjectData, out NoodleObjectData? noodleData))
                    {
                        throw new InvalidOperationException("Failed to get data.");
                    }

                    float? noteJumpMovementSpeed = noodleData.NJS;
                    float? noteJumpStartBeatOffset = noodleData.SpawnOffset;
                    float aheadTime = _spawnDataManager.GetSpawnAheadTime(noteJumpMovementSpeed, noteJumpStartBeatOffset);
                    noodleData.InternalAheadTime = aheadTime;
                    return beatmapObjectData.time - aheadTime;
                });

            _firstNode = new LinkedList<BeatmapDataItem>(objectDatas).First;
        }

        private void OnReload()
        {
            Init();
        }

        private void OnRewind()
        {
            _prevSongtime = 0;
            _callbacksInTime.lastProcessedNode = null;
        }
    }
}
