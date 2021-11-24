using Chroma.Colorizer;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer.Note
{
    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJumpColorizer
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController, ref Color ____bombColorEffect)
        {
            if (noteController.noteData.colorType == ColorType.None)
            {
                ____bombColorEffect = noteController.GetBombColorizer().Color.ColorWithAlpha(0.5f);
            }
            else
            {
                ColorManagerColorForType.EnableColorOverride(noteController);
            }
        }

        [UsedImplicitly]
        private static void Postfix()
        {
            ColorManagerColorForType.DisableColorOverride();
        }
    }
}
