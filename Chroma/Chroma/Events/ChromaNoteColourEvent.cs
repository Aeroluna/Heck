using Chroma.Extensions;
using Chroma.Utils;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaNoteColourEvent
    {
        internal static Dictionary<NoteType, List<TimedColor>> NoteColours = new Dictionary<NoteType, List<TimedColor>>();
        internal static Dictionary<INoteController, Color> SavedNoteColours = new Dictionary<INoteController, Color>();

        // Creates dictionary loaded with all _noteColor custom events and indexs them with the event's time
        internal static void Activate(List<CustomEventData> eventData)
        {
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    int id = (int)Trees.at(dynData, "_id");
                    Color c = ChromaUtils.GetColorFromData(dynData, false);

                    // Dictionary of dictionaries!
                    if (!NoteColours.TryGetValue((NoteType)id, out List<TimedColor> dictionaryID))
                    {
                        dictionaryID = new List<TimedColor>();
                        NoteColours.Add((NoteType)id, dictionaryID);
                    }
                    dictionaryID.Add(new TimedColor(d.time, c));

                    ColourManager.TechnicolourBlocksForceDisabled = true;
                }
                catch (Exception e)
                {
                    Logger.Log("INVALID CUSTOM EVENT", Logger.Level.WARNING);
                    Logger.Log(e);
                }
            }
        }

        internal static void SaberColour(NoteController noteController, NoteCutInfo noteCutInfo)
        {
            Color color;
            bool noteType = noteController.noteData.noteType == NoteType.NoteA;
            bool saberType = noteCutInfo.saberType == SaberType.SaberA;
            if (noteType == saberType)
            {
                if (ColourManager.TechnicolourBlocks && Settings.ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT)
                    color = noteType ? VFX.TechnicolourController.Instance.gradientLeftColor : VFX.TechnicolourController.Instance.gradientRightColor;
                else if (SavedNoteColours.TryGetValue(noteController, out Color c)) color = c;
                else
                {
                    Logger.Log("SavedNoteColour not found!", Logger.Level.WARNING);
                    return;
                }
                foreach (SaberColourizer saber in SaberColourizer.saberColourizers)
                {
                    if (saber.warm == noteType)
                    {
                        saber.Colourize(color);
                    }
                }
            }
        }
    }
}