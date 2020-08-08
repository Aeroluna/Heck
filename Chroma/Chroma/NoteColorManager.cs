namespace Chroma
{
    using Chroma.Extensions;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;

    internal static class NoteColorManager
    {
        internal static Color? NoteColorOverride { get; private set; }

        internal static void EnableNoteColorOverride(NoteController noteController)
        {
            if (noteController.noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                NoteColorOverride = Trees.at(dynData, "color");
            }
        }

        internal static void DisableNoteColorOverride()
        {
            NoteColorOverride = null;
        }

        internal static void ColorizeSaber(INoteController noteController, NoteCutInfo noteCutInfo)
        {
            NoteData noteData = noteController.noteData;
            if ((int)noteData.noteType == (int)noteCutInfo.saberType && noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                Color? color = Trees.at(dynData, "color");

                if (color.HasValue)
                {
                    foreach (SaberColorizer saber in SaberColorizer.SaberColorizers)
                    {
                        if (saber.SaberType == noteCutInfo.saberType)
                        {
                            saber.Colorize(color.Value);
                        }
                    }
                }
            }
        }
    }
}
