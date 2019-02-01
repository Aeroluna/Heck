using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap {

    public class CustomBeatmap {

        private BeatmapData _beatmapData = null;
        private List<CustomBeatmapObject> _customBeatmapData;

        public bool isTailored = false;

        public BeatmapData BeatmapData {
            get { return _beatmapData; }
            set { _beatmapData = value; }
        }

        public List<CustomBeatmapObject> CustomBeatmapObjects {
            get { return _customBeatmapData; }
        }

        public CustomBeatmap(List<CustomBeatmapObject> customBeatmapData) {
            this._customBeatmapData = customBeatmapData;
        }

        public CustomBeatmapObject GetCustomObject(int id) {
            for (int i = 0; i < _customBeatmapData.Count; i++) {
                if (_customBeatmapData[i].Data.id == id) return _customBeatmapData[i];
            }
            return null;
        }

        public CustomBeatmapObject GetCustomObject(BeatmapObjectData data) {
            return GetCustomObject(data.id);
        }

        public CustomBeatmapObject GetCustomObject(NoteData noteData) {
            return GetCustomObject(noteData.id);
        }

        public List<CustomBeatmapObject> GetCustomBeatmapObjectsByTime(float time) {
            List<CustomBeatmapObject> objs = new List<CustomBeatmapObject>();
            for (int i = 0; i < _customBeatmapData.Count; i++) {
                if (Mathf.Approximately(_customBeatmapData[i].Data.time, time)) objs.Add(_customBeatmapData[i]);
            }
            return objs;
        }

        public List<CustomBeatmapObject> GetCustomBeatmapObjectsWithinOffset(float time, float offset) {
            List<CustomBeatmapObject> objs = new List<CustomBeatmapObject>();
            for (int i = 0; i < _customBeatmapData.Count; i++) {
                if (Mathf.Abs(_customBeatmapData[i].Data.time - time) <= offset) objs.Add(_customBeatmapData[i]);
            }
            return objs;
        }

        public static CustomBeatmapNote[] PurgeNonNotes(List<CustomBeatmapObject> list) {
            List<CustomBeatmapNote> notes = new List<CustomBeatmapNote>();
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Data.beatmapObjectType == BeatmapObjectType.Note) {
                    if (list[i].Data is NoteData note) {
                        if (note.noteType == NoteType.NoteA || note.noteType == NoteType.NoteB) notes.Add(list[i] as CustomBeatmapNote);
                    }
                }
            }
            return notes.ToArray();
        }

    }

}
