using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches
{
    internal class DebrisSpawnerSkip : IAffinity, IDisposable
    {
        private static readonly MethodInfo _spawnDebris =
            AccessTools.Method(typeof(NoteDebrisSpawner), nameof(NoteDebrisSpawner.SpawnDebris));

        private readonly DeserializedData _deserializedData;

        private readonly CodeInstruction _debrisCheck;

        private DebrisSpawnerSkip([Inject(Id = ChromaController.ID)] DeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
            _debrisCheck = InstanceTranspilers.EmitInstanceDelegate<DebrisCheckDelegate>(DebrisCheck);
        }

        private delegate void DebrisCheckDelegate(
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

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_debrisCheck);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteCutCoreEffectsSpawner), nameof(NoteCutCoreEffectsSpawner.SpawnNoteCutEffect))]
        private IEnumerable<CodeInstruction> ReplaceConditionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- this._noteDebrisSpawner.SpawnDebris(noteData.gameplayType, noteCutInfo.cutPoint, noteCutInfo.cutNormal, noteCutInfo.saberSpeed, noteCutInfo.saberDir,
                 * ++ DebrisCheck(_noteDebrisSpawner, noteData.gameplayType, noteCutInfo.cutPoint, noteCutInfo.cutNormal, noteCutInfo.saberSpeed, noteCutInfo.saberDir,
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _spawnDebris))
                .RemoveInstruction()
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_2),
                    _debrisCheck)
                .InstructionEnumeration();
        }

        // they couldve just passed NoteCutInfo and NoteController to SpawnDebris but i guess that would be too easy
        private void DebrisCheck(
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
            if (_deserializedData.Resolve(noteController.noteData, out ChromaNoteData? chromaData) && chromaData.DisableDebris.HasValue && chromaData.DisableDebris.Value)
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
    }
}
