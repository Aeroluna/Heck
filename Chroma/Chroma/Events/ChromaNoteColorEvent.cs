namespace Chroma.Events
{
    using System.Collections.Generic;
    using Chroma.Extensions;
    using UnityEngine;

    internal class ChromaNoteColorEvent
    {
        internal static Dictionary<INoteController, Color> SavedNoteColors { get; } = new Dictionary<INoteController, Color>();

        internal static void SaberColor(NoteController noteController, NoteCutInfo noteCutInfo)
        {
            Color color;
            bool noteType = noteController.noteData.noteType == NoteType.NoteA;
            bool saberType = noteCutInfo.saberType == SaberType.SaberA;
            if (noteType == saberType)
            {
                if (SavedNoteColors.TryGetValue(noteController, out Color c))
                {
                    color = c;
                }
                else
                {
                    ChromaLogger.Log("SavedNoteColor not found!", IPA.Logging.Logger.Level.Warning);
                    return;
                }

                foreach (SaberColorizer saber in SaberColorizer.SaberColorizers)
                {
                    if (saber.SaberType == noteCutInfo.saberType)
                    {
                        saber.Colorize(color);
                    }
                }
            }
        }
    }
}
