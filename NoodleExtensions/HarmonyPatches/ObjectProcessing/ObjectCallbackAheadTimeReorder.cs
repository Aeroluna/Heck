using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using NoodleExtensions.Extras;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    internal class ObjectCallbackAheadTimeReorder : IAffinity, IDisposable
    {
        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");

        private static readonly FieldInfo _aheadTimeField = AccessTools.Field(typeof(BeatmapObjectCallbackData), nameof(BeatmapObjectCallbackData.aheadTime));

        private static readonly MethodInfo _beatmapObjectSpawnControllerCallback = AccessTools.Method(typeof(BeatmapObjectSpawnController), nameof(BeatmapObjectSpawnController.HandleBeatmapObjectCallback));

        private readonly CodeInstruction _getAheadTime;
        private readonly SpawnDataManager _spawnDataManager;
        private readonly CustomData _customData;

        private ObjectCallbackAheadTimeReorder(
            SpawnDataManager spawnDataManager,
            [Inject(Id = NoodleController.ID)] CustomData customData)
        {
            _spawnDataManager = spawnDataManager;
            _customData = customData;
            _getAheadTime = InstanceTranspilers.EmitInstanceDelegate<Func<BeatmapObjectCallbackData, BeatmapObjectData, float, float>>(GetAheadTime);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_getAheadTime);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectCallbackController), nameof(BeatmapObjectCallbackController.SetNewBeatmapData))]
        private void ReorderLineData(IReadonlyBeatmapData beatmapData)
        {
            if (beatmapData is not CustomBeatmapData customBeatmapData)
            {
                throw new InvalidOperationException("beatmapData was not CustomBeatmapData.");
            }

            foreach (IReadonlyBeatmapLineData t in customBeatmapData.beatmapLinesData)
            {
                BeatmapLineData beatmapLineData = (BeatmapLineData)t;
                foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                {
                    Dictionary<string, object?> dynData = beatmapObjectData.GetDataForObject();

                    if (!_customData.Resolve(beatmapObjectData, out NoodleObjectData? noodleData))
                    {
                        throw new InvalidOperationException("Failed to get data.");
                    }

                    float? noteJumpMovementSpeed = noodleData.NJS;
                    float? noteJumpStartBeatOffset = noodleData.SpawnOffset;
                    float bpm = dynData.Get<float?>("bpm") ?? throw new InvalidOperationException("Bpm for object not found.");
                    noodleData.AheadTimeInternal = _spawnDataManager.GetSpawnAheadTime(noteJumpMovementSpeed, noteJumpStartBeatOffset, bpm);
                }

                _beatmapObjectsDataAccessor(ref beatmapLineData) = beatmapLineData.beatmapObjectsData
                    .OrderBy(n =>
                    {
                        if (_customData.Resolve(n, out NoodleObjectData? noodleData))
                        {
                            return n.time - noodleData.AheadTimeInternal;
                        }

                        throw new InvalidOperationException("Failed to get data.");
                    })
                    .ToList();
            }
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(BeatmapObjectCallbackController), nameof(BeatmapObjectCallbackController.LateUpdate))]
        private IEnumerable<CodeInstruction> UseAheadTimeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _aheadTimeField))
                .Advance(-1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_3))
                .Advance(2)
                .Insert(_getAheadTime)
                .InstructionEnumeration();
        }

        private float GetAheadTime(BeatmapObjectCallbackData beatmapObjectCallbackData, BeatmapObjectData beatmapObjectData, float @default)
        {
            if (beatmapObjectCallbackData.callback.Method == _beatmapObjectSpawnControllerCallback &&
                beatmapObjectData is CustomObstacleData or CustomNoteData &&
                _customData.Resolve(beatmapObjectData, out NoodleObjectData? noodleObjectData) &&
                noodleObjectData.AheadTimeInternal.HasValue)
            {
                return noodleObjectData.AheadTimeInternal.Value;
            }

            return @default;
        }
    }
}
