using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes;

[HeckPatch(PatchType.Features)]
[HarmonyPatch(typeof(BeatmapObjectSpawnController))]
internal class InitializedSpawnMovementData
{
    // Moves initializition of BeatmapObjectSpawnMovementData from BeatmapObjectSpawnController.Start to the Zenject intializtion phase
    private InitializedSpawnMovementData(
        BeatmapObjectSpawnController.InitData initData,
        IJumpOffsetYProvider jumpOffsetYProvider,
#if LATEST
        BeatmapObjectSpawnController spawnController,
        IReadonlyBeatmapData beatmapData,
        IVariableMovementDataProvider variableMovementDataProvider)
    {
        BeatmapObjectSpawnMovementData beatmapObjectSpawnMovementData = spawnController.beatmapObjectSpawnMovementData;
        beatmapObjectSpawnMovementData.Init(
            initData.noteLinesCount,
            jumpOffsetYProvider,
            Vector3.right);

        float minRelativeNoteJumpSpeed = beatmapData
            .GetBeatmapDataItems<NoteJumpSpeedEventData>(0)
            .Aggregate(0.0f, (current, beatmapDataItem) => Mathf.Min(current, beatmapDataItem.relativeNoteJumpSpeed));
        variableMovementDataProvider.Init(
            beatmapObjectSpawnMovementData.startHalfJumpDurationInBeats,
            beatmapObjectSpawnMovementData.maxHalfJumpDistance,
            initData.noteJumpMovementSpeed,
            minRelativeNoteJumpSpeed,
            initData.beatsPerMinute,
            initData.noteJumpValueType,
            initData.noteJumpValue,
            beatmapObjectSpawnMovementData.centerPos,
            Vector3.forward);
#else
        IBeatmapObjectSpawnController spawnController)
    {
        spawnController.beatmapObjectSpawnMovementData.Init(
            initData.noteLinesCount,
            initData.noteJumpMovementSpeed,
            initData.beatsPerMinute,
            initData.noteJumpValueType,
            initData.noteJumpValue,
            jumpOffsetYProvider,
            Vector3.right,
            Vector3.forward);
#endif
        MovementData = spawnController.beatmapObjectSpawnMovementData;
    }

    internal BeatmapObjectSpawnMovementData MovementData { get; }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BeatmapObjectSpawnController.Start))]
    private static IEnumerable<CodeInstruction> RemoveInitTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- this._beatmapObjectSpawnMovementData.Init(this._initData.noteLinesCount, this._initData.noteJumpMovementSpeed, this._initData.beatsPerMinute, this._initData.noteJumpValueType, this._initData.noteJumpValue, this._jumpOffsetYProvider, Vector3.right, Vector3.forward);
             */
            .Start()
#if LATEST
            /*
             *
             * -- float num = 0f;
             * -- foreach (NoteJumpSpeedEventData noteJumpSpeedEventData in this._beatmapData.GetBeatmapDataItems<NoteJumpSpeedEventData>(0))
             * -- {
             * --     num = Mathf.Min(num, noteJumpSpeedEventData.relativeNoteJumpSpeed);
             * -- }
             * -- this._variableMovementDataProvider.Init(this._beatmapObjectSpawnMovementData.startHalfJumpDurationInBeats, this._beatmapObjectSpawnMovementData.maxHalfJumpDistance, this._initData.noteJumpMovementSpeed, num, this._initData.beatsPerMinute, this._initData.noteJumpValueType, this._initData.noteJumpValue, this._beatmapObjectSpawnMovementData.centerPos, Vector3.forward);
             */
            .RemoveInstructions(61)
#else
            .RemoveInstructions(22)
#endif
            .InstructionEnumeration();
    }
}
