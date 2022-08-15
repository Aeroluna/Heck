using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Chroma.Animation;
using Chroma.Colorizer;
using Chroma.Settings;
using HarmonyLib;
using Heck;
using Heck.Animation;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Colorizer
{
    internal class NoteObjectColorize : IAffinity, IDisposable
    {
        private readonly BombColorizerManager _bombManager;
        private readonly NoteColorizerManager _noteManager;
        private readonly DeserializedData _deserializedData;
        private readonly CodeInstruction _noteUpdateColorize;

        private NoteController? _noteController;

        private NoteObjectColorize(
            BombColorizerManager bombManager,
            NoteColorizerManager noteManager,
            [Inject(Id = ChromaController.ID)] DeserializedData deserializedData)
        {
            _bombManager = bombManager;
            _noteManager = noteManager;
            _deserializedData = deserializedData;
            _noteUpdateColorize = InstanceTranspilers.EmitInstanceDelegate<Action<float>>(NoteUpdateColorize);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_noteUpdateColorize);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BombNoteController), nameof(BombNoteController.Init))]
        private void BombColorize(BombNoteController __instance, NoteData noteData)
        {
            // They said it couldn't be done, they called me a madman
            if (_deserializedData.Resolve(noteData, out ChromaObjectData? chromaData))
            {
                _bombManager.Colorize(__instance, chromaData.Color);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameNoteController), nameof(GameNoteController.Init))]
        private void NoteColorize(GameNoteController __instance, NoteData noteData)
        {
            if (ChromaConfig.Instance.NoteColoringDisabled)
            {
                return;
            }

            if (_deserializedData.Resolve(noteData, out ChromaObjectData? chromaData))
            {
                _noteManager.Colorize(__instance, chromaData.Color);
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(NoteController), nameof(NoteController.ManualUpdate))]
        private void NoteUpdateSetData(NoteController __instance)
        {
            if (ChromaConfig.Instance.NoteColoringDisabled)
            {
                return;
            }

            _noteController = __instance;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(NoteFloorMovement), nameof(NoteFloorMovement.ManualUpdate))]
        private void NoteFloorMovementColorize() => NoteUpdateColorize(0);

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteJump), nameof(NoteJump.ManualUpdate))]
        private IEnumerable<CodeInstruction> NoteJumpColorize(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * float num2 = num / this._jumpDuration;
                 * ++ NoteUpdateColorize(num2);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_1))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    _noteUpdateColorize)
                .InstructionEnumeration();
        }

        private void NoteUpdateColorize(float time)
        {
            if (_noteController == null || !_deserializedData.Resolve(_noteController.noteData, out ChromaObjectData? chromaData))
            {
                return;
            }

            List<Track>? tracks = chromaData.Track;
            PointDefinition<Vector4>? pathPointDefinition = chromaData.LocalPathColor;
            if (tracks == null && pathPointDefinition == null)
            {
                return;
            }

            AnimationHelper.GetColorOffset(pathPointDefinition, tracks, time, out Color? colorOffset);

            if (!colorOffset.HasValue)
            {
                return;
            }

            Color color = colorOffset.Value;
            if (_noteController is BombNoteController)
            {
                _bombManager.Colorize(_noteController, color);
            }
            else
            {
                _noteManager.Colorize(_noteController, color);
            }
        }
    }
}
