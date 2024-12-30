using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing;

internal class ObjectCallbackAheadTimeReorder : IAffinity, IDisposable
{
    private static readonly ConstructorInfo _noteDataCtor = AccessTools.FirstConstructor(
        typeof(BeatmapDataCallback<NoteData>),
        _ => true);

    private static readonly ConstructorInfo _obstacleDataCtor =
        AccessTools.FirstConstructor(typeof(BeatmapDataCallback<ObstacleData>), _ => true);

    private static readonly ConstructorInfo _sliderDataCtor =
        AccessTools.FirstConstructor(typeof(BeatmapDataCallback<SliderData>), _ => true);

    private readonly CodeInstruction _addNoteCallback;

    private readonly CodeInstruction _addObstacleCallback;
    private readonly CodeInstruction _addSliderCallback;
    private readonly NoodleObjectsCallbacksManager _noodleObjectsCallbacksManager;

    private ObjectCallbackAheadTimeReorder(NoodleObjectsCallbacksManager noodleObjectsCallbacksManager)
    {
        _noodleObjectsCallbacksManager = noodleObjectsCallbacksManager;
        _addObstacleCallback =
            InstanceTranspilers
                .EmitInstanceDelegate<Func<float, BeatmapDataCallback<ObstacleData>, BeatmapDataCallbackWrapper>>(
                    (x, y) =>
                        _noodleObjectsCallbacksManager.AddBeatmapCallback(x, y));
        _addNoteCallback =
            InstanceTranspilers
                .EmitInstanceDelegate<Func<float, BeatmapDataCallback<NoteData>, BeatmapDataCallbackWrapper>>(
                    (x, y) =>
                        _noodleObjectsCallbacksManager.AddBeatmapCallback(x, y));
        _addSliderCallback =
            InstanceTranspilers
                .EmitInstanceDelegate<Func<float, BeatmapDataCallback<SliderData>, BeatmapDataCallbackWrapper>>(
                    (x, y) =>
                        _noodleObjectsCallbacksManager.AddBeatmapCallback(x, y));
    }

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_addObstacleCallback);
        InstanceTranspilers.DisposeDelegate(_addNoteCallback);
        InstanceTranspilers.DisposeDelegate(_addSliderCallback);
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(BeatmapCallbacksController), nameof(BeatmapCallbacksController.ManualUpdate))]
    private void UpdateNoodleCallback(float songTime)
    {
        _noodleObjectsCallbacksManager.ManualUpdate(songTime);
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(BeatmapObjectSpawnController), nameof(BeatmapObjectSpawnController.Start))]
    private IEnumerable<CodeInstruction> UseNoodleCallback(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- this._obstacleDataCallbackWrapper = this._beatmapCallbacksController.AddBeatmapCallback<ObstacleData>(this._beatmapObjectSpawnMovementData.spawnAheadTime, new BeatmapDataCallback<ObstacleData>(this.HandleObstacleDataCallback));
             * ++ this._obstacleDataCallbackWrapper = _noodleObjectsCallbacksManager.AddBeatmapCallback<ObstacleData>(this._beatmapObjectSpawnMovementData.spawnAheadTime, new BeatmapDataCallback<ObstacleData>(this.HandleObstacleDataCallback));
             * repeat for other matches
             */
            .MatchForward(false, new CodeMatch(OpCodes.Newobj, _obstacleDataCtor))
            .Advance(1)
            .RemoveInstruction()
            .Insert(_addObstacleCallback)
            .Advance(-8)
            .RemoveInstructions(2)

            .Start()
            .MatchForward(false, new CodeMatch(OpCodes.Newobj, _noteDataCtor))
            .Advance(1)
            .RemoveInstruction()
            .Insert(_addNoteCallback)
            .Advance(-8)
            .RemoveInstructions(2)

            .MatchForward(false, new CodeMatch(OpCodes.Newobj, _sliderDataCtor))
            .Advance(1)
            .RemoveInstruction()
            .Insert(_addSliderCallback)
            .Advance(-8)
            .RemoveInstructions(2)
            .InstructionEnumeration();
    }
}
