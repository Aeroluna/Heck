using Chroma.Colorizer;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer
{
    internal class NoteEffectsColorize : IAffinity
    {
        private readonly BombColorizerManager _bombManager;
        private readonly NoteColorizerManager _noteManager;

        private Color? _noteColorOverride;

        private NoteEffectsColorize(BombColorizerManager bombManager, NoteColorizerManager noteManager)
        {
            _bombManager = bombManager;
            _noteManager = noteManager;
        }

        internal void EnableColorOverride(NoteControllerBase noteController)
        {
            _noteColorOverride = _noteManager.GetColorizer(noteController).Color;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ColorManager), nameof(ColorManager.ColorForType))]
        private bool UseChromaColor(ref Color __result)
        {
            Color? color = _noteColorOverride;
            if (!color.HasValue)
            {
                return true;
            }

            __result = color.Value;
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(NoteCutCoreEffectsSpawner), nameof(NoteCutCoreEffectsSpawner.SpawnNoteCutEffect))]
        private void NoteCutCoreEffectsSetColor(NoteController noteController)
        {
            EnableColorOverride(noteController);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(NoteCutCoreEffectsSpawner), nameof(NoteCutCoreEffectsSpawner.SpawnNoteCutEffect))]
        private void NoteCutCoreEffectsResetColor(NoteController noteController)
        {
            _noteColorOverride = null;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatEffectSpawner), nameof(BeatEffectSpawner.HandleNoteDidStartJump))]
        private void BeatEffectSetColor(NoteController noteController, Color? __state, ref Color ____bombColorEffect)
        {
            if (noteController.noteData.colorType != ColorType.None)
            {
                EnableColorOverride(noteController);
                return;
            }

            __state = ____bombColorEffect;
            ____bombColorEffect = _bombManager.GetColorizer(noteController).Color.ColorWithAlpha(0.5f);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatEffectSpawner), nameof(BeatEffectSpawner.HandleNoteDidStartJump))]
        private void BeatEffectResetColor(Color? __state, ref Color ____bombColorEffect)
        {
            _noteColorOverride = null;
            if (__state.HasValue)
            {
                ____bombColorEffect = __state.Value;
            }
        }
    }
}
