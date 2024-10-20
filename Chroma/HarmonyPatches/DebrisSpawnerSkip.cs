using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Deserialize;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches;

internal class DebrisSpawnerSkip : IAffinity, IDisposable
{
    private static readonly MethodInfo _noteSpawnDebris =
        AccessTools.Method(typeof(NoteDebrisSpawner), nameof(NoteDebrisSpawner.SpawnDebris));

    private static readonly MethodInfo _bombSpawnDebris =
        AccessTools.Method(typeof(BombExplosionEffect), nameof(BombExplosionEffect.SpawnExplosion));

    private readonly CodeInstruction _noteDebrisCheck;
    private readonly CodeInstruction _bombDebrisCheck;

    private readonly DeserializedData _deserializedData;

    private DebrisSpawnerSkip([Inject(Id = ChromaController.ID)] DeserializedData deserializedData)
    {
        _deserializedData = deserializedData;
        _noteDebrisCheck = InstanceTranspilers.EmitInstanceDelegate<NoteDebrisCheckDelegate>(NoteDebrisCheck);
        _bombDebrisCheck = InstanceTranspilers.EmitInstanceDelegate<BombDebrisCheckDelegate>(BombDebrisCheck);
    }

    private delegate void NoteDebrisCheckDelegate(
        NoteDebrisSpawner noteDebrisSpawner,
        NoteData.GameplayType noteGameplayType,
        Vector3 cutPoint,
        Vector3 cutNormal,
        float saberSpeed,
        Vector3 saberDir,
        Vector3 notePos,
        Quaternion noteRotation,
        Vector3 noteScale,
        ColorType colorType,
        float timeToNextColorNote,
        Vector3 moveVec,
        NoteController noteController);

    private delegate void BombDebrisCheckDelegate(
        BombExplosionEffect bombExplosionEffect,
        Vector3 cutPoint,
        NoteController noteController);

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_noteDebrisCheck);
    }

    // they couldve just passed NoteCutInfo and NoteController to SpawnDebris but i guess that would be too easy
    private void NoteDebrisCheck(
        NoteDebrisSpawner noteDebrisSpawner,
        NoteData.GameplayType noteGameplayType,
        Vector3 cutPoint,
        Vector3 cutNormal,
        float saberSpeed,
        Vector3 saberDir,
        Vector3 notePos,
        Quaternion noteRotation,
        Vector3 noteScale,
        ColorType colorType,
        float timeToNextColorNote,
        Vector3 moveVec,
        NoteController noteController)
    {
        if (_deserializedData.Resolve(noteController.noteData, out ChromaNoteData? chromaData) &&
            chromaData.DisableDebris.HasValue &&
            chromaData.DisableDebris.Value)
        {
            return;
        }

        noteDebrisSpawner.SpawnDebris(
            noteGameplayType,
            cutPoint,
            cutNormal,
            saberSpeed,
            saberDir,
            notePos,
            noteRotation,
            noteScale,
            colorType,
            timeToNextColorNote,
            moveVec);
    }

    private void BombDebrisCheck(BombExplosionEffect instance, Vector3 cutPoint, NoteController noteController)
    {
        if (_deserializedData.Resolve(noteController.noteData, out ChromaNoteData? chromaData) &&
            chromaData.DisableDebris.HasValue &&
            chromaData.DisableDebris.Value)
        {
            return;
        }

        instance.SpawnExplosion(cutPoint);
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(NoteCutCoreEffectsSpawner), nameof(NoteCutCoreEffectsSpawner.SpawnNoteCutEffect))]
    private IEnumerable<CodeInstruction> ReplaceNoteConditionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- this._noteDebrisSpawner.SpawnDebris(noteData.gameplayType, noteCutInfo.cutPoint, noteCutInfo.cutNormal, noteCutInfo.saberSpeed, noteCutInfo.saberDir,
             * ++ NoteDebrisCheck(_noteDebrisSpawner, noteData.gameplayType, noteCutInfo.cutPoint, noteCutInfo.cutNormal, noteCutInfo.saberSpeed, noteCutInfo.saberDir,
             */
            .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _noteSpawnDebris))
            .RemoveInstruction()
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_2),
                _noteDebrisCheck)
            .InstructionEnumeration();
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(NoteCutCoreEffectsSpawner), nameof(NoteCutCoreEffectsSpawner.SpawnBombCutEffect))]
    private IEnumerable<CodeInstruction> ReplaceBombConditionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- this._bombExplosionEffect.SpawnExplosion(cutPoint);
             * ++ BombDebrisCheck(cutPoint, noteController);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _bombSpawnDebris))
            .RemoveInstruction()
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_2),
                _bombDebrisCheck)
            .InstructionEnumeration();
    }
}
