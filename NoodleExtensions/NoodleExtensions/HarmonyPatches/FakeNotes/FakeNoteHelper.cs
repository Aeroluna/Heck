namespace NoodleExtensions.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Plugin;

    internal static class FakeNoteHelper
    {
        internal static bool GetFakeNote(INoteController noteController)
        {
            if (noteController.noteData is CustomNoteData customNoteData)
            {
                dynamic dynData = customNoteData.customData;
                bool? fake = Trees.at(dynData, FAKENOTE);
                if (fake.HasValue && fake.Value)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool GetCuttable(NoteData noteData)
        {
            if (noteData is CustomNoteData customNoteData)
            {
                dynamic dynData = customNoteData.customData;
                bool? cuttable = Trees.at(dynData, CUTTABLE);
                if (cuttable.HasValue && cuttable.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
