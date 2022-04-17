using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using Zenject;

namespace Chroma.HarmonyPatches
{
    internal class BeatEffectSpawnerSkip : IAffinity, IDisposable
    {
        private static readonly FieldInfo _hideNoteSpawnEffect =
            AccessTools.Field(typeof(BeatEffectSpawner.InitData), nameof(BeatEffectSpawner.InitData.hideNoteSpawnEffect));

        private readonly CustomData _customData;

        private readonly CodeInstruction _beatEffectForce;

        private BeatEffectSpawnerSkip([Inject(Id = ChromaController.ID)] CustomData customData)
        {
            _customData = customData;
            _beatEffectForce = InstanceTranspilers.EmitInstanceDelegate<Func<bool, NoteController, bool>>(BeatEffectForce);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_beatEffectForce);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(BeatEffectSpawner), nameof(BeatEffectSpawner.HandleNoteDidStartJump))]
        private IEnumerable<CodeInstruction> ReplaceConditionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _hideNoteSpawnEffect))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    _beatEffectForce)
                .InstructionEnumeration();
        }

        private bool BeatEffectForce(bool hideNoteSpawnEffect, NoteController noteController)
        {
            if (_customData.Resolve(noteController.noteData, out ChromaNoteData? chromaData) && chromaData.SpawnEffect.HasValue)
            {
                return !chromaData.SpawnEffect.Value;
            }

            return hideNoteSpawnEffect;
        }
    }
}
