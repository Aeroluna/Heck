namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using static NoodleExtensions.Plugin;

    [HarmonyPatch(typeof(BadNoteCutEffectSpawner))]
    [HarmonyPatch("HandleNoteWasCut")]
    internal static class BadNoteCutEffectSpawnerHandleNoteWasCut
    {
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
