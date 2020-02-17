using Chroma.Extensions;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Beatmap.Events
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
                    float r = (float)Trees.at(dynData, "_r");
                    float g = (float)Trees.at(dynData, "_g");
                    float b = (float)Trees.at(dynData, "_b");
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
            if (SavedNoteColours.TryGetValue(noteController, out Color c)) color = c;
            else
            {
                ChromaLogger.Log("SavedNoteColour not found!", ChromaLogger.Level.WARNING);
                return;
            }
            foreach (SaberColourizer saber in SaberColourizer.saberColourizers)
            {
                if (saber.warm == (noteController.noteData.noteType == NoteType.NoteA))
                {
                    saber.Colourize(color);
                }
            }

            // unsubscribe
            noteController.noteWasCutEvent -= SaberColour;
        }
    }
}