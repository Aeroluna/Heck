namespace Chroma.Events
{
    using System.Collections.Generic;
    using Chroma.Extensions;
    using UnityEngine;

    internal class ChromaNoteColourEvent
    {
        internal static Dictionary<INoteController, Color> SavedNoteColours { get; } = new Dictionary<INoteController, Color>();

        internal static void SaberColour(NoteController noteController, NoteCutInfo noteCutInfo)
        {
            Color color;
            bool noteType = noteController.noteData.noteType == NoteType.NoteA;
            bool saberType = noteCutInfo.saberType == SaberType.SaberA;
            if (noteType == saberType)
            {
                if (SavedNoteColours.TryGetValue(noteController, out Color c))
                {
                    color = c;
                }
                else
                {
                    ChromaLogger.Log("SavedNoteColour not found!", IPA.Logging.Logger.Level.Warning);
                    return;
                }

                foreach (SaberColourizer saber in SaberColourizer.SaberColourizers)
                {
                    if (saber.SaberType == noteCutInfo.saberType)
                    {
                        saber.Colourize(color);
                    }
                }
            }
        }
    }
}
