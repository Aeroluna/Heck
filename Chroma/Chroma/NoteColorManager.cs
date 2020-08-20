namespace Chroma
{
    using Chroma.Extensions;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;

    internal static class NoteColorManager
    {
        internal static Color?[] NoteColorOverride { get; private set; } = new Color?[2] { null, null };

        internal static void EnableNoteColorOverride(NoteController noteController)
        {
            if (noteController.noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                NoteColorOverride[0] = Trees.at(dynData, "color0");
                NoteColorOverride[1] = Trees.at(dynData, "color1");
            }
        }

        internal static void DisableNoteColorOverride()
        {
            NoteColorOverride[0] = null;
            NoteColorOverride[1] = null;
        }

        internal static void ColorizeSaber(INoteController noteController, NoteCutInfo noteCutInfo)
        {
            NoteData noteData = noteController.noteData;
            if ((int)noteData.noteType == (int)noteCutInfo.saberType && noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                Color? color = Trees.at(dynData, "color" + (int)noteData.noteType);

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
