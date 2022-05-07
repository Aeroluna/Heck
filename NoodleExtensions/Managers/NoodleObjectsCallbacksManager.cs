using System;
using System.Collections.Generic;
using System.Linq;
using Heck;
using JetBrains.Annotations;
using Zenject;

namespace NoodleExtensions.Managers
{
    [UsedImplicitly]
    internal class NoodleObjectsCallbacksManager
    {
        private readonly DeserializedData _deserializedData;
        private readonly float _startFilterTime;
        private readonly LinkedListNode<BeatmapDataItem>? _firstNode;

        private readonly CallbacksInTime _callbacksInTime = new(0);

        private float _prevSongtime = float.MinValue;

        private NoodleObjectsCallbacksManager(
            BeatmapCallbacksController.InitData initData,
            SpawnDataManager spawnDataManager,
            [Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
            IReadonlyBeatmapData beatmapData = initData.beatmapData;
            IEnumerable<BeatmapDataItem> objectDatas = beatmapData.GetBeatmapDataItems<NoteData>()
                .Cast<BeatmapObjectData>()
                .Concat(beatmapData.GetBeatmapDataItems<ObstacleData>())
                .OrderBy(beatmapObjectData =>
                {
                    if (!deserializedData.Resolve(beatmapObjectData, out NoodleObjectData? noodleData))
                    {
                        throw new InvalidOperationException("Failed to get data.");
                    }

                    float? noteJumpMovementSpeed = noodleData.NJS;
                    float? noteJumpStartBeatOffset = noodleData.SpawnOffset;
                    float aheadTime = spawnDataManager.GetSpawnAheadTime(noteJumpMovementSpeed, noteJumpStartBeatOffset);
                    noodleData.InternalAheadTime = aheadTime;
                    return beatmapObjectData.time - aheadTime;
                });

            _firstNode = new LinkedList<BeatmapDataItem>(objectDatas).First;
            _startFilterTime = initData.startFilterTime;
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

                if (value2.time >= _startFilterTime)
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
    }
}
