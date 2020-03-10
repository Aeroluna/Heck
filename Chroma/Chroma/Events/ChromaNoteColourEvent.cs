using Chroma.Extensions;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaNoteColourEvent
    {
        public static Dictionary<NoteType, Dictionary<float, Color>> CustomNoteColours = new Dictionary<NoteType, Dictionary<float, Color>>();
        public static Dictionary<INoteController, Color> SavedNoteColours = new Dictionary<INoteController, Color>();

        // Creates dictionary loaded with all _noteColor custom events and indexs them with the event's time
        public static void Activate(List<CustomEventData> eventData)
        {
            if (!ChromaBehaviour.LightingRegistered) return;
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    int id = (int)Trees.at(dynData, "_id");
                    List<object> color = Trees.at(dynData, "_color");
                    float r = Convert.ToSingle(color[0]);
                    float g = Convert.ToSingle(color[1]);
                    float b = Convert.ToSingle(color[2]);

                    Color c = new Color(r, g, b);

                    // Dictionary of dictionaries!
                    Dictionary<float, Color> dictionaryID;
                    if (!CustomNoteColours.TryGetValue((NoteType)id, out dictionaryID))
                    {
                        dictionaryID = new Dictionary<float, Color>();
                        CustomNoteColours.Add((NoteType)id, dictionaryID);
                    }
                    dictionaryID.Add(d.time, c);

                    ColourManager.TechnicolourBlocksForceDisabled = true;
                }
                catch (Exception e)
                {
                    ChromaLogger.Log("INVALID CUSTOM EVENT", ChromaLogger.Level.WARNING);
                    ChromaLogger.Log(e);
                }
            }
        }

        public static void SaberColour(NoteController noteController, NoteCutInfo noteCutInfo)
        {
            Color color;
            bool noteType = noteController.noteData.noteType == NoteType.NoteA;
            bool saberType = noteCutInfo.saberType == Saber.SaberType.SaberA;
            if (noteType == saberType)
            {
                if (ColourManager.TechnicolourBlocks && Settings.ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT)
                    color = noteType ? VFX.TechnicolourController.Instance.gradientLeftColor : VFX.TechnicolourController.Instance.gradientRightColor;
                else if (SavedNoteColours.TryGetValue(noteController, out Color c)) color = c;
                else
                {
                    ChromaLogger.Log("SavedNoteColour not found!", ChromaLogger.Level.WARNING);
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

            // unsubscribe
            noteController.noteWasCutEvent -= SaberColour;
        }
    }
}