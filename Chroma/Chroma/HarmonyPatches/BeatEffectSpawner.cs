namespace Chroma.HarmonyPatches
{
    using Chroma.Events;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJumpEvent")]
    internal class HandleNoteDidStartJumpEvent
    {
        private static bool Prefix(NoteController noteController)
        {
            if (ChromaController.LightingRegistered && noteController.noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                bool? disable = Trees.at(dynData, "_disableSpawnEffect");
                if (disable.HasValue && disable == true)
                {
                    return false;
                }
            }

            NoteColorManager.EnableNoteColorOverride(noteController);
            return true;
        }

        private static void Postfix()
        {
            NoteColorManager.DisableNoteColorOverride();
        }
    }
}
