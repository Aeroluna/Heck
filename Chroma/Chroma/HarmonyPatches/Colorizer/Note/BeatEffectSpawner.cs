namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJumpColorizer
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController, ref Color ____bombColorEffect)
        {
            if (noteController.noteData.colorType == ColorType.None)
            {
                if (noteController.TryGetBombColorizer(out BombColorizer bombColorizer))
                {
                    ____bombColorEffect = bombColorizer.Color.ColorWithAlpha(0.5f);
                }
            }
            else
            {
                ColorManagerColorForType.EnableColorOverride(noteController);
            }
        }

        private static void Postfix()
        {
            ColorManagerColorForType.DisableColorOverride();
        }
    }
}
