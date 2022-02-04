using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Chroma.Colorizer;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer
{
    internal class NoteEffectsColorize : IAffinity, IDisposable
    {
        private static readonly MethodInfo _colorForType = AccessTools.Method(typeof(ColorManager), nameof(ColorManager.ColorForType));

        private readonly CodeInstruction _getNoteColor;
        private readonly BombColorizerManager _bombManager;

        private NoteEffectsColorize(BombColorizerManager bombManager, NoteColorizerManager noteManager)
        {
            _bombManager = bombManager;
            _getNoteColor = InstanceTranspilers.EmitInstanceDelegate<Func<NoteController, Color>>(n => noteManager.GetColorizer(n).Color);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_getNoteColor);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteCutCoreEffectsSpawner), nameof(NoteCutCoreEffectsSpawner.SpawnNoteCutEffect))]
        private IEnumerable<CodeInstruction> NoteCutCoreEffectsSetNoteColor(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _colorForType))
                .Advance(-4)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2), _getNoteColor)
                .RemoveInstructions(5)
                .InstructionEnumeration();
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(BeatEffectSpawner), nameof(BeatEffectSpawner.HandleNoteDidStartJump))]
        private IEnumerable<CodeInstruction> BeatEffectSetNoteColor(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _colorForType))
                .Advance(-3)
                .SetOpcodeAndAdvance(OpCodes.Ldarg_1)
                .InsertAndAdvance(_getNoteColor)
                .RemoveInstructions(3)
                .InstructionEnumeration();
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatEffectSpawner), nameof(BeatEffectSpawner.HandleNoteDidStartJump))]
        private void BeatEffectSetBombColor(NoteController noteController, Color? __state, ref Color ____bombColorEffect)
        {
            if (noteController.noteData.colorType != ColorType.None)
            {
                return;
            }

            __state = ____bombColorEffect;
            ____bombColorEffect = _bombManager.GetColorizer(noteController).Color.ColorWithAlpha(0.5f);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatEffectSpawner), nameof(BeatEffectSpawner.HandleNoteDidStartJump))]
        private void BeatEffectResetBombColor(Color? __state, ref Color ____bombColorEffect)
        {
            if (__state.HasValue)
            {
                ____bombColorEffect = __state.Value;
            }
        }
    }
}
